using Microsoft.AspNetCore.Mvc;

namespace StockLedge.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase
	{
		private static readonly string[] Summaries =
		[
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		];

		[HttpGet(Name = "GetWeatherForecast")]
		public IActionResult Get()
		{
			return Ok("Hello there");
		}
	}
}
