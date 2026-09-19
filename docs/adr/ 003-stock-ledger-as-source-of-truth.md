# ADR-003: Stock Ledger as Source of Truth

## Status

Accepted

## Context

The system needs both a complete history of stock movements and an efficient way to determine current available stock.

A ledger provides historical traceability and supports FIFO costing, while calculating current stock entirely from the ledger would make frequent stock checks more expensive.

A balance provides fast reads but cannot replace the historical movement information required for auditability and FIFO costing.

## Decision

The system will use an **append-only Stock Ledger as the historical source of truth** and a **Stock Balance as the derived current-state representation**.

Every stock-changing operation creates one or more Stock Ledger entries.

Ledger transaction facts are immutable. Corrections must be represented through new inventory movements rather than modifying historical entries.

Stock Balance maintains current inventory for:

```text
ProductVariant + Warehouse + UOM
```

Stock Balance contains:

```text
Quantity
ReservedQuantity
```

where:

```text
AvailableQuantity = Quantity - ReservedQuantity
```

`ReservedQuantity` represents stock committed to a pending operation but not yet physically moved.

FIFO inventory layers are represented through inbound Stock Ledger entries using `RemainingBaseQuantity` as mutable inventory-layer state.

Stock Ledger and Stock Balance changes must be performed within the same database transaction.

The Stock Balance must be recoverable or reconcilable from the Stock Ledger.

## Consequences

### Positive

* Complete and auditable history of stock movements.
* Efficient current-stock reads.
* Supports FIFO inventory layers.
* Supports stock reservations without treating reservations as physical movements.
* Historical transaction facts remain immutable.
* Stock Balance can be reconciled against the ledger.

### Tradeoffs

* Stock-changing operations must maintain both ledger and balance.
* Transaction boundaries are critical for consistency.
* Reservations add additional state to Stock Balance.
* FIFO layers require mutable `RemainingBaseQuantity`.
* A reconciliation mechanism is needed to detect unexpected differences.

The additional complexity is acceptable because inventory correctness, traceability, FIFO costing, and efficient stock availability checks are core requirements.
