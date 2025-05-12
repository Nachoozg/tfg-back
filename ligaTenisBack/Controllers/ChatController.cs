using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
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

            var googleAi = new GoogleAI(apiKey: apiKey);
            _ai = googleAi.GenerativeModel(model: modelId);
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

            if (Regex.IsMatch(texto, @"\bcolegi"))
            {
                var lista = await _http.GetFromJsonAsync<List<ColegioDto>>("api/Colegio")
                            ?? new();
                var datos = lista.Any()
                    ? string.Join("\n- ", lista.Select(c => c.Nombre))
                    : "- No hay colegios registrados";

                var prompt = MakePrompt(datos, pregunta);
                var aiResp = await _ai.GenerateContent(prompt);
                return Ok(new ChatResponse(aiResp.Text.Trim()));
            }

            if (Regex.IsMatch(texto, @"\bpartid"))
            {
                bool soloFuturos = texto.Contains("prox");
                var todos = await _http.GetFromJsonAsync<List<PartidoDto>>("api/Partido")
                           ?? new();
                var lista = soloFuturos
                    ? todos.Where(p => p.Fecha >= DateOnly.FromDateTime(DateTime.Today))
                           .OrderBy(p => p.Fecha)
                           .ToList()
                    : todos;

                if (!lista.Any())
                    return Ok(new ChatResponse("No hay partidos para ese criterio."));

                var colegios = await _http.GetFromJsonAsync<List<ColegioDto>>("api/Colegio")
                               ?? new();
                var mapa = colegios.ToDictionary(c => c.Id, c => c.Nombre);

                var lineas = lista
                    .Select(p => $"- {mapa.GetValueOrDefault(p.LocalId ?? -1, "Local?")} vs " +
                                 $"{mapa.GetValueOrDefault(p.VisitanteId ?? -1, "Visitante?")} el {p.Fecha:dd/MM/yyyy}");
                var datos = string.Join("\n", lineas);

                var prompt = MakePrompt(datos, pregunta);
                var aiResp = await _ai.GenerateContent(prompt);
                return Ok(new ChatResponse(aiResp.Text.Trim()));
            }

            if (Regex.IsMatch(texto, @"\bjugador"))
            {
                List<JugadorDto> jugadores;
                var match = Regex.Match(texto, @"\bcolegio\s+(.+)$");
                if (match.Success)
                {
                    var buscado = Simplify(match.Groups[1].Value);
                    var colList = await _http.GetFromJsonAsync<List<ColegioDto>>("api/Colegio")
                                  ?? new();
                    var mapC = colList.ToDictionary(c => Simplify(c.Nombre), c => c.Id);

                    if (mapC.TryGetValue(buscado, out var idCole))
                    {
                        jugadores = await _http
                            .GetFromJsonAsync<List<JugadorDto>>($"api/Jugador/colegio/{idCole}")
                            ?? new();
                    }
                    else
                    {
                        return Ok(new ChatResponse($"No encontré el colegio “{match.Groups[1].Value}”."));
                    }
                }
                else
                {
                    jugadores = await _http.GetFromJsonAsync<List<JugadorDto>>("api/Jugador")
                                ?? new();
                }

                if (!jugadores.Any())
                    return Ok(new ChatResponse("No hay jugadores para ese criterio."));

                var datos = string.Join("\n- ",
                    jugadores.Select(j => $"{j.Nombre} {j.Apellidos}"));

                var prompt = MakePrompt(datos, pregunta);
                var aiResp = await _ai.GenerateContent(prompt);
                return Ok(new ChatResponse(aiResp.Text.Trim()));
            }

            if (Regex.IsMatch(texto, @"\bclasificaci"))
            {
                var tabla = await _http.GetFromJsonAsync<List<Clasificacion>>("api/Clasificacion")
                         ?? new();
                if (!tabla.Any())
                    return Ok(new ChatResponse("No hay datos de clasificación."));

                var lineas = tabla
                    .Select(c => $"- {c.NombreEquipo}: {c.Puntos} pts ({c.Victorias}V/{c.Derrotas}D)");
                var datos = string.Join("\n", lineas);

                var prompt = MakePrompt(datos, pregunta);
                var aiResp = await _ai.GenerateContent(prompt);
                return Ok(new ChatResponse(aiResp.Text.Trim()));
            }

            var fallPrompt = MakePrompt("—", pregunta);
            var fallResp = await _ai.GenerateContent(fallPrompt);
            return Ok(new ChatResponse(fallResp.Text.Trim()));
        }
    }
}
