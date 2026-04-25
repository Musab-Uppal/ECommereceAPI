using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using ECommereceAPI.Settings;
using ECommereceAPI.Services.Interfaces;

namespace ECommereceAPI.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(JwtSettings jwtSettings, ILogger<AuthService> logger)
        {
            _jwtSettings = jwtSettings;
            _logger = logger;
        }

        /// <summary>
        /// Hash password using BCrypt
        /// </summary>
        public string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new ArgumentException("Password cannot be empty");
                }

                // BCrypt automatically generates salt and hashes
                return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error hashing password: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Verify password against BCrypt hash
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                {
                    return false;
                }

                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying password: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generate JWT token with user claims
        /// </summary>
        public string GenerateJwtToken(int userId, string email, string role)
        {
            try
            {
                if (userId <= 0 || string.IsNullOrWhiteSpace(email))
                {
                    throw new ArgumentException("Invalid user data for token generation");
                }

                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role),
                    new Claim("sub", userId.ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    signingCredentials: signingCredentials
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.WriteToken(token);

                _logger.LogInformation($"JWT token generated for user: {email}");

                return jwtToken;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating JWT token: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Validate JWT token and extract claims
        /// </summary>
        public (bool isValid, int userId, string email, string role) ValidateJwtToken(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return (false, 0, null, null);
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var secretKey = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
                var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return (false, 0, null, null);
                }

                return (true, userId, emailClaim, roleClaim);
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token has expired");
                return (false, 0, null, null);
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("Invalid token signature");
                return (false, 0, null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating JWT token: {ex.Message}");
                return (false, 0, null, null);
            }
        }
    }
}