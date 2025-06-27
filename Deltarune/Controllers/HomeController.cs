using Deltarune.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace Deltarune.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;

            // Crea y configura el cliente HTTP para Supabase
            _httpClient = httpClientFactory.CreateClient();

            // Obtiene configuración desde appsettings.json
            string url = _configuration["Supabase:Url"] ?? "";
            string key = _configuration["Supabase:Key"] ?? "";

            // Asegura que la URL termine con '/' para formar rutas correctamente
            if (!string.IsNullOrEmpty(url) && !url.EndsWith("/"))
            {
                url += "/";
            }

            // Configura la URL base y headers de autenticación
            if (!string.IsNullOrEmpty(url))
            {
                _httpClient.BaseAddress = new Uri(url);
            }
            else
            {
                _logger.LogWarning("La URL de Supabase no está configurada en appsettings.json");
            }
            _httpClient.DefaultRequestHeaders.Add("apikey", key);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Intentando verificar la conexión a Supabase");

                // Verifica que la URL base esté configurada
                if (_httpClient.BaseAddress == null)
                {
                    ViewBag.Error = "Error: La URL de Supabase no está configurada correctamente";
                    return View();
                }

                // Realiza una petición de prueba a la API REST de Supabase
                // El endpoint '/rest/v1/' es público y debe estar disponible
                var response = await _httpClient.GetAsync("rest/v1/");

                if (response.IsSuccessStatusCode)
                {
                    // Conexión exitosa (código 200-299)
                    _logger.LogInformation("Conexión exitosa a Supabase");
                    ViewBag.ConexionExitosa = "Conexión a Supabase establecida con éxito";
                    ViewBag.StatusCode = response.StatusCode;
                }
                else
                {
                    // Error en la conexión
                    _logger.LogWarning($"Error al conectar a Supabase: {response.StatusCode}");
                    ViewBag.Error = $"Error al conectar a Supabase: {response.StatusCode}";
                    ViewBag.StatusCode = response.StatusCode;

                    // Lee detalles adicionales del error
                    var content = await response.Content.ReadAsStringAsync();
                    ViewBag.ErrorDetail = content;
                }
            }
            catch (Exception ex)
            {
                // Captura excepciones (problemas de red, URL incorrecta, etc.)
                _logger.LogError(ex, "Error al conectar a Supabase");
                ViewBag.Error = "Error al conectar: " + ex.Message;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}