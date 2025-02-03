using System.ComponentModel.DataAnnotations;

namespace Mirage.Api.Models.DomainModels.User;

public class User
{
    public long Id { get; set; } 

    public string Name { get; set; } 
    public User(long id, string name)
    {
        Id = id;
        Name = name;    
    }
}
