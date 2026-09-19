# ADR-005: Stock Transfer Movement Model

## Status

Accepted

## Context

A stock transfer moves inventory from one warehouse to another.

An earlier design considered an in-transit inventory state where stock would move from the source warehouse into an intermediate state before being received by the destination.

This would introduce additional inventory state and workflow complexity that is not required by the current system.

The system instead needs to support two operational modes:

* Immediate transfers
* Approval-required transfers

Approval-required transfers also need to prevent the requested stock from being consumed by another operation while approval is pending.

## Decision

A Stock Transfer will use a **direct source-to-destination movement model** with configurable approval.

### Automatic Transfer

When configured for direct processing:

```text
Source Warehouse
      ↓
Transfer Out
      ↓
Transfer In
      ↓
Destination Warehouse
```

Both movements are created as part of the same atomic operation.

### Approval Required Transfer

When approval is required:

```text
Create Transfer
      ↓
Reserve Source Stock
      ↓
Pending
      ↓
Approve
      ↓
Release Reservation
      ↓
Transfer Out + Transfer In
      ↓
Confirmed
```

While pending, the reserved stock remains physically in the source warehouse but is unavailable to other stock-consuming operations.

If the transfer is rejected or cancelled, the reservation is released and no inventory movement is created.

Transfer Out consumes source inventory using FIFO and creates Stock Ledger Allocations.

Transfer In creates the corresponding destination inventory while preserving the cost composition of the transferred stock.

There is no in-transit inventory state in the current version.

## Consequences

### Positive

* Simpler transfer lifecycle.
* No additional in-transit inventory state.
* Supports both immediate and controlled transfers.
* Reserved stock cannot be consumed while awaiting approval.
* Source and destination changes remain atomic.
* FIFO cost information can be preserved across warehouses.

### Tradeoffs

* Physical transportation time is not represented as an inventory state.
* There is no separate destination receiving workflow.
* Future requirements for shipment tracking or partial receiving may require a new transfer model.

This tradeoff is acceptable because the current system focuses on inventory movement rather than logistics or shipment management.
