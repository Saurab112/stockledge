using System;
using System.Collections.Generic;
using System.Text;

namespace StockLedge.Domain.Exceptions
{
	
	public sealed class InsufficientStockException : DomainException
	{
		public string Sku { get; }
		public int Requested { get; }
		public int Available { get; }

		public InsufficientStockException(string sku, int requested, int available)
		: base($"Insufficient stock for '{sku}'. Requested {requested}, available {available}.")
		{
			Sku = sku;
			Requested = requested;
			Available = available;

			WithExtension("sku", sku);
			WithExtension("requested", requested);
			WithExtension("available", available);
		}
	}
}
