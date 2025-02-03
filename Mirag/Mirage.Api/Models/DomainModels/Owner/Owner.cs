namespace Mirage.Api.Models.DomainModels.Owner;

public class Owner
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Mobile { get; set; }
    public Owner(Guid id, string firstName, string lastName, string mobile)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Mobile = mobile;
    }
}