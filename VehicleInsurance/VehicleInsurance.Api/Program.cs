using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using VehicleInsurance.Application.Estimates.Services;
using VehicleInsurance.Infrastructure.Estimates.Services;
using VehicleInsurance.Application.Customers.Validators;
using VehicleInsurance.Application.Auth;
using VehicleInsurance.Domain.Common.Email;
using VehicleInsurance.Domain.Common.Errors;
using VehicleInsurance.Application.Vehicles.Services;
using VehicleInsurance.Application.Vehicles.Interfaces;

using VehicleInsurance.Application.EmailVerification;
using VehicleInsurance.Infrastructure.Customers;
using VehicleInsurance.Domain.Auth;
using VehicleInsurance.Domain.Users;
using VehicleInsurance.Application.Customers.Services;
using VehicleInsurance.Infrastructure;
using VehicleInsurance.Infrastructure.Auth;
using VehicleInsurance.Infrastructure.Users;
using VehicleInsurance.Infrastructure.EmailVerification;
using VehicleInsurance.Infrastructure.Common.Email; // GmailSmtpEmailSender
using VehicleInsurance.Application.Customers.Interfaces;
using VehicleInsurance.Infrastructure.Vehicles.Services;
using VehicleInsurance.Infrastructure.Vehicles;
using VehicleInsurance.Infrastructure.Data;
using VehicleInsurance.Api.Middleware;
using Microsoft.Extensions.Logging;
using System.Text.Json;

Console.WriteLine(">>> BOOT VEHICLE API :: 9f6ab7c6-2a7a-4c79-8b35-7f9b6db3f33b");

var builder = WebApplication.CreateBuilder(args);

// ---------- Logging ----------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ---------- MVC + FluentValidation ----------
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<VehicleInsurance.Api.Validators.RegisterRequestValidator>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerCreateValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------- DbContext ----------
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(cs, new MySqlServerVersion(new Version(5, 7, 0)),
        my => my.SchemaBehavior(MySqlSchemaBehavior.Ignore)));
// ---------- DI (Repositories/Services) ----------

// Auth + User + EmailVerification
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<AuthService>();

// Customers
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<CustomerService>();

// Vehicles
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleInsurance.Infrastructure.Vehicles.Services.VehicleService>();
// Estimates
builder.Services.AddScoped<IEstimateService, EstimateService>();


// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// ===== VEHICLE MODULE =====


// ---------- AutoMapper ----------



// IEmailSender: dùng Gmail SMTP trong Development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailSender, GmailSmtpEmailSender>(); // đọc keys Email:* từ IConfiguration
}
else
{
    // TODO: thay bằng sender cho production (SendGrid/SES hoặc Gmail API OAuth2)
    builder.Services.AddScoped<IEmailSender, GmailSmtpEmailSender>();
}

// ---------- JWT ----------
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var accessMinutes = int.TryParse(builder.Configuration["Jwt:AccessTokenMinutes"], out var m) ? m : 15;

builder.Services.AddSingleton(new JwtTokenService(jwtIssuer, jwtAudience, jwtSecret, TimeSpan.FromMinutes(accessMinutes)));
builder.Services.AddScoped<AuthService>();

// Chuẩn hoá lỗi model binding
builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors?.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var payload = new
        {
            error = new { code = ErrorCodes.BadRequest, message = "Request is invalid", errors },
            traceId = context.HttpContext.TraceIdentifier
        };
        return new BadRequestObjectResult(payload);
    };
});

// AuthN/AuthZ
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,  // 👈 Bắt buộc
        NameClaimType = System.Security.Claims.ClaimTypes.Name,

        ClockSkew = TimeSpan.FromSeconds(30)
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            if (ctx.Request.Cookies.TryGetValue("access_token", out var token) && !string.IsNullOrEmpty(token))
                ctx.Token = token;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ====== Build app AFTER all Add...() ======
var app = builder.Build();
app.Lifetime.ApplicationStarted.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
    Console.WriteLine(">>> IEmailSender impl = " + sender.GetType().FullName);
});

// ---------- Middlewares ----------
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // app.UseDeveloperExceptionPage(); // log lỗi chi tiết
}

// app.UseHttpsRedirection(); // tắt tạm khi debug nếu cần

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ===== DEBUG endpoints =====
app.MapGet("/ping", () => Results.Text("pong"));
app.MapGet("/__routes", (EndpointDataSource es) =>
{
    var routes = es.Endpoints
        .OfType<RouteEndpoint>()
        .Select(e => new
        {
            Pattern = e.RoutePattern.RawText,
            Methods = string.Join(",", e.Metadata.OfType<HttpMethodMetadata>().SelectMany(m => m.HttpMethods))
        });
    return Results.Json(routes, new JsonSerializerOptions { WriteIndented = true });
});
// ===========================

app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        var sp = app.Services;
        var partManager = sp.GetRequiredService<Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartManager>();
        var controllerFeature = new Microsoft.AspNetCore.Mvc.Controllers.ControllerFeature();
        partManager.PopulateFeature(controllerFeature);

        Console.WriteLine("=== MVC Controllers discovered ===");
        foreach (var c in controllerFeature.Controllers)
            Console.WriteLine($" - {c.FullName}");

        var es = sp.GetRequiredService<EndpointDataSource>();
        Console.WriteLine("=== Endpoints mapped ===");
        foreach (var e in es.Endpoints.OfType<RouteEndpoint>())
        {
            var methods = string.Join(",", e.Metadata.OfType<HttpMethodMetadata>().SelectMany(m => m.HttpMethods));
            Console.WriteLine($" - {methods} {e.RoutePattern.RawText}");
        }
        Console.WriteLine("==================================");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed to dump controllers/endpoints: " + ex);
    }
});

app.Run();
