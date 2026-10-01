using System;
using System.Collections.Generic;
using System.Text;

namespace StockLedge.Domain.Exceptions
{
	public abstract class DomainException : BaseException
	{
		protected DomainException(string message) : base(message)
		{
			
		}
	}
}
