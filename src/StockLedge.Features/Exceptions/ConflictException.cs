using StockLedge.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockLedge.Application.Exceptions
{
	public class ConflictException : BaseException
	{
		public ConflictException(string message) : base(message)
		{
		}
	}
}
