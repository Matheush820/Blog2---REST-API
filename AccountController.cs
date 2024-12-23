using Blog.Data;
using Blog.Models;
using Blog2.Extensions;
using Blog2.Services;
using Blog2.ViewModels;
using Blog2.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using System.IO;
using System.Text.RegularExpressions;

namespace Blog2.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpPost("v1/accounts/")]
        public async Task<IActionResult> Post([FromBody] RegisterViewModel model, [FromServices] BlogDataContext context, [FromServices] EmailService emailService)
        {
            // Verifica se o modelo de dados é válido
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
            }

            // Valida o formato do e-mail
            if (!IsValidEmail(model.Email))
            {
                return BadRequest(new ResultViewModel<string>("Formato de e-mail inválido"));
            }

            // Cria um novo objeto de usuário a partir do modelo
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Slug = model.Email.Replace("@", "-").Replace(".", "-")
            };

            // Gera uma senha forte e faz o hash da senha antes de salvar
            var password = PasswordGenerator.Generate(25, includeSpecialChars: true, upperCase: false);
            user.PasswordHash = PasswordHasher.Hash(password);

            try
            {
                // Adiciona o usuário ao banco de dados e salva as alterações
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                // Envia o e-mail para o usuário com a senha gerada
                var resetLink = $"https://meublog.com/redefinir-senha?email={user.Email}";  // Exemplo de link dinâmico para redefinição de senha
                emailService.SendEmail(
                    toEmail: user.Email,
                    subject: "Bem-vindo ao blog",
                    body: $"Sua conta foi criada com sucesso. Acesse seu perfil utilizando o seguinte link para definir sua senha: {resetLink}."
                );

                // Retorna a resposta com a confirmação de e-mail
                return Ok(new ResultViewModel<dynamic>(new
                {
                    user = user.Email,
                    message = "Conta criada com sucesso. Verifique seu e-mail para definir sua senha."
                }));
            }
            catch (DbUpdateException)
            {
                // Caso ocorra uma tentativa de inserir um e-mail já registrado
                return StatusCode(400, new ResultViewModel<string>("Este e-mail já está registrado."));
            }
            catch (Exception ex)
            {
                // Caso ocorra qualquer outro erro inesperado
                return StatusCode(500, new ResultViewModel<string>($"Erro ao processar sua solicitação: {ex.Message}"));
            }
        }

        // Função de validação de e-mail
        private bool IsValidEmail(string email)
        {
            try
            {
                var mail = new System.Net.Mail.MailAddress(email);
                return mail.Address == email;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginViewModel model,
            [FromServices] BlogDataContext context,
            [FromServices] TokenService tokenService)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

            // Verifica se o usuário existe
            var user = await context.Users
                .AsNoTracking()
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null)
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

            // Verifica se a senha está correta
            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

            try
            {
                // Gera o token
                var token = tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05x04 - Falha interna no servidor"));
            }
        }

        [Authorize]
        [HttpPost("v1/accounts/upload-image")]
        public async Task<IActionResult> UploadImage(
     [FromBody] UploadImageViewModel model,
     [FromServices] BlogDataContext context)
        {
            // Gerar um nome único para a imagem
            var fileName = $"{Guid.NewGuid().ToString()}.jpg";

            // Remover o prefixo "data:image/jpeg;base64," (ou qualquer outro tipo de imagem)
            var base64Data = new Regex(@"^data:image\/[a-z]+;base64,").Replace(model.Base65Image, "");

            // Converter a string base64 para um array de bytes
            byte[] imageBytes = Convert.FromBase64String(base64Data);

            try
            {
                // Caminho para salvar a imagem
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                // Garantir que o diretório exista
                var directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Escrever o arquivo no sistema
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                // Buscar o usuário no banco de dados
                var user = await context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

                if (user == null)
                {
                    return NotFound(new ResultViewModel<User>("Usuário não encontrado"));
                }

                // Atualiza a URL da imagem no usuário
                user.Image = $"https://localhost:5001/images/{fileName}";  // Ajuste a URL conforme o seu servidor local

                // Salva as alterações no banco de dados
                context.Users.Update(user);
                await context.SaveChangesAsync();

                // Retornar uma resposta de sucesso
                return Ok(new ResultViewModel<string>("Imagem carregada com sucesso"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>($"Falha interna no servidor: {ex.Message}"));
            }
        }

    }
}
