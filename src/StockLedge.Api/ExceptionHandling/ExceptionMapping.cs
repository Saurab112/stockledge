using Microsoft.EntityFrameworkCore;
using StockLedge.Application.Exceptions;
using StockLedge.Domain.Exceptions;

namespace StockLedge.Api.ExceptionHandling
{
	public sealed record ProblemMapping(int StatusCode, string Title, string TypeUri);

	public static class ExceptionMapping
	{
		private const string ProblemBaseUri = "https://stockledge.dev/problems";
		public static ProblemMapping Resolve(Exception exception) => exception switch
		{
			AppValidationException =>
				new ProblemMapping(StatusCodes.Status400BadRequest, "Validation failed", $"{ProblemBaseUri}/validation-failed"),

			NotFoundException =>
				new ProblemMapping(StatusCodes.Status404NotFound, "Resource not found", $"{ProblemBaseUri}/not-found"),

			ConflictException =>
				new ProblemMapping(StatusCodes.Status409Conflict, "Conflict", $"{ProblemBaseUri}/conflict"),

			DbUpdateConcurrencyException =>
				new ProblemMapping(StatusCodes.Status409Conflict, "Concurrent update detected", $"{ProblemBaseUri}/concurrency-conflict"),

			InsufficientStockException =>
				new ProblemMapping(StatusCodes.Status422UnprocessableEntity, "Insufficient stock", $"{ProblemBaseUri}/insufficient-stock"),

			// Catch-all for any other domain rule violation not mapped above.
			DomainException =>
				new ProblemMapping(StatusCodes.Status422UnprocessableEntity, "Business rule violated", $"{ProblemBaseUri}/business-rule-violation"),

			// Fallback: anything unexpected becomes a generic 500.
			_ =>
				new ProblemMapping(StatusCodes.Status500InternalServerError, "An unexpected error occurred", $"{ProblemBaseUri}/server-error")
		};
	}
}
