# Documentación del Proyecto Deltarune

## 📋 Resumen Ejecutivo

**Deltarune** es una aplicación web desarrollada en **ASP.NET Core 7.0** que implementa el patrón **MVC (Model-View-Controller)** y se conecta a **Supabase** como base de datos en la nube. La aplicación incluye un sistema de autenticación básico y una interfaz de usuario con temática del juego Deltarune.

---

## 🏗️ Arquitectura del Proyecto

### Patrón MVC (Model-View-Controller)

El proyecto sigue el patrón MVC de ASP.NET Core:

- **Models**: Clases que representan los datos y la lógica de negocio
- **Views**: Archivos Razor (.cshtml) que definen la interfaz de usuario
- **Controllers**: Clases que manejan las peticiones HTTP y coordinan entre Models y Views

---

## 🔧 Componentes Principales

### 1. **Program.cs** - Punto de Entrada
```csharp
// Configuración de servicios
builder.Services.AddControllersWithViews();  // Habilita MVC
builder.Services.AddHttpClient();            // Para conexiones HTTP a Supabase

// Configuración del pipeline HTTP
app.UseStaticFiles();   // Sirve archivos CSS/JS
app.UseRouting();       // Habilita enrutamiento

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Función**: Configura la aplicación, registra servicios y define el pipeline HTTP.

---

### 2. **Controladores (Controllers/)**

#### **HomeController.cs**
```csharp
public class HomeController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    
    // Constructor con inyección de dependencias
    public HomeController(ILogger<HomeController> logger, 
                         IConfiguration configuration, 
                         IHttpClientFactory httpClientFactory)
```

**Funciones principales**:
- **Index()**: Verifica la conexión a Supabase
- **Privacy()**: Muestra página de privacidad
- **Error()**: Maneja errores de la aplicación

**Flujo de datos**:
1. Recibe petición HTTP
2. Configura HttpClient con credenciales de Supabase
3. Hace petición a la API REST de Supabase
4. Pasa resultados a la vista mediante ViewBag

#### **AuthController.cs**
```csharp
public class AuthController : Controller
{
    // Métodos principales:
    [HttpGet] public IActionResult Login()           // Muestra formulario
    [HttpPost] public async Task<IActionResult> Login(LoginViewModel model)  // Procesa login
    [HttpGet] public IActionResult Dashboard()       // Panel de usuario
    [HttpGet] public IActionResult Logout()          // Cerrar sesión
```

**Flujo de autenticación**:
1. Usuario envía credenciales
2. `ValidateUserAsync()` busca usuario en Supabase
3. `BCrypt.Verify()` valida contraseña hasheada
4. Si es válido, redirige al Dashboard
5. Si no, muestra error

---

### 3. **Modelos (Models/)**

#### **UserModel.cs**
```csharp
public class UserModel
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }  // Contraseña hasheada con BCrypt
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; }
}
```

#### **LoginViewModel.cs**
```csharp
public class LoginViewModel
{
    [Required] public string Username { get; set; }
    [Required][DataType(DataType.Password)] public string Password { get; set; }
    public string ErrorMessage { get; set; }  // Para mostrar errores
}
```

**Función**: Representan la estructura de datos y validaciones.

---

### 4. **Vistas (Views/)**

#### **Layout Principal (_Layout.cshtml)**
```html
<!-- Música de fondo automática -->
<audio id="bg-music" src="/audio/field-of-hopes-and-dreams.mp3" autoplay loop hidden></audio>
<script>
    // Persiste el tiempo de reproducción en localStorage
    const savedTime = localStorage.getItem('bg-music-time');
    if (savedTime) {
        audio.currentTime = parseFloat(savedTime);
    }
</script>
```

**Características**:
- Música de fondo automática (Field of Hopes and Dreams)
- Persistencia del tiempo de reproducción
- Diseño responsive con Bootstrap

#### **Vista Index (Home/Index.cshtml)**
```html
@if (!string.IsNullOrEmpty(ViewBag.ConexionExitosa))
{
    <div class="alert alert-success">
        <strong>¡Conexión exitosa!</strong> @ViewBag.ConexionExitosa
    </div>
}
```

**Función**: Muestra el estado de conexión a Supabase y información técnica.

#### **Vista Login (Auth/Login.cshtml)**
```html
<form asp-action="Login" method="post">
    @Html.AntiForgeryToken()  <!-- Protección CSRF -->
    <input asp-for="Username" class="form-control" />
    <input asp-for="Password" class="form-control" />
    <button type="submit" class="btn btn-primary">Iniciar Sesión</button>
