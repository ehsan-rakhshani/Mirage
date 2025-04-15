namespace Mirage.Api.Models.DomainModels.Owner;

public record Owner(Guid Id, string FirstName, string LastName, string Mobile)
{
    public Owner() : this(default, default, default, default)
    {

    }
}