using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using scommon;
using scommon_sample_web_api.Caching;
using scommon_sample_web_api.Contexts;
using scommon_sample_web_api.Dependencies;
using scommon_sample_web_api.Exceptions;
using scommon_sample_web_api.Validations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using scommon_sample_web_api.Auths;


var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .Build();
builder.Services.AddSingleton(config);

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
//Global exception handling
builder.Services.AddGlobalExceptionHandler();

//Mediator & Pipelines
builder.Services.AddMediator(v =>
{
    v.AddPipelineForContextTransaction<SampleContext>(conf =>
    {
        conf.BeginTransactionOnCommand = false;//InMemory Db transaction scope desteklemediği için false yapıyorum.
        conf.BeginTransactionOnEvent = false;
    });

    v.AddPipelineForValidation(validationOptions =>
    {
        validationOptions.ValidateCommand = true;
        validationOptions.ValidateEvent = true;
        validationOptions.ValidateQuery = true;
    });
    
    v.AddPipelineForLogging(conf =>
    {
        conf.LogCommand = true;
        conf.LogCommandResult = false;
        conf.Level = LogLevel.Information;
        conf.LogEvent = false;
        conf.LogQuery = true;
        conf.LogQueryResult = false;
    });

    v.AddHandlersFromAssemblyOf<Program>();
});

//Dependencies
builder.Services.AddAllDependencies();

//Request validations
builder.Services.AddRequestValidators();

//DbContext
builder.Services.AddInMemoryContext();

//Caching
builder.Services.AddCachingServices();

//Health Check
builder.Services.AddHealthChecks();

//Swagger
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "Scommon Sample WEB API", 
    Version = "v1"
}); });

//Auth
builder.Services.LoadCurrentUser();

//Http
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

//Cors
builder.Services.AddCors();

//Controller
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

var app = builder.Build();
//Health Check

app.MapHealthChecks("/health");

//CORS
app.UseCors(policyBuilder => policyBuilder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// UpdateDatabase(app);

//Swagger
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample v1"));

app.UseRouting();
app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
app.Run();


 static void UpdateDatabase(IApplicationBuilder app)
{
    //TODO: move to extension
    using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
    var context = serviceScope.ServiceProvider.GetService<SampleContext>();
    context!.Database.Migrate();
}