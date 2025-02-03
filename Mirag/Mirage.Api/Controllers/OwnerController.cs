using Microsoft.AspNetCore.Mvc;
using Mirage.Api.Models.DomainModels.Owner;

namespace Mirage.Api.Controllers;

[ApiController]
[Route("api/owners")]
public class OwnerController : ControllerBase
{
    [HttpGet("{id}")]
    public ActionResult<Owner> GetOwner(Guid id)
    {
        var owner = OwnerFakeObject.Owners.Find(x => x.Id == id);
        if (owner == null)
        {
            return NotFound();
        }
        return owner;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Owner>> GetOwners([FromQuery] int skip, [FromQuery] int take)
    {
        return OwnerFakeObject.Owners.Skip(skip).Take(take).ToList();
    }

    [HttpPost]
    public ActionResult<bool> PostOwner([FromBody] Owner request)
    {
        try
        {
            OwnerFakeObject.Owners.Add(request);
            return Ok(true);
        }
        catch (Exception)
        {
            return BadRequest(false);
        }
    }

    [HttpPut("{id}")]
    public ActionResult<bool> PutOwner([FromRoute] Guid id, [FromBody] Owner request)
    {
        try
        {
            var owner = OwnerFakeObject.Owners.Find(x => x.Id == id);
            if (owner == null)
            {
                return NotFound(false);
            }
            owner.FirstName = request.FirstName;
            owner.LastName = request.LastName;
            owner.Mobile = request.Mobile;
            return Ok(true);
        }
        catch (Exception)
        {
            return BadRequest(false);
        }
    }

    [HttpDelete("{id}")]
    public ActionResult<bool> DeleteOwner([FromRoute] Guid id)
    {
        try
        {
            var owner = OwnerFakeObject.Owners.Find(x => x.Id == id);
            if (owner == null)
            {
                return NotFound(false);
            }
            OwnerFakeObject.Owners.Remove(owner);
            return Ok(true);
        }
        catch (Exception)
        {
            return BadRequest(false);
        }
    }
}

public static class OwnerFakeObject
{
    public static List<Owner> Owners = new List<Owner>
    {
        new Owner(Guid.Parse("afe6cf9a-eca3-496e-a1a9-83fee0132f05"), "Ehsan", "Rakhshani", "09365957533"),
        new Owner(Guid.Parse("b72ee075-d8f2-4ca5-96d8-a8d687aa2ea6"), "Mohammad", "Asghar", "09365957534"),
        new Owner(Guid.Parse("dfe891ff-331b-490e-b407-ab0980f89bbc"), "Akbar", "Ahmad", "09345957535")
    };
}
