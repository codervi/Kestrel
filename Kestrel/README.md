# Kestrel Engineering

Website and admin panel for a fictional civil and structural engineering firm. I built it as a portfolio project with ASP.NET Core Razor Pages (.NET 8) and SQL Server.

Visitors can browse the project portfolio, filter it by type, and send a quote request. Staff log in to manage projects, categories and incoming requests.

## What it does

Public site
- Home page with featured projects and a few numbers calculated from the database
- Project list with filter by type, search by name or place, and paging
- Project detail page with related projects
- Quote request form (validation on the server, plus a hidden honeypot field against simple bots)

Admin panel (`/Admin`, Admin role only)
- Dashboard with the latest requests
- Projects: add, edit, delete, with cover image upload
- Categories: add and delete (a category that is still used cannot be deleted)
- Quote requests: filter by status, mark as reviewed or archive

## Stack

- ASP.NET Core Razor Pages, .NET 8
- Entity Framework Core 8, Code First with migrations
- SQL Server (LocalDB works fine)
- ASP.NET Core Identity with roles, account lockout after 5 failed logins
- HTML, CSS and a little plain JavaScript, no frontend framework

## Running it

You need the .NET 8 SDK and SQL Server or LocalDB.

```
cd src/Kestrel.Web
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet run
```

On startup the app applies the migration, creates the admin user and adds a few sample projects.

Admin login (development only, set in `appsettings.json`):

```
admin@kestrel.test
Admin#12345
```

Change the connection string in `appsettings.json` if you are not using LocalDB. For a real deployment, put the admin credentials in user secrets or environment variables instead.

## Notes on a few decisions

- **Razor Pages instead of MVC.** Every page here is a form or a list, so keeping the handler next to its view is simpler. The whole `/Admin` folder is protected with one line in `Program.cs` (`AuthorizeFolder`).
- **Image uploads.** `ImageStore` checks the extension, the size (3 MB) and the first bytes of the file, then saves it under a random name. The original file name is never used on disk.
- **Delete rules.** Categories use `DeleteBehavior.Restrict`, so the database refuses to remove a category that still has projects or quote requests. The admin page shows a normal message instead of an exception.
- **Add and edit share one page.** `Admin/Projects/Edit` takes an optional id.
- **Status stored as text** (`New`, `Reviewed`, `Archived`) so the table is readable in SSMS.

## Ideas for later

- Email notification when a quote request arrives
- Several images per project
- Tests for the upload checks
