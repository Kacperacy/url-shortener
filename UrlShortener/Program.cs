using Microsoft.EntityFrameworkCore;
using UrlShortener;
using UrlShortener.Models;
using UrlShortener.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<UrlShorteningService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

DatabaseManagementService.MigrationInitialisation(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.MapPost("/shorten", async (ShortenedUrlRequest request, ApplicationDbContext dbContext, UrlShorteningService urlShorteningService, HttpContext httpContext) =>
{
    var code = await urlShorteningService.GenerateUniqueCode();
    var shortenedUrl = new ShortenedUrl
    {
        LongUrl = request.LongUrl,
        Code = code,
        CreatedOnUtc = DateTime.UtcNow
    };

    dbContext.ShortenedUrls.Add(shortenedUrl);
    await dbContext.SaveChangesAsync();

    return shortenedUrl;
});

app.MapGet("{code}", async (string code, ApplicationDbContext dbContext) =>
{
    var shortenedUrl = await dbContext.ShortenedUrls.SingleOrDefaultAsync(s => s.Code == code);

    if (shortenedUrl is null)
    {
        return Results.NotFound($"No URL found for code {code}");
    }

    return Results.Redirect(shortenedUrl.LongUrl);
});

app.Run();