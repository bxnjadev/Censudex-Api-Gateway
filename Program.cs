using System;
using System.Text;
using DotNetEnv;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserProto;
using System.Net.Http.Headers;
using order_service;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Censudex API Gateway",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme =
    opt.DefaultChallengeScheme =
    opt.DefaultForbidScheme = 
    opt.DefaultScheme =
    opt.DefaultSignInScheme =
    opt.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
        ValidateAudience = true,
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? throw new ArgumentNullException("Signing key cannot be null or empty."))),
    };
    /// <summary>
    /// token validation event
    /// </summary>
    /// <value></value>
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async ctx =>
        {
            var httpFactory = ctx.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
            var client = httpFactory.CreateClient("AuthService");

            var token = ctx.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrWhiteSpace(token))
            {
                ctx.Fail("No token");
                return;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await client.GetAsync("api/login"); 
            if (!res.IsSuccessStatusCode)
            {
                ctx.Fail("Token revoked or invalid");
            }
        }
    };
});
builder.Services.AddControllers();

// register gRPC client
builder.Services.AddGrpcClient<UserService.UserServiceClient>(o =>
{
    o.Address = new Uri(Environment.GetEnvironmentVariable("CLIENTS_SERVICE_URL") ?? "http://localhost:5000");
});


builder.Services.AddGrpcClient<OrderGrpcService.OrderGrpcServiceClient>(o =>
{
    o.Address = new Uri(Environment.GetEnvironmentVariable("ORDERS_SERVICE_URL") ?? "http://localhost:5247");
});

builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5010/"); 
});
var app = builder.Build();

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
