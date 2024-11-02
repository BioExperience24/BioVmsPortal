using CleanArchitecture.Application.Common.Models;

namespace CleanArchitecture.Domain.Entities;

public class Media : BaseModel
{
    public string Name { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;
}
