using Microsoft.AspNetCore.Mvc;
using Mirage.Api.Infrastructure.Attributes;
using Mirage.Api.Models.DomainModels.Owner;
using Mirage.Api.StaticSamples.Owners;

namespace Mirage.Api.Controllers;

[ApiController]
[Route("api/owners")]
public class OwnerController : ControllerBase
{
    [HttpGet("{id}")]
    public ActionResult<Owner> GetOwner([FromRoute] Guid id)
    {
        var owner = OwnerDataSource.Owners.Find(x => x.Id == id);
        if (owner == null)
        {
            return NotFound();
        }
        return owner;
    }

    [HttpGet]
    public ActionResult<Owner> GetOwnerById([FromQuery] Guid id)
    {
        var owner = OwnerDataSource.Owners.Find(x => x.Id == id);
        if (owner == null)
        {
            return NotFound();
        }
        return owner;
    }

    [HttpGet]
    [Route("AllGetOwners")]
    public List<Owner> AllGetOwners()
    {
        return OwnerDataSource.Owners;
    }

    [HttpPost]
    public ActionResult<Guid> PostOwner([FromBody] Owner request)
    {
        try
        {
            OwnerDataSource.Owners.Add(request);
            return Ok(true);
        }
        catch (Exception)
        {
            return BadRequest(false);
        }
    }

    [HttpPut("{id}")]
    public ActionResult<Guid> PutOwner([FromRoute] Guid id, [FromBody] Owner request)
    {
        try
        {
            var owner = OwnerDataSource.Owners.Find(x => x.Id == id);
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
    public ActionResult DeleteOwner([FromRoute] Guid id)
    {
        try
        {
            var owner = OwnerDataSource.Owners.Find(x => x.Id == id);
            if (owner == null)
            {
                return NotFound(false);
            }
            OwnerDataSource.Owners.Remove(owner);
            return Ok(true);
        }
        catch (Exception)
        {
            return BadRequest(false);
        }
    }

    [HttpGet("{id}/static")]
    [SampleAttribute<GetOwnerSample>]
    public ActionResult<Owner> GetStatic([FromRoute] Guid id)
    {
        var owner = OwnerDataSource.Owners.Find(x => x.Id == id);
        if (owner == null)
        {
            return NotFound();
        }
        return owner;
    }

}

public static class OwnerDataSource
{
    public static List<Owner> Owners = new List<Owner>
    {
        new Owner(){Id= Guid.Parse("afe6cf9a-eca3-496e-a1a9-83fee0132f05"),FirstName="Ehsan", LastName="Rakhshani",Mobile="09365957533" },
        new Owner(){Id=Guid.Parse("b72ee075-d8f2-4ca5-96d8-a8d687aa2ea6"),FirstName="Mohammad",LastName= "Asghar",Mobile= "09365957534" },
        new Owner(){Id=Guid.Parse("dfe891ff-331b-490e-b407-ab0980f89bbc"),FirstName= "Akbar",LastName= "Ahmad",Mobile= "09345957535" }
    };
}