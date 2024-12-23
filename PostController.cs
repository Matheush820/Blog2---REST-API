using Blog.Data;
using Blog.Models;
using Blog2.ViewModels;
using Blog2.ViewModels.Posts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog2.Controllers
{
    [ApiController]
    public class PostController : ControllerBase
    {
        [HttpGet("v1/posts")]
        public async Task<IActionResult> GetAsync([FromServices] 
            BlogDataContext context, 
            [FromQuery]int page = 0, 
            [FromQuery]int pageSize = 25)
        {
            var count = await context.Posts.CountAsync();
            var posts = await context.Posts
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Author)
                .Select(x => new ListPostsViewModel
                {
                   Id = x.Id,
                   Title = x.Title,
                   Slug = x.Slug,
                   lastUpdateDate = x.LastUpdateDate,
                   Category = x.Category.Name,
                   Author = $"{x.Author.Name} ({x.Author.Email})",
                })
                .Skip(page * pageSize)
                .Take(pageSize)
                .OrderByDescending(x => x.lastUpdateDate)
                .ToListAsync();

            return Ok(new ResultViewModel<dynamic>(new
            {
                total = count,
                page, pageSize,
                posts
            }));
        }

        [HttpGet("v1/posts/{id:int}")]
        public async Task<IActionResult> DetailsAsync(
     [FromServices] BlogDataContext context,
     [FromRoute] int id)
        {
            try
            {
                var post = await context.Posts
                    .AsNoTracking()
                    .Include(x => x.Author)
                        .ThenInclude(x => x.Roles)
                    .Include(x => x.Category)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (post == null)
                    return NotFound(new ResultViewModel<Post>("Conteúdo não encontrado"));

                return Ok(new ResultViewModel<Post>(post));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Post>($"Erro interno: {ex.Message}"));
            }
        }

        [HttpGet("v1/posts/category/{category}")]
        public async Task<IActionResult> GetByCategoryAsync(
    [FromRoute] string category,
    [FromServices] BlogDataContext context,
    [FromQuery] int page = 0,
    [FromQuery] int pageSize = 25)
        {
            try
            {
                var count = await context.Posts.AsNoTracking().CountAsync();
                var posts = await context
                    .Posts
                    .AsNoTracking()
                    .Include(x => x.Author)
                    .Include(x => x.Category)
                    .Where(x => x.Category.Slug == category)
                    .Select(x => new ListPostsViewModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Slug = x.Slug,
                        lastUpdateDate = x.LastUpdateDate,
                        Category = x.Category.Name,
                        Author = $"{x.Author.Name} ({x.Author.Email})"
                    })
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .OrderByDescending(x => x.lastUpdateDate)
                    .ToListAsync();

                return Ok(new ResultViewModel<dynamic>( // Corrigido para parêntese
                    new
                    {
                        total = count,
                        page,
                        pageSize,
                        posts
                    })); // Corrigido
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<List<Post>>($"Erro interno: {ex.Message}"));
            }
        }


    }
}
