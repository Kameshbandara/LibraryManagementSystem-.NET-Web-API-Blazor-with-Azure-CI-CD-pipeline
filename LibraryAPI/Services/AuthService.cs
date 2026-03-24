using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ═══════════════════════════════════════
        // REGISTER
        // ═══════════════════════════════════════
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email is already registered"
                };
            }

            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Username is already taken"
                };
            }

            // Create user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate tokens
            var accessToken = GenerateAccessToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                Expiration = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        // ═══════════════════════════════════════
        // LOGIN
        // ═══════════════════════════════════════
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            if (!user.IsActive)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Account is deactivated. Contact support."
                };
            }

            // Generate tokens
            var accessToken = GenerateAccessToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                Expiration = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        // ═══════════════════════════════════════
        // REFRESH TOKEN
        // ═══════════════════════════════════════
        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid refresh token"
                };
            }

            if (!storedToken.IsActive)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Refresh token has expired or been revoked"
                };
            }

            var user = storedToken.User;

            // Revoke the old refresh token (token rotation)
            storedToken.RevokedAt = DateTime.UtcNow;

            // Generate new tokens
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = await GenerateRefreshTokenAsync(user.Id);

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Success = true,
                Message = "Token refreshed successfully",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                Expiration = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        // ═══════════════════════════════════════
        // LOGOUT
        // ═══════════════════════════════════════
        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
                return false;

            // Revoke the refresh token
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        // ═══════════════════════════════════════
        // PRIVATE HELPERS
        // ═══════════════════════════════════════

        private string GenerateAccessToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<RefreshToken> GenerateRefreshTokenAsync(int userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays()),
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        private int GetAccessTokenExpiryMinutes()
        {
            return int.TryParse(_configuration["JwtSettings:AccessTokenExpiryMinutes"], out var minutes)
                ? minutes : 15;
        }

        private int GetRefreshTokenExpiryDays()
        {
            return int.TryParse(_configuration["JwtSettings:RefreshTokenExpiryDays"], out var days)
                ? days : 7;
        }
    }
}
