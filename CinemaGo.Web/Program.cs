using CinemaGo.Application.Settings;
using CinemaGo.Application.Validators;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Infrastructure.Data;
using CinemaGo.Web.Extensions;
using CinemaGo.Web.Filters;
using CinemaGo.Web.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// git-ignored ფაილი სადაცაა settings/secrets
configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// ---------------------------------------------------------------- Database (SQL Server + EF Core)
builder.Services.AddDbContext<CinemaGoDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly(typeof(CinemaGoDbContext).Assembly.GetName().Name)));

// ---------------------------------------------------------------- Identity (users + roles)
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<CinemaGoDbContext>()
    .AddDefaultTokenProviders(); // needed to generate / validate e-mail confirmation tokens

// ---------------------------------------------------------------- Configuration / options
builder.Services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<AdminRegistrationSettings>(configuration.GetSection(AdminRegistrationSettings.SectionName));
builder.Services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddDataProtection(); // protects the confirmation tokens

var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
          ?? throw new InvalidOperationException("The 'Jwt' configuration section is missing.");
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.StartsWith("[ENTER") || jwt.Key.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key is missing or still the placeholder. Copy appsettings.Local.example.json to appsettings.Local.json " +
        "(next to appsettings.json) and set Jwt:Key to a string of at least 32 characters.");

// ---------------------------------------------------------------- JWT authentication + authorization
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // keep the claim types exactly as written by JwtTokenGenerator
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = AppClaimTypes.UserName,
            RoleClaimType = AppClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

// ---------------------------------------------------------------- Validation, MVC, errors
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // This customizes how invalid model state / validation errors are returned
        options.InvalidModelStateResponseFactory = context =>
        {
            // Extract the field-level errors
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var traceId = context.HttpContext.TraceIdentifier;

            // სტანდარტული რესპონსი
            var response = new
            {
                success = false,
                message = "One or more validation errors occurred.",
                status = StatusCodes.Status400BadRequest,
                errors = errors,
                traceId = traceId
            };

            return new BadRequestObjectResult(response);
        };
    })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ---------------------------------------------------------------- Dependency injection (AddScoped...)
builder.Services.AddCinemaGoServices(configuration);

// ---------------------------------------------------------------- Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CinemaGo API", Version = "v1" });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT returned by /api/auth/login (Swagger adds the 'Bearer ' prefix)."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

var app = builder.Build();

// ---------------------------------------------------------------- Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () =>
{
    var indexPath = Path.Combine(
        app.Environment.WebRootPath!,
        "index.html"
    );

    return Results.File(indexPath, "text/html");
});

app.Run();
