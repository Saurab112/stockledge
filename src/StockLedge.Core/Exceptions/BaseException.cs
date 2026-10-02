using System;
using System.Collections.Generic;
using System.Text;

namespace StockLedge.Domain.Exceptions
{
	public abstract class BaseException : Exception
	{
		public IDictionary<string, object?> Extensions { get; } = new Dictionary<string, object?>();

		protected BaseException(string message) : base(message)
		{
		}

		protected BaseException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BaseException WithExtension(string key, object? value)
		{
			Extensions[key] = value;
			return this;
		}
	}
}
