using Microsoft.AspNetCore.Mvc;
using Mirage.Api.Models.DomainModels.User;

namespace Mirage.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : Controller
{

    [HttpGet("{id}")]
    public ActionResult<User> GetUser(long id)
    {
        var User = UserDataSource.Users.Find(x => x.Id == id);
        if (User == null)
        {
            return NotFound();
        }
        return User;
    }

    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers([FromQuery] int skip, [FromQuery] int take)
    {
        return UserDataSource.Users.Skip(skip).Take(take).ToList();
    }

    [HttpPost]
    public ActionResult<bool> PostUser([FromBody] User request)
    {
        try
        {
            UserDataSource.Users.Add(request);
            return Ok(true);
        }
        catch (Exception)
        {
            return BadRequest(false);
        }
    }

    [HttpPut("{id}")]
    public ActionResult<bool> PutUser([FromRoute] long id, [FromBody] User request)
    {
        try
        {
            var User = UserDataSource.Users.Find(x => x.Id == id);
            if (User == null)
            {
                return NotFound(false);
            }
            User.Name = request.Name;
            return Ok(true);
        }
        catch (Exception)
        {
            return BadRequest(false);
        }
    }

    [HttpDelete("{id}")]
    public ActionResult<bool> DeleteUser([FromRoute] long id)
    {
        try
        {
            var User = UserDataSource.Users.Find(x => x.Id == id);
            if (User == null)
            {
                return NotFound(false);
            }
            UserDataSource.Users.Remove(User);
            return Ok(true);
        }
        catch (Exception)
        {
            return BadRequest(false);
        }
    }
}

public static class UserDataSource
{
    public static List<User> Users = new List<User>
    {
        new User(1111, "Ehsan - Rakhshani"),
        new User(1111, "Mohammad - Asghar"),
        new User(1111, "Akbar - Ahmad")
    };
}
