namespace Mirage.Api.Models.DomainModels.Owner;

public class Owner
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Mobile { get; set; }

    public Owner()
    {
        
    }
    public Owner(Guid Id, string FirstName, string LastName, string Mobile)
    {
        
    }
};