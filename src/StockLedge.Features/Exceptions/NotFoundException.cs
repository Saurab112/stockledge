using StockLedge.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockLedge.Application.Exceptions
{
	public class NotFoundException : BaseException
	{
		public NotFoundException(string message) : base(message)
		{
		}
	}
}
