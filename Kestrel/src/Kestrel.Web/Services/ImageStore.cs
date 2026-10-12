namespace Kestrel.Web.Services;

// Saves uploaded cover images into wwwroot/uploads.
// Extension and size are checked, and the first bytes of the file are checked
// too, because a file name like "shell.jpg" proves nothing.
public class ImageStore
{
    private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxBytes = 3 * 1024 * 1024;

    private readonly string _folder;

    public ImageStore(IWebHostEnvironment env)
    {
        _folder = Path.Combine(env.WebRootPath, "uploads");
    }

    public async Task<(string? Name, string? Error)> SaveAsync(IFormFile file)
    {
        if (file.Length == 0) return (null, "The image file is empty.");
        if (file.Length > MaxBytes) return (null, "The image must be 3 MB or smaller.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext)) return (null, "Use a JPG, PNG or WebP image.");

        var head = new byte[12];
        int read;
        using (var s = file.OpenReadStream())
            read = await s.ReadAsync(head.AsMemory(0, head.Length));

        if (!LooksLikeImage(head, read)) return (null, "That file is not a valid image.");

        Directory.CreateDirectory(_folder);
        var name = Guid.NewGuid().ToString("N") + ext;
        await using var fs = File.Create(Path.Combine(_folder, name));
        await file.CopyToAsync(fs);
        return (name, null);
    }

    public void Delete(string? name)
    {
        if (string.IsNullOrEmpty(name)) return;
        var path = Path.Combine(_folder, Path.GetFileName(name));
        if (File.Exists(path)) File.Delete(path);
    }

    private static bool LooksLikeImage(byte[] b, int n)
    {
        if (n < 12) return false;
        bool jpeg = b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF;
        bool png = b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47;
        bool webp = b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
                    && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P';
        return jpeg || png || webp;
    }
}
