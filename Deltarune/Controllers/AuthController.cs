// Controlador que maneja la autenticación de usuarios
// Conecta con Supabase para validar credenciales y gestionar sesiones
using Deltarune.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Deltarune.Controllers
{
    public class AuthController : Controller
    {
        // Cliente HTTP para conectar con Supabase
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;

        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _supabaseUrl = configuration["Supabase:Url"] ?? string.Empty;
            _supabaseKey = configuration["Supabase:Key"] ?? string.Empty;

            // Configura headers de autenticación para Supabase
            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
        }

        [HttpGet]
        public IActionResult Login()
        {
            var model = new LoginViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Valida las credenciales del usuario
                var user = await ValidateUserAsync(model);

                if (user != null)
                {
                    // Login exitoso - guarda datos en TempData para la sesión
                    TempData["Username"] = user.Username;
                    TempData["WelcomeMessage"] = $"¡Bienvenido, {user.Username}!";
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    // Credenciales incorrectas
                    model.ErrorMessage = "Usuario o contraseña incorrectos";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Login: {ex.Message}");
                model.ErrorMessage = "Error del sistema. Intenta nuevamente.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            // Verifica que el usuario esté autenticado
            if (TempData["Username"] == null)
                return RedirectToAction("Login");

            ViewBag.Username = TempData["Username"];
            ViewBag.WelcomeMessage = TempData["WelcomeMessage"];
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            TempData.Clear();  // Limpia todos los datos de sesión
            return RedirectToAction("Index", "Home");
        }

        private async Task<UserModel?> ValidateUserAsync(LoginViewModel model)
        {
            try
            {
                // Busca el usuario por nombre de usuario
                var user = await GetUserByUsernameAsync(model.Username);
                if (user == null)
                    return null;

                // Verifica la contraseña usando BCrypt
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                return isPasswordValid ? user : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ValidateUserAsync: {ex.Message}");
                return null;
            }
        }

        private async Task<UserModel?> GetUserByUsernameAsync(string username)
        {
            try
            {
                // Construye la URL para buscar el usuario en Supabase
                string url = $"{_supabaseUrl}/rest/v1/users?username=eq.{username}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Deserializa usando snake_case para coincidir con Supabase
                    var users = JsonSerializer.Deserialize<UserModel[]>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = new SnakeCaseNamingPolicy()
                    });

                    return users?.FirstOrDefault();
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetUserByUsernameAsync: {ex.Message}");
                return null;
            }
        }
    }

    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            // Convierte "PasswordHash" a "password_hash"
            return string.Concat(name.Select((ch, i) =>
                i > 0 && char.IsUpper(ch)
                    ? "_" + char.ToLower(ch)
                    : char.ToLower(ch).ToString()
            ));
        }
    }
}
