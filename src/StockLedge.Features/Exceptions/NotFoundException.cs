using StockLedge.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockLedge.Application.Exceptions
{
	public sealed class NotFoundException : BaseException
	{
		public NotFoundException(string entityName, object key)
				: base($"{entityName} '{key}' was not found.")
		{
			WithExtension("entity", entityName);
			WithExtension("key", key);
		}
	}
}
