using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace StockLedge.Api.ExceptionHandling
{
	public class GlobalExceptionHandler : IExceptionHandler
	{
		private readonly ILogger<GlobalExceptionHandler> _logger;

		public GlobalExceptionHandler(
			ILogger<GlobalExceptionHandler> logger)
		{
			_logger = logger;
		}

		public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
		{
			_logger.LogError(exception, "An unhandled exception occured");

			var statusCode = ExceptionMapping.GetStatusCode(exception);

			httpContext.Response.StatusCode = statusCode;

			await httpContext.Response.WriteAsJsonAsync(
				new ProblemDetails
				{
					Status = statusCode,
					Title = exception.Message
				},
				cancellationToken
				);

			return true;
		}
	}
}
