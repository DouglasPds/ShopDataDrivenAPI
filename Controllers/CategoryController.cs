using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
  [Route("categories")]
  public class CategoryController : ControllerBase
  {
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any, Duration = 30)]
    public async Task<ActionResult<List<Category>>> Get(
      [FromServices] DataContext context
    )
    {
      var categories = await context.Categories.AsNoTracking().ToListAsync();
      return categories;
    }

    [HttpGet]
    [Route("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Category>> GetById(
      int id,
      [FromServices] DataContext context
    )
    {
      var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
      if (category == null)
        return NotFound(new { message = "Category not found" });

      return category;
    }

    [HttpPost]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Category>> Post(
      [FromBody] Category model,
      [FromServices] DataContext context
    )
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      try
      {
        context.Categories.Add(model);
        await context.SaveChangesAsync();
        return model;
      }
      catch (System.Exception)
      {
        return BadRequest(new { message = "Was not possible to create a category" });
      }
    }

    [HttpPut]
    [Route("{id:int}")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Category>> Put(
      int id,
      [FromBody] Category model,
      [FromServices] DataContext context
      )
    {
      if (model.Id != id)
        return NotFound(new { message = "Category not found" });

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      try
      {
        context.Entry<Category>(model).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return Ok(model);
      }
      catch (DbUpdateConcurrencyException)
      {
        return BadRequest(new { message = "This register has already been updated" });
      }
      catch (Exception)
      {
        return BadRequest(new { message = "Could not update the category" });
      }
    }

    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Category>> Delete(
      int id,
      [FromServices] DataContext context
    )
    {
      var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
      if (category == null)
        return NotFound(new { message = "Category not found" });

      try
      {
        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return Ok(new { message = "Category successfully removed" });
      }
      catch (Exception)
      {
        return BadRequest(new { message = "Could not remove the category" });
      }
    }
  }
}