</form>
```

**Características**:
- Formulario con validación del lado cliente
- Token anti-falsificación (CSRF protection)
- Mensajes de error dinámicos

---

## 🔄 Flujo de Datos Completo

### 1. **Petición HTTP → Controlador**
```
Usuario hace clic → Navegador envía HTTP Request → Router de ASP.NET Core → Controlador apropiado
```

### 2. **Controlador → Modelo**
```
Controlador recibe petición → Valida datos → Llama métodos del modelo → Procesa lógica de negocio
```

### 3. **Modelo → Base de Datos (Supabase)**
```
Modelo hace petición HTTP → Supabase API REST → PostgreSQL → Respuesta JSON
```

### 4. **Controlador → Vista**
```
Controlador recibe respuesta → Procesa datos → Pasa a vista mediante ViewBag/Model → Renderiza HTML
```

### 5. **Vista → Usuario**
```
Vista genera HTML → Navegador recibe respuesta → Muestra página al usuario
```

---

## 🗄️ Conexión a Supabase

### Configuración (appsettings.json)
```json
{
  "Supabase": {
    "Url": "https://vfygligjibznboqozhzv.supabase.co",
    "Key": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "Options": {
      "AutoRefreshToken": true,
      "PersistSession": true
    }
  }
}
```

### Método de Conexión
```csharp
// Configuración del HttpClient
_httpClient.BaseAddress = new Uri(url);
_httpClient.DefaultRequestHeaders.Add("apikey", key);
_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);

// Ejemplo de consulta
string url = $"{_supabaseUrl}/rest/v1/users?username=eq.{username}";
var response = await _httpClient.GetAsync(url);
```

**Características**:
- Conexión mediante API REST
- Autenticación con API Key
- Consultas SQL mediante parámetros de URL

---

## 🔐 Sistema de Autenticación

### Proceso de Login
1. **Usuario envía credenciales** → Formulario POST
2. **Validación del modelo** → DataAnnotations
3. **Búsqueda en Supabase** → Query por username
4. **Verificación de contraseña** → BCrypt.Verify()
5. **Creación de sesión** → TempData
6. **Redirección** → Dashboard o error

### Seguridad
- **Contraseñas hasheadas** con BCrypt
- **Token anti-CSRF** en formularios
- **Validación del lado cliente y servidor**
- **Manejo seguro de errores**

---

## 🎨 Interfaz de Usuario

### Tecnologías Frontend
- **Bootstrap 5**: Framework CSS para diseño responsive
- **jQuery**: Manipulación del DOM
- **Font Awesome**: Iconos
- **Google Fonts**: Tipografía "Press Start 2P" (estilo pixel art)

### Características Visuales
- **Temática Deltarune**: Colores y estilos del juego
- **Música de fondo**: Reproducción automática
- **Diseño responsive**: Adaptable a móviles y desktop
- **Animaciones**: Transiciones suaves

---

## 📁 Estructura de Archivos

```
Deltarune/
├── Controllers/          # Lógica de control
│   ├── HomeController.cs
│   └── AuthController.cs
├── Models/              # Estructura de datos
│   ├── UserModel.cs
│   ├── LoginViewModel.cs
│   └── ErrorViewModel.cs
├── Views/               # Interfaz de usuario
│   ├── Home/
│   ├── Auth/
│   └── Shared/
├── wwwroot/            # Archivos estáticos
│   ├── css/
│   ├── js/
│   ├── audio/
│   └── img/
├── Program.cs          # Configuración de la app
└── appsettings.json    # Configuración
```

---

## 🚀 Cómo Ejecutar el Proyecto

1. **Requisitos**:
   - .NET 7.0 SDK
   - Visual Studio 2022 o VS Code

2. **Configuración**:
   - Verificar credenciales en `appsettings.json`
   - Asegurar conexión a Internet

3. **Ejecución**:
   ```bash
   dotnet run
   ```

4. **Acceso**:
   - URL: `http://localhost:5237`
   - Login: `http://localhost:5237/Auth/Login`

---

## 🔧 Tecnologías Utilizadas

- **Backend**: ASP.NET Core 7.0, C#
- **Frontend**: HTML5, CSS3, JavaScript, Bootstrap 5
- **Base de Datos**: Supabase (PostgreSQL)
- **Autenticación**: BCrypt para hashing
- **Patrón**: MVC (Model-View-Controller)
- **API**: REST con HttpClient

---

## 📝 Puntos Clave para la Evaluación

1. **Arquitectura MVC**: Separación clara de responsabilidades
2. **Inyección de Dependencias**: Uso correcto de DI container
3. **Conexión a Base de Datos**: Integración con Supabase via REST API
4. **Seguridad**: Hashing de contraseñas, tokens CSRF
5. **Validación**: Del lado cliente y servidor
6. **Experiencia de Usuario**: Música, diseño responsive, feedback visual
7. **Manejo de Errores**: Try-catch, logging, páginas de error
8. **Configuración**: appsettings.json para credenciales

---

## 🎯 Funcionalidades Implementadas

✅ **Sistema de autenticación completo**
✅ **Conexión a Supabase**
✅ **Interfaz de usuario responsive**
✅ **Música de fondo persistente**
✅ **Validación de formularios**
✅ **Manejo de errores**
✅ **Navegación entre páginas**

---

*Documento creado para evaluación del proyecto Deltarune - ASP.NET Core MVC con Supabase* 