using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Percuro.Services;
using BCrypt.Net;
using System;

namespace Percuro.Services;

public static class LoginApiHost
{
    public static void Start()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapPost("/api/login", async (HttpContext context) =>
        {
            // Deserialisiere Request case-insensitive
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var loginRequest = await JsonSerializer.DeserializeAsync<LoginRequest>(context.Request.Body, options);
            if (loginRequest is null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, error = "Ungültige Anfrage." });
                return;
            }

            var dbService = new DatabaseService();
            Console.WriteLine($"[LoginApi] Suche User: '{loginRequest.Username}'");
            var user = await dbService.GetUserByUsernameAsync(loginRequest.Username);
            if (user == null)
            {
                Console.WriteLine($"[LoginApi] User '{loginRequest.Username}' NICHT gefunden!");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, error = "Benutzername nicht gefunden." });
                return;
            }

            // Debug-Logging: Hash und Passwort prüfen
            Console.WriteLine($"[LoginApi] Username: {user.Username}");
            Console.WriteLine($"[LoginApi] Hash from DB: '{user.PasswordHash}'");
            Console.WriteLine($"[LoginApi] Password Provided: '{loginRequest.Password}'");
            bool passwordOk = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);
            Console.WriteLine($"[LoginApi] Password OK: {passwordOk}");

            if (passwordOk)
            {
                // Token kann hier z.B. ein einfacher Guid oder JWT sein, für Demo ein Guid
                var token = Guid.NewGuid().ToString();
                context.Response.StatusCode = 200;
                await context.Response.WriteAsJsonAsync(new {
                    success = true,
                    token,
                    username = user.Username,
                    role = user.Role
                });
            }
            else
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { success = false, error = "Falsches Passwort." });
            }
        });

        app.Run("http://localhost:5005");
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
