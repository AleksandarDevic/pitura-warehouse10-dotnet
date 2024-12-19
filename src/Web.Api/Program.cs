using Serilog;
using Application;
using Infrastructure;
using Web.Api.Extensions;
using System.Reflection;
using Asp.Versioning.Builder;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration)
);

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddPresentation();

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app
    .MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

app.MapEndpoints(versionedGroup);

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwaggerWithUi();
}

app.UseHttpsRedirection();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

public partial class Program;
