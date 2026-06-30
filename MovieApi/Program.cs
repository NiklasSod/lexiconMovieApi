using Microsoft.AspNetCore.Authentication.JwtBearer;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieApi.Extensions;
using System.Text;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
string connectionString;
string blobToken;
if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("MovieApiContext")
        ?? throw new InvalidOperationException("Local connection string 'MovieApiContext' not found in appsettings.Development.json.");
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

// changed to follow the lab
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
//var jwtSecret = builder.Configuration["JWT_SECRET"];
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
        ValidateLifetime = false // update if app go live and real login
    };
});

// Add services to the container.
builder.Services.AddDbContext<MovieApiContext>(options => options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    // for hosting on railway i add this in if-statement
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.SeedData();

app.Run();
