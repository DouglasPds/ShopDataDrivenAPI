using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
  [Route("products")]
  public class ProductController : ControllerBase
  {
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<Product>>> Get(
      [FromServices] DataContext context
    )
    {
      var products = await context.Products
        .Include(x => x.Category)
        .AsNoTracking()
        .ToListAsync();
      return products;
    }

    [HttpGet]
    [Route("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Product>> GetById(
      int id,
      [FromServices] DataContext context
    )
    {
      var product = await context.Products
        .Include(x => x.Category)
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == id);

      return product;
    }

    [HttpGet]
    [Route("categories/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<Product>>> GetByCategory(
      int id,
      [FromServices] DataContext context
    )
    {
      var products = await context.Products
        .Include(x => x.Category)
        .AsNoTracking()
        .Where(x => x.CategoryId == id)
        .ToListAsync();

      return products;
    }

    [HttpPost]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Product>> Post(
      [FromBody] Product model,
      [FromServices] DataContext context
    )
    {
      if (ModelState.IsValid)
      {
        context.Products.Add(model);
        await context.SaveChangesAsync();
        return model;
      }
      else
      {
        return BadRequest(ModelState);
      }
    }

    [HttpPut]
    [Route("{id:int}")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Product>> Put(
      [FromBody] Product model,
      [FromServices] DataContext context,
      int id
    )
    {
      if (model.Id != id)
        return NotFound(new { message = "Product not found" });

      if (ModelState.IsValid)
      {
        context.Entry<Product>(model).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return Ok(model);
      }
      else
      {
        return BadRequest(new { message = "Could not update the product" });
      }
    }

    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Product>> Delete(
      [FromServices] DataContext context,
      int id
    )
    {
      var product = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
      if (product == null)
        return NotFound(new { message = "Product not found" });

      try
      {
        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return Ok(new { message = "Product successfully removed" });
      }
      catch
      {
        return BadRequest(new { message = "Could not remove the product" });
      }
    }
  }
}