using Microsoft.AspNetCore.Authentication.JwtBearer;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieApi.Extensions;
using System.Text;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
string connectionString;
if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("MovieApiContext")
        ?? throw new InvalidOperationException("Local connection string 'MovieApiContext' not found in appsettings.Development.json.");
}
else
{
    connectionString = builder.Configuration["AZURE_SQL_CONNECTIONSTRING"]
        ?? throw new InvalidOperationException("Azure SQL connection string 'AZURE_SQL_CONNECTIONSTRING' not found. Ensure .env contains this variable.");
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
        ValidateLifetime = false // update if app go live and real login
    };
});

// Add services to the container.
builder.Services.AddDbContext<MovieApiContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.SeedData();

app.Run();
