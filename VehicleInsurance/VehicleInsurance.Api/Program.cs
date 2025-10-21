using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;                       // ApiBehaviorOptions
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;                   // <-- thêm
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using VehicleInsurance.Application.Common.Email;
using VehicleInsurance.Application.EmailVerification;
using VehicleInsurance.Infrastructure.Common.Email;
using VehicleInsurance.Infrastructure.EmailVerification;
using FluentValidation;
using FluentValidation.AspNetCore;
using VehicleInsurance.Application.Auth;
using VehicleInsurance.Application.Common.Errors;
using VehicleInsurance.Domain.Auth;
using VehicleInsurance.Domain.Users;
using VehicleInsurance.Infrastructure;
using VehicleInsurance.Infrastructure.Auth;
using VehicleInsurance.Infrastructure.Users;
using VehicleInsurance.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Logging nên cấu hình TRƯỚC khi Build()
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Controllers + FluentValidation (v11+)
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<VehicleInsurance.Api.Validators.RegisterRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext MySQL 5.7
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(cs, new MySqlServerVersion(new Version(5, 7, 0)),
        my => my.SchemaBehavior(MySqlSchemaBehavior.Ignore)));

// Repo + Service
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// JWT config
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var accessMinutes = int.TryParse(builder.Configuration["Jwt:AccessTokenMinutes"], out var m) ? m : 15;

builder.Services.AddSingleton(new JwtTokenService(jwtIssuer, jwtAudience, jwtSecret, TimeSpan.FromMinutes(accessMinutes)));
builder.Services.AddScoped<AuthService>();

// IEmailSender (constructor 5 tham số)
builder.Services.AddSingleton<IEmailSender>(sp =>
{
    var cfg = builder.Configuration;
    return new SmtpEmailSender(
        cfg["Smtp:Host"]!,
        int.TryParse(cfg["Smtp:Port"], out var p) ? p : 587,
        cfg["Smtp:User"]!,
        cfg["Smtp:Pass"]!,
        cfg["Smtp:From"]!
    );
});

builder.Services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();

// JwtBearer đọc token từ cookie "access_token"
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

// Chuẩn hóa lỗi 400 từ model binding
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
            error = new
            {
                code = ErrorCodes.BadRequest,
                message = "Request is invalid",
                errors
            },
            traceId = context.HttpContext.TraceIdentifier
        };

        return new BadRequestObjectResult(payload);
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Bắt lỗi thật sớm
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
