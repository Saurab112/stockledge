using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StockLedge.Application.Exceptions;
using StockLedge.Domain.Exceptions;

namespace StockLedge.Api.ExceptionHandling
{
	public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger,
	IProblemDetailsService _problemDetailsService,
	IHostEnvironment _environment) : IExceptionHandler
	{

		public async ValueTask<bool> TryHandleAsync(
			HttpContext httpContext,
			Exception exception,
			CancellationToken cancellationToken)
		{
			if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
			{
				return true;
			}

			var mapping = ExceptionMapping.Resolve(exception);

			LogException(exception, mapping.StatusCode);

			httpContext.Response.StatusCode = mapping.StatusCode;

			var problemDetails = BuildProblemDetails(exception, mapping);

			return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
			{
				HttpContext = httpContext,
				ProblemDetails = problemDetails,
				Exception = exception
			});
		}

		private void LogException(Exception exception, int statusCode)
		{
			if (statusCode >= 500)
			{
				_logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
			}
			else
			{
				_logger.LogWarning("Handled exception ({StatusCode}): {Message}", statusCode, exception.Message);
			}
		}

		private ProblemDetails BuildProblemDetails(Exception exception, ProblemMapping mapping)
		{
			if (exception is AppValidationException validationException)
			{
				return new ValidationProblemDetails(validationException.Errors)
				{
					Status = mapping.StatusCode,
					Title = mapping.Title,
					Type = mapping.TypeUri,
					Detail = validationException.Message
				};
			}

			var problemDetails = new ProblemDetails
			{
				Status = mapping.StatusCode,
				Title = mapping.Title,
				Type = mapping.TypeUri,
				Detail = mapping.StatusCode >= 500
					? "An unexpected error occurred. Please try again later."
					: exception.Message
			};

			if (exception is BaseException baseException)
			{
				foreach (var (key, value) in baseException.Extensions)
				{
					problemDetails.Extensions[key] = value;
				}
			}

			if (_environment.IsDevelopment() && mapping.StatusCode >= 500)
			{
				problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
				problemDetails.Extensions["stackTrace"] = exception.StackTrace;
			}

			return problemDetails;
		}

	}
}
