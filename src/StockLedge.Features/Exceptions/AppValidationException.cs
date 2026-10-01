using StockLedge.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockLedge.Application.Exceptions
{
	public class AppValidationException : BaseException
	{
		public AppValidationException(string message) : base(message)
		{
		}
	}
}
