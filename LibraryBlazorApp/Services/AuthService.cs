using Blazored.LocalStorage;
using LibraryBlazorApp.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace LibraryBlazorApp.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(
            HttpClient httpClient,
            ILocalStorageService localStorage,
            AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<AuthResponse> LoginAsync(LoginModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
                {
                    Email = model.Email,
                    Password = model.Password
                });

                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                if (result != null && result.Success)
                {
                    // Store tokens in local storage
                    await _localStorage.SetItemAsStringAsync("accessToken", result.AccessToken);
                    await _localStorage.SetItemAsStringAsync("refreshToken", result.RefreshToken);
                    await _localStorage.SetItemAsStringAsync("username", result.Username);
                    await _localStorage.SetItemAsStringAsync("email", result.Email);
                    await _localStorage.SetItemAsStringAsync("role", result.Role);

                    // Notify auth state has changed
                    ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.AccessToken);
                }

                return result ?? new AuthResponse { Success = false, Message = "Unknown error occurred" };
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, Message = $"Connection error: {ex.Message}" };
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", new
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password
                });

                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                if (result != null && result.Success)
                {
                    // Store tokens in local storage
                    await _localStorage.SetItemAsStringAsync("accessToken", result.AccessToken);
                    await _localStorage.SetItemAsStringAsync("refreshToken", result.RefreshToken);
                    await _localStorage.SetItemAsStringAsync("username", result.Username);
                    await _localStorage.SetItemAsStringAsync("email", result.Email);
                    await _localStorage.SetItemAsStringAsync("role", result.Role);

                    // Notify auth state has changed
                    ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.AccessToken);
                }

                return result ?? new AuthResponse { Success = false, Message = "Unknown error occurred" };
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, Message = $"Connection error: {ex.Message}" };
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                var refreshToken = await _localStorage.GetItemAsStringAsync("refreshToken");
                if (string.IsNullOrEmpty(refreshToken))
                    return false;

                var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new
                {
                    RefreshToken = refreshToken
                });

                if (!response.IsSuccessStatusCode)
                    return false;

                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

                if (result != null && result.Success)
                {
                    await _localStorage.SetItemAsStringAsync("accessToken", result.AccessToken);
                    await _localStorage.SetItemAsStringAsync("refreshToken", result.RefreshToken);
                    await _localStorage.SetItemAsStringAsync("username", result.Username);
                    await _localStorage.SetItemAsStringAsync("email", result.Email);
                    await _localStorage.SetItemAsStringAsync("role", result.Role);

                    ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.AccessToken);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var refreshToken = await _localStorage.GetItemAsStringAsync("refreshToken");
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    // Tell the server to invalidate the refresh token
                    await _httpClient.PostAsJsonAsync("api/auth/logout", new
                    {
                        RefreshToken = refreshToken
                    });
                }
            }
            catch { /* Ignore errors during logout API call */ }

            // Clear local storage
            await _localStorage.RemoveItemAsync("accessToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            await _localStorage.RemoveItemAsync("username");
            await _localStorage.RemoveItemAsync("email");
            await _localStorage.RemoveItemAsync("role");

            // Notify auth state has changed
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }

        public async Task<string> GetUsernameAsync()
        {
            return await _localStorage.GetItemAsStringAsync("username") ?? "";
        }

        public async Task<string> GetRoleAsync()
        {
            return await _localStorage.GetItemAsStringAsync("role") ?? "";
        }
    }
}
