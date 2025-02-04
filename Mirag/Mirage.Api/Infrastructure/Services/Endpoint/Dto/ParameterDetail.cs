using Mirage.Api.Common;

namespace Mirage.Api.Infrastructure.Services.Endpoint.Dto;

public class ParameterDetail
{
    public string Name { get; set; } 
    public Type Type { get; set; } 
    public ModelBindingType ModelBinding { get; set; } 
}
