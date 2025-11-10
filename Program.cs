using System;
using DotNetEnv;
using Microsoft.OpenApi.Models;
using UserProto;

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
});
builder.Services.AddControllers();

// register gRPC client
builder.Services.AddGrpcClient<UserService.UserServiceClient>(o =>
{
    o.Address = new Uri(Environment.GetEnvironmentVariable("CLIENTS_SERVICE_URL") ?? "http://localhost:5000");
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
