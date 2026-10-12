namespace Kestrel.Web.Models;

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Location { get; set; } = "";
    public int Year { get; set; }
    public string? ClientName { get; set; }
    public int? AreaM2 { get; set; }
    public string Description { get; set; } = "";
    public string? CoverImage { get; set; }   // file name inside wwwroot/uploads
    public bool IsFeatured { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
