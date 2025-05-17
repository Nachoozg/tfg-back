using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ligaTenisBack.Dtos;
using ligaTenisBack.Models.DbModels;

namespace ligaTenisBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly HttpClient _http;
        private readonly GenerativeModel _ai;

        public ChatController(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _http = httpFactory.CreateClient("ApiInterna");

            var gemini = config.GetSection("Gemini");
            var apiKey = gemini["Credentials:ApiKey"]
                          ?? throw new InvalidOperationException("No está configurada la ApiKey de Gemini.");
            var modelId = gemini["Model"] ?? "gemini-2.0-flash";

            _ai = new GoogleAI(apiKey: apiKey).GenerativeModel(model: modelId);
        }

        private static readonly string SystemPrompt = @"
            Eres LigaIA, el asistente virtual de la Liga de Tenis para Colegios de Burgos.
            - Sé breve y directo: no uses saludos ni introducciones largas.
            - Tono cordial y profesional.
            - Listas: un ítem por línea, precedido por ""- "".
            - Varía siempre la apertura de tus respuestas.
            - Responde SOLO sobre colegios, partidos, jugadores y clasificación.
            - Idioma: español.
            ".Trim();

        private static string Simplify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (var ch in normalized)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            return sb.ToString().ToLowerInvariant();
        }

        private static string MakePrompt(string datos, string pregunta) =>
            $@"{SystemPrompt}

                Datos:
                {datos}

                Usuario pregunta: ""{pregunta}""

                Responde de forma natural y variada, siguiendo las reglas.";

        public record ChatRequest(string Message);
        public record ChatResponse(string Reply);

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest req)
        {
            var pregunta = (req.Message ?? "").Trim();
            var texto = Simplify(pregunta);

            // --- 1) PREDICCIÓN DE PARTIDO ---
            if (Regex.IsMatch(texto, @"\b(predicc|probabil|crees?|va a qued|quedara|terminar)"))
            {
                var todos = await _http.GetFromJsonAsync<List<PartidoDto>>("api/Partido")
                             ?? new List<PartidoDto>();
                var futuros = todos
                    .Where(p => p.Fecha >= DateOnly.FromDateTime(DateTime.Today))
                    .OrderBy(p => p.Fecha)
                    .ToList();

                if (!futuros.Any())
                    return Ok(new ChatResponse("No hay partidos venideros para predecir."));

                if (futuros.Count > 1)
                    return Ok(new ChatResponse(
                        "Hay varios partidos próximos. ¿Podrías indicar cuál (p.ej. “La Salle vs Concepcionistas”)?"));

                var partido = futuros.First();
                var idA = partido.LocalId ?? -1;
                var idB = partido.VisitanteId ?? -1;
                var pasados = todos
                    .Where(p => p.Fecha < DateOnly.FromDateTime(DateTime.Today))
                    .ToList();

                double CalculoRatioPartidos(int eq)
                {
                    var ult10 = pasados
                        .Where(p => p.LocalId == eq || p.VisitanteId == eq)
                        .OrderByDescending(p => p.Fecha)
                        .Take(10)
                        .ToList();
                    if (!ult10.Any()) return 0.5;
                    var victorias = ult10.Count(p =>
                        (p.LocalId == eq && p.ResultadoLocal > p.ResultadoVisitante) ||
                        (p.VisitanteId == eq && p.ResultadoVisitante > p.ResultadoLocal));
                    return victorias / (double)ult10.Count;
                }

                double CalculoRatioJuegos(int eq)
                {
                    var ult10 = pasados
                        .Where(p => p.LocalId == eq || p.VisitanteId == eq)
                        .OrderByDescending(p => p.Fecha)
                        .Take(10)
                        .ToList();
                    if (!ult10.Any()) return 0.5;
                    var ganados = ult10.Sum(p => p.LocalId == eq
                            ? p.ResultadoLocal.GetValueOrDefault()
                            : p.ResultadoVisitante.GetValueOrDefault());
                    var tot = ult10.Sum(p =>
                            p.ResultadoLocal.GetValueOrDefault() +
                            p.ResultadoVisitante.GetValueOrDefault());
                    return tot == 0 ? 0.5 : ganados / (double)tot;
                }

                var mA = CalculoRatioPartidos(idA);
                var mB = CalculoRatioPartidos(idB);
                var gA = CalculoRatioJuegos(idA);
                var gB = CalculoRatioJuegos(idB);

                var enfrentados = pasados
                    .Where(p => (p.LocalId == idA && p.VisitanteId == idB) ||
                                (p.LocalId == idB && p.VisitanteId == idA))
                    .OrderByDescending(p => p.Fecha)
                    .Take(5)
                    .ToList();
                var enfrentadosA = enfrentados.Any()
                    ? enfrentados.Count(p =>
                        (p.LocalId == idA && p.ResultadoLocal > p.ResultadoVisitante) ||
                        (p.VisitanteId == idA && p.ResultadoVisitante > p.ResultadoLocal))
                      / (double)enfrentados.Count
                    : 0.5;

                var probA = (mA * .4 + gA * .3 + enfrentadosA * .3) * 100;
                var probB = 100 - probA;

                var colegios = await _http.GetFromJsonAsync<List<ColegioDto>>("api/Colegio")
                                ?? new List<ColegioDto>();
                var mapa = colegios.ToDictionary(c => c.Id, c => c.Nombre);

                var reply = $@"
                    Basándome en:
                    - Win‐rate partidos (10): {mapa[idA]} {(int)Math.Round(mA * 100)}% vs {mapa[idB]} {(int)Math.Round(mB * 100)}%
                    - % juegos ganados (10):  {mapa[idA]} {(int)Math.Round(gA * 100)}% vs {mapa[idB]} {(int)Math.Round(gB * 100)}%
                    - Cara a cara (5):        {mapa[idA]} {(int)Math.Round(enfrentadosA * 100)}% vs {mapa[idB]} {(int)Math.Round((1 - enfrentadosA) * 100)}%

                    Probabilidades de victoria:
                    - {mapa[idA]}: {(int)Math.Round(probA)}%
                    - {mapa[idB]}: {(int)Math.Round(probB)}%";

                return Ok(new ChatResponse(reply.Trim()));
            }

            // --- 2) COLEGIOS ---
            if (Regex.IsMatch(texto, @"\bcolegi"))
            {
                var lista = await _http.GetFromJsonAsync<List<ColegioDto>>("api/Colegio")
                            ?? new List<ColegioDto>();
                var datos = lista.Any()
                    ? string.Join("\n- ", lista.Select(c => c.Nombre))
                    : "- No hay colegios registrados";

                var prompt = MakePrompt(datos, pregunta);
                var aiResp = await _ai.GenerateContent(prompt);
                return Ok(new ChatResponse(aiResp.Text.Trim()));
            }

            // --- 3) PARTIDOS ---
            if (Regex.IsMatch(texto, @"\bpartid"))
            {
                bool soloFuturos = texto.Contains("prox");
                var todos = await _http.GetFromJsonAsync<List<PartidoDto>>("api/Partido")
                           ?? new List<PartidoDto>();
                var lista = soloFuturos
                    ? todos.Where(p => p.Fecha >= DateOnly.FromDateTime(DateTime.Today))
                           .OrderBy(p => p.Fecha)
                           .ToList()
                    : todos;

                if (!lista.Any())
                    return Ok(new ChatResponse("No hay partidos para ese criterio."));

                var colegios = await _http.GetFromJsonAsync<List<ColegioDto>>("api/Colegio")
                               ?? new List<ColegioDto>();
                var mapa = colegios.ToDictionary(c => c.Id, c => c.Nombre);

                var lineas = lista.Select(p =>
                    $"- {mapa.GetValueOrDefault(p.LocalId ?? -1, "Local?")} vs " +
                    $"{mapa.GetValueOrDefault(p.VisitanteId ?? -1, "Visitante?")} el {p.Fecha:dd/MM/yyyy}");
                var datos = string.Join("\n", lineas);

                var prompt = MakePrompt(datos, pregunta);
                var aiResp = await _ai.GenerateContent(prompt);
                return Ok(new ChatResponse(aiResp.Text.Trim()));
            }

            // --- 4) JUGADORES ---
            if (Regex.IsMatch(texto, @"\bjugador"))
            {
                List<JugadorDto> jugadores;
                var match = Regex.Match(texto, @"\bcolegio\s+(.+)$");
                if (match.Success)
                {
                    var buscado = Simplify(match.Groups[1].Value);
                    var colList = await _http.GetFromJsonAsync<List<ColegioDto>>("api/Colegio")
                                  ?? new List<ColegioDto>();
                    var mapC = colList.ToDictionary(c => Simplify(c.Nombre), c => c.Id);

                    if (mapC.TryGetValue(buscado, out var idCole))
                    {
                        jugadores = await _http.GetFromJsonAsync<List<JugadorDto>>($"api/Jugador/colegio/{idCole}")
                                      ?? new List<JugadorDto>();
                    }
                    else
                    {
                        return Ok(new ChatResponse($"No encontré el colegio “{match.Groups[1].Value}”."));
                    }
                }
                else
                {
                    jugadores = await _http.GetFromJsonAsync<List<JugadorDto>>("api/Jugador")
                                  ?? new List<JugadorDto>();
                }

                if (!jugadores.Any())
                    return Ok(new ChatResponse("No hay jugadores para ese criterio."));

                var datos = string.Join("\n- ", jugadores.Select(j => $"{j.Nombre} {j.Apellidos}"));
                var prompt = MakePrompt(datos, pregunta);
                var aiResp = await _ai.GenerateContent(prompt);
                return Ok(new ChatResponse(aiResp.Text.Trim()));
            }

            // --- 5) CLASIFICACIÓN ---
            if (Regex.IsMatch(texto, @"\bclasificaci"))
            {
                var tabla = await _http.GetFromJsonAsync<List<Clasificacion>>("api/Clasificacion")
                         ?? new List<Clasificacion>();
                if (!tabla.Any())
                    return Ok(new ChatResponse("No hay datos de clasificación."));

                var lineas = tabla.Select(c => $"- {c.NombreEquipo}: {c.Puntos} pts ({c.Victorias}V/{c.Derrotas}D)");
                var datos = string.Join("\n", lineas);

                var prompt = MakePrompt(datos, pregunta);
                var aiResp = await _ai.GenerateContent(prompt);
                return Ok(new ChatResponse(aiResp.Text.Trim()));
            }

            // --- 6) FALLBACK ---
            var fallPrompt = MakePrompt("—", pregunta);
            var fallResp = await _ai.GenerateContent(fallPrompt);
            return Ok(new ChatResponse(fallResp.Text.Trim()));
        }
    }
}
