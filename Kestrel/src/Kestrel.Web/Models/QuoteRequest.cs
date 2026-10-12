namespace Kestrel.Web.Models;

public enum QuoteStatus { New, Reviewed, Archived }

public class QuoteRequest
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public string? Budget { get; set; }
    public string Message { get; set; } = "";
    public QuoteStatus Status { get; set; } = QuoteStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
