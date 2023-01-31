using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Services;

namespace Shop.Controllers
{
  [Route("users")]
  public class UserController : ControllerBase
  {
    [HttpGet]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<List<User>>> Get(
      [FromServices] DataContext context
    )
    {
      var users = await context.Users
        .AsNoTracking()
        .ToListAsync();
      return users;
    }

    [HttpPost]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<User>> Post(
      [FromBody] User model,
      [FromServices] DataContext context
    )
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      try
      {
        model.Role = "employee";
        context.Users.Add(model);
        await context.SaveChangesAsync();
        model.Password = "";
        return model;
      }
      catch (Exception)
      {
        return BadRequest(new { message = "Was not possible to create a user" });
      }
    }

    [HttpPut]
    [Route("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<User>> Put(
      int id,
      [FromBody] User model,
      [FromServices] DataContext context
    )
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if (id != model.Id)
        return NotFound(new { message = "User not found" });

      try
      {
        context.Entry(model).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return model;
      }
      catch (Exception)
      {
        return BadRequest(new { message = "Was not possible to update the user" });
      }
    }

    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<dynamic>> Authenticate(
      [FromBody] User model,
      [FromServices] DataContext context
    )
    {
      var user = await context.Users
      .AsNoTracking()
      .Where(x => x.Username == model.Username && x.Password == model.Password)
      .FirstOrDefaultAsync();

      if (user == null)
        return NotFound(new { message = "User or password invalid" });

      var token = TokenService.GenerateToken(user);
      user.Password = "";
      return new
      {
        user,
        token
      };

    }

    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<User>> Delete(
      [FromServices] DataContext context,
      int id
    )
    {
      var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
      if (user == null)
        return NotFound(new { message = "User not found" });

      try
      {
        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return Ok(new { message = "User successfully removed" });
      }
      catch
      {
        return BadRequest(new { message = "Could not remove the user" });
      }
    }
  }
}