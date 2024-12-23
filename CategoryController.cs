using Blog.Data;
using Blog.Models;
using Blog2.Extensions;
using Blog2.ViewModels;
using Blog2.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog2.Controllers;

[ApiController]
public class CategoryController : ControllerBase
{
    // GET /v1/categories
    [HttpGet("v1/categories")]

    public async Task<IActionResult> GetAsync([FromServices] BlogDataContext context, [FromServices] IMemoryCache cache)
    {
        try
        {
            var categories = cache.GetOrCreate(key:"CategoriesCache", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return GetCategories(context);
            });

            return Ok(new ResultViewModel<List<Category>>(categories));
        }
        catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<List<Category>>("Falha interna no servidor"));
        }
    }

    private List<Category> GetCategories(BlogDataContext context)
    {
        return context.Categories.ToList();
    }



    // GET /v1/categories/{id:int}
    [HttpGet("v1/categories/{id:int}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] int id,
        [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

            return Ok(new ResultViewModel<Category>(category));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor"));
        }
    }

    // POST /v1/categories
    [HttpPost("v1/categories")]
    public async Task<IActionResult> PostAsync(
        [FromBody] EditorCategoryViewModel model,
        [FromServices] BlogDataContext context)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

        try
        {
            var category = new Category
            {
                Id = 0,
                Name = model.Name,
                Slug = model.Slug.ToLower(),
            };

            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            return Created($"v1/Categories/{category.Id}", new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, new ResultViewModel<Category>("Não foi possível incluir a categoria"));
        }
        catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<Category>("Erro interno no servidor"));
        }
    }

    // PUT /v1/categories/{id:int}
    [HttpPut("v1/categories/{id:int}")]
    public async Task<IActionResult> PutAsync(
        [FromRoute] int id,
        [FromBody] EditorCategoryViewModel model,
        [FromServices] BlogDataContext context)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
            return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

        category.Name = model.Name;
        category.Slug = model.Slug;

        try
        {
            context.Categories.Update(category);
            await context.SaveChangesAsync();

            return Ok(new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, new ResultViewModel<Category>("Não foi possível atualizar a categoria"));
        }
        catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<Category>("Erro interno no servidor"));
        }
    }

    // DELETE /v1/categories/{id:int}
    [HttpDelete("v1/categories/{id:int}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] int id,
        [FromServices] BlogDataContext context)
    {
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
            return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

        try
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();

            return Ok(new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException)
        {
            return StatusCode(500, new ResultViewModel<Category>("Erro ao deletar a categoria"));
        }
        catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<Category>("Erro interno no servidor"));
        }
    }
}
