using StockLedge.Api.ExceptionHandling;
using StockLedge.Data.DiConfig;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.DbContextDiConfig(builder.Configuration);
builder.Services.AddRepositoriesDiConfig();


builder.Services.AddControllers();

builder.Services.AddProblemDetails(options =>
{
	options.CustomizeProblemDetails = context =>
	{
		context.ProblemDetails.Instance = context.HttpContext.Request.Path;
	};
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
