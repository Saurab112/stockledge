using FluentValidation;
using MediatR;

namespace StockLedge.Application.Behaviors
{
	public class ValidationBehavior<TRequest, TResponse> 
		: IPipelineBehavior<TRequest, TResponse>
	{
		private readonly IEnumerable<IValidator<TRequest>> _validators;

		public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
		{
			_validators = validators;
		}

		public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
		{
			if (!_validators.Any())
			{
				return await next();
			}

			var results = await Task.WhenAll(
			_validators.Select(
				validator => validator.ValidateAsync(
					request,
					cancellationToken)));

			var failures = results
				.SelectMany(x => x.Errors)
				.ToList();

			if (failures.Any())
			{
				throw new ValidationException(failures);
			}

			return await next();
		}
	}
}
