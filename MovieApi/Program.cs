using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieApi;
using MovieApi.Extensions;
using MovieApi.Services;
using System.Text;
using System.Text.Json;

// Only load .env file if it exists (not present in Docker containers — config comes from env vars)
var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFilePath))
{
    Env.Load(envFilePath);
}

var builder = WebApplication.CreateBuilder(args);

var isRunningInDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
string? connectionString;
string blobToken;
if (builder.Environment.IsDevelopment())
{
    //connectionString = Environment.GetEnvironmentVariable("DOCKER_SQL_CONNECTIONSTRING")
    //    ?? builder.Configuration.GetConnectionString("MovieApiContext")
    //    ?? throw new InvalidOperationException("Local connection string 'MovieApiContext' not found in appsettings.Development.json.");
    connectionString = isRunningInDocker
        ? Environment.GetEnvironmentVariable("DOCKER_SQL_CONNECTIONSTRING")
        : null;
    connectionString ??= builder.Configuration.GetConnectionString("MovieApiContext")
        ?? throw new InvalidOperationException("Local connection string 'MovieApiContext' not found.");
    blobToken = builder.Configuration["VERCEL_BLOB_TOKEN"]
        ?? throw new InvalidOperationException("VERCEL_BLOB_TOKEN not found. Ensure .env contains this variable.");
}
else
{
    connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"]
        ?? throw new InvalidOperationException("Azure SQL connection string 'AZURE_SQL_CONNECTIONSTRING' not found. Ensure .env contains this variable.");
    blobToken = builder.Configuration["BLOB_READ_WRITE_TOKEN"]
        ?? throw new InvalidOperationException("Vercel blob 'BLOB_READ_WRITE_TOKEN' not found. Ensure .env contains this variable.");
}

var jwtSecret = builder.Configuration["JWT_SECRET"];
if (string.IsNullOrEmpty(jwtSecret))
    throw new InvalidOperationException("JWT_SECRET is missing or empty. Authentication will not work.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});

// Add services to the container.
builder.Services.AddDbContext<MovieApiContext>(options => options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));
builder.Services.AddScoped<IMovieApiContext>(sp => sp.GetRequiredService<MovieApiContext>());
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Global exception handler: log the full error server-side and return a
// readable JSON body instead of an empty 500, so the frontend can surface it.
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");
        logger.LogError(exception, "Unhandled exception during {Method} {Path}",
            context.Request.Method, context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            message = exception?.Message ?? "An unexpected error occurred.",
        }));
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // for hosting on railway i add this in if-statement
    app.UseHttpsRedirection();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MovieApiContext>();
    dbContext.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.SeedData();

app.Run();
