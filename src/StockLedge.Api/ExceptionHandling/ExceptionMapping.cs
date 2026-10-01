using FluentValidation;
using StockLedge.Application.Exceptions;
using StockLedge.Domain.Exceptions;

namespace StockLedge.Api.ExceptionHandling
{
	public static class ExceptionMapping
	{
		public static int GetStatusCode(Exception exception)
		{
			return exception switch
			{
				ValidationException =>
					StatusCodes.Status400BadRequest,

				NotFoundException =>
					StatusCodes.Status404NotFound,

				InsufficientStockException =>
					StatusCodes.Status409Conflict,

				ConflictException =>
					StatusCodes.Status409Conflict,

				_ =>
					StatusCodes.Status500InternalServerError
			};
		}
	}
}
