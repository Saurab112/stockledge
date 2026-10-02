using StockLedge.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockLedge.Application.Exceptions
{
	public sealed class AppValidationException : BaseException
	{
		public IDictionary<string, string[]> Errors { get; }

		public AppValidationException(IDictionary<string, string[]> errors)
			: base("One or more validation errors occurred.")
		{
			Errors = errors;
		}
	}
}
