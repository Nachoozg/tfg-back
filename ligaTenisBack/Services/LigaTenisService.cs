using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ligaTenisBack.Dtos;
using ligaTenisBack.Models;
using ligaTenisBack.Models.DbModels;  // para la clase Clasificacion

namespace ligaTenisBack.Services
{
    /// <summary>
    /// Servicio que consume tus endpoints GET para
    /// exponerlos como funciones del chat.
    /// </summary>
    public class LigaTenisService
    {
        private readonly HttpClient _http;

        public LigaTenisService(IHttpClientFactory factory)
            => _http = factory.CreateClient("ApiInterna");

        /// <summary>1) Devuelve todos los colegios.</summary>
        public async Task<object> ListarColegiosAsync()
            => await _http
                 .GetFromJsonAsync<List<ColegioDto>>("api/Colegio")
               ?? new List<ColegioDto>();

        /// <summary>
        /// 2) Devuelve todos los partidos;
        ///    si soloFuturos==true, filtra los que aún no se han jugado.
        /// </summary>
        public async Task<object> ListarPartidosAsync(bool soloFuturos)
        {
            var todos = await _http
                .GetFromJsonAsync<List<PartidoDto>>("api/Partido")
              ?? new List<PartidoDto>();

            if (soloFuturos)
            {
                var hoy = DateOnly.FromDateTime(DateTime.Today);
                todos = todos
                    .Where(p => p.Fecha >= hoy)
                    .OrderBy(p => p.Fecha)
                    .ToList();
            }

            return todos;
        }

        /// <summary>
        /// 3) Devuelve jugadores.
        ///    Si colegioId tiene valor, llama a GET api/Jugador/colegio/{colegioId},
        ///    si no, devuelve todos.
        /// </summary>
        public async Task<object> ListarJugadoresAsync(int? colegioId)
        {
            if (colegioId.HasValue)
            {
                return await _http
                    .GetFromJsonAsync<List<JugadorDto>>(
                        $"api/Jugador/colegio/{colegioId.Value}"
                    )
                  ?? new List<JugadorDto>();
            }
            else
            {
                return await _http
                    .GetFromJsonAsync<List<JugadorDto>>("api/Jugador")
                  ?? new List<JugadorDto>();
            }
        }

        /// <summary>
        /// 4) Devuelve la clasificación actual de la liga.
        /// </summary>
        public async Task<object> ObtenerClasificacionAsync()
            => await _http
                 .GetFromJsonAsync<List<Clasificacion>>("api/Clasificacion")
               ?? new List<Clasificacion>();
    }
}
