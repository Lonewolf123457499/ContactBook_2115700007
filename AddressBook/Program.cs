using BusinessLayer.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModelLayer.Mapper;
using RepositoryLayer.Service;
using AutoMapper;
using FluentValidation.AspNetCore;
using FluentValidation;
using AddressBook.HelperService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AddressBook.Cache;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

try
{
    Console.WriteLine(" Starting application...");

    // Load configuration
    var configuration = builder.Configuration;

    // Get Redis connection string safely
    var redisConnectionString = configuration.GetConnectionString("RedisURL") ?? "127.0.0.1:6379";

    // Register Redis Connection Multiplexer with Exception Handling
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        try
        {
            var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            Console.WriteLine(" Redis Connected Successfully!");
            return multiplexer;
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Redis Connection Failed: {ex.Message}");
            throw; // Ensure the application stops if Redis is critical
        }
    });

    // Register Cache Service
    builder.Services.AddSingleton<ICacheService, CacheService>();

    // Add FluentValidation services
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<AddressBookEntryDTOValidator>();

    // Add database context with Exception Handling
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        try
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Database connection string is missing"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Database Connection Failed: {ex.Message}");
            throw;
        }
    });

    // Get JWT values safely
    var jwtKey = configuration.GetValue<string>("Jwt:Key") ?? throw new Exception("JWT Key is missing");
    var jwtIssuer = configuration.GetValue<string>("Jwt:Issuer") ?? throw new Exception("JWT Issuer is missing");

    // Configure JWT Authentication with Logging
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($" Token authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                }
            };
        });

    // Add Controllers
    builder.Services.AddControllers();

    // Add AutoMapper
    builder.Services.AddAutoMapper(typeof(MappingProfile));

    // Dependency Injection
    builder.Services.AddScoped<AddressBookRL>();
    builder.Services.AddScoped<AddressBookBL>();
    builder.Services.AddScoped<TokenService>();
    builder.Services.AddScoped<EmailService>();

    // RabbitMQ Producer and Consumer
    builder.Services.AddSingleton<RabbitMQService>(); // Publisher
    builder.Services.AddHostedService<RabbitMQConsumer>(); // Consumer runs in background

    // Swagger configuration
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure Middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Console.WriteLine(" Application Running...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($" Application Startup Failed: {ex.Message}");
    throw;
}
