using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using ProyectoWebFinal.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using ProyectoWebFinal.DTOs;

namespace ProyectoWebFinal.Controllers
{
    public class RespuestasController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly HttpClient _httpClient;

        public RespuestasController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5159");
        }

        // ==========================================
        // 1️⃣ Página principal: Listar todas las respuestas (Con Paginación)
        // ==========================================
        public async Task<IActionResult> Respuestas(int pagina = 1)
        {
            try
            {
                int cantidad = 50;
                // ✅ Petición a la API con parámetros de paginación
                var response = await _httpClient.GetAsync($"api/RespuestasApi?pagina={pagina}&cantidad={cantidad}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var respuestas = JsonSerializer.Deserialize<List<RespuestaDTO>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // ✅ Enviar datos a la vista para los botones de navegación
                ViewBag.PaginaActual = pagina;

                return View(respuestas);
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"❌ Error al conectar con la API: {ex.Message}";
                return View(new List<RespuestaDTO>());
            }
        }

        // ==========================================
        // 2️⃣ Eliminar encuestas (Con Paginación)
        // ==========================================
        public async Task<IActionResult> RespuestasEliminar(int pagina = 1)
        {
            int cantidad = 50;
            var response = await _httpClient.GetAsync($"api/RespuestasApi?pagina={pagina}&cantidad={cantidad}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<List<RespuestaDTO>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ViewBag.PaginaActual = pagina;
            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarEliminar(int numero)
        {
            var response = await _httpClient.DeleteAsync($"api/RespuestasApi/{numero}");
            TempData["Mensaje"] = response.IsSuccessStatusCode
                ? "✅ Encuesta eliminada correctamente"
                : "❌ Error al eliminar la encuesta";

            return RedirectToAction("RespuestasEditar");
        }

        // ==========================================
        // 3️⃣ Editar encuestas (Con Paginación)
        // ==========================================
        public async Task<IActionResult> RespuestasEditar(int pagina = 1)
        {
            int cantidad = 50;
            var response = await _httpClient.GetAsync($"api/RespuestasApi?pagina={pagina}&cantidad={cantidad}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<List<RespuestaDTO>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ViewBag.PaginaActual = pagina;
            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> RespuestasEditar(RespuestaDTO respuesta)
        {
            var json = JsonSerializer.Serialize(respuesta);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/RespuestasApi/{respuesta.Numero}", content);
            TempData["Mensaje"] = response.IsSuccessStatusCode
                ? "✅ Encuesta actualizada correctamente"
                : "❌ Error al actualizar la encuesta";

            return RedirectToAction("RespuestasEditar");
        }

        // ==========================================
        // 4️⃣ Exportación (Vista con Paginación)
        // ==========================================
        public async Task<IActionResult> RespuestasExportar(int pagina = 1)
        {
            try
            {
                int cantidad = 50;
                var response = await _httpClient.GetAsync($"api/RespuestasApi?pagina={pagina}&cantidad={cantidad}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var respuestasExportar = JsonSerializer.Deserialize<List<RespuestaDTO>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                ViewBag.PaginaActual = pagina;
                return View(respuestasExportar);
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"❌ Error al conectar con la API: {ex.Message}";
                return View(new List<RespuestaDTO>());
            }
        }

        // ==========================================
        // 5️⃣ Métodos de Descarga Directa (Sin Paginación)
        // ==========================================
        public async Task<IActionResult> ExportarPDF(int id)
        {
            var response = await _httpClient.GetAsync($"api/RespuestasApi/exportarpdf/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "❌ No se pudo generar el PDF";
                return RedirectToAction("RespuestasExportar");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return File(bytes, "application/pdf", $"Encuesta_{id}.pdf");
        }

        public async Task<IActionResult> ExportarExcel(int id)
        {
            var response = await _httpClient.GetAsync($"api/RespuestasApi/exportarexcel/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "❌ No se pudo generar el Excel";
                return RedirectToAction("RespuestasExportar");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return File(bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Encuesta_{id}.xlsx");
        }

        // Nota: Estos dos métodos son los que se beneficiarán del aumento de Timeout a 120s 
        // en Program.cs, ya que intentarán descargar TODA la base de datos a la vez.
        public async Task<IActionResult> ExportarTodasPDF()
        {
            var response = await _httpClient.GetAsync("api/RespuestasApi/exportartodospdf");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "❌ No se pudo generar el PDF";
                return RedirectToAction("RespuestasExportar");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return File(bytes, "application/pdf", "Todas_Encuestas.pdf");
        }

        public async Task<IActionResult> ExportarTodasExcel()
        {
            var response = await _httpClient.GetAsync("api/RespuestasApi/exportartodosexcel");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "❌ No se pudo generar el Excel";
                return RedirectToAction("RespuestasExportar");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return File(bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Todas_Encuestas.xlsx");
        }
    }
}