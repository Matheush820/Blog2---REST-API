using Blog.Models;
using Blog2.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Blog2.Services
{
    public class TokenService
    {
        public string GenerateToken(User user)
        {
            try
            {
                // Criação do handler para tokens JWT
                var tokenHandler = new JwtSecurityTokenHandler();

                // Chave de assinatura (verifique se está corretamente configurada em sua aplicação)
                var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);

                // Criação das claims associadas ao usuário
                var claims = user.GetClaims();

                // Descrição do token
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(8), // Token expira em 8 horas
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                // Criação do token JWT
                var token = tokenHandler.CreateToken(tokenDescriptor);

                // Retorno do token em formato string
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                // Log opcional do erro (ou uso de monitoramento como Sentry)
                Console.WriteLine($"Erro ao gerar o token: {ex.Message}");

                // Retorno de um valor controlado em caso de falha
                return string.Empty; // Retorna uma string vazia ao invés de null
            }
        }
    }
}
