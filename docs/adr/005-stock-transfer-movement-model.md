# ADR-005: Stock Transfer Movement Model

## Status

Accepted

## Context

A stock transfer moves inventory from one warehouse to another.

The simplest approach is to decrease stock in the source warehouse and increase stock in the destination warehouse as a single operation. However, this does not accurately represent inventory while the goods are physically between warehouses.

Once stock has left the source warehouse, it may take time to arrive at the destination. During this period, the inventory should not be considered available at either warehouse. The system should also provide a clear point at which the destination can confirm what was actually received.

## Decision

A stock transfer will be modeled using three inventory stages:

```text
Source Warehouse
       ↓
   In Transit
       ↓
Destination Warehouse
```

When a transfer is dispatched, the transferred quantity is removed from the source warehouse and moved into an in-transit state.

When the destination warehouse confirms receipt, the quantity is removed from in-transit and added to the destination warehouse.

The transfer must be approved before it can be dispatched.

The exact transfer workflow and status transitions will be defined in the system's business rules.

## Consequences

### Positive

* The system can distinguish stock at the source, stock in transit, and stock received at the destination.
* Stock that has left the source is not incorrectly shown as available there.
* Stock that has not yet been received is not incorrectly shown as available at the destination.
* The destination has an explicit receiving step, allowing the system to account for what was actually received.
* Transfer history provides a clear record of the movement between warehouses.

### Tradeoffs

* Transfers require more state and workflow management than a direct warehouse-to-warehouse update.
* The system must handle transfers that remain in transit.
* The receiving workflow must account for differences between dispatched and received quantities if partial receipt is supported.
* Transfer operations require careful transaction handling to keep the source, in-transit, and destination quantities consistent.

This additional complexity is acceptable because accurately representing inventory location is more important than making transfers appear as a single instantaneous operation.
