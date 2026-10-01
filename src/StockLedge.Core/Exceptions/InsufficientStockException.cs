using System;
using System.Collections.Generic;
using System.Text;

namespace StockLedge.Domain.Exceptions
{
	
	public class InsufficientStockException : DomainException
	{
		public InsufficientStockException(
				decimal requested,
				decimal available)
				: base(
					$"Insufficient stock. Requested: {requested}, Available: {available}.")
		{
		}
	}
}
