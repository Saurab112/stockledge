# ADR-004: FIFO Costing

## Status

Accepted

## Context

The system needs a costing method to calculate cost of goods sold (COGS) and the value of remaining inventory.

FIFO was selected because it provides a clear and deterministic cost-flow method while preserving the cost history of incoming inventory.

## Decision

Inventory will be costed using **FIFO (First In, First Out)**.

Each costed inbound Stock Ledger entry acts as a FIFO inventory layer and contains:

* Original quantity
* Base quantity
* Unit cost
* Remaining base quantity
* Originating transaction reference

When inventory is consumed, the oldest eligible layer is consumed first.

If one layer is insufficient, subsequent layers are consumed until the required quantity is fulfilled.

Example:

```text
Layer 1: 100 units @ Rs. 100
Layer 2:  50 units @ Rs. 120

Sale: 120 units

100 units → Layer 1
 20 units → Layer 2
```

The consumption of each layer is recorded through `StockLedgerAllocation`.

FIFO calculations respect:

```text
ProductVariant + Warehouse + UOM
```

UOM conversion may be used to normalize quantities into the UOM Group's base UOM for layer calculations.

`RemainingBaseQuantity` is mutable inventory-layer state, while the original Stock Ledger transaction facts remain immutable.

## Consequences

### Positive

* Deterministic COGS calculation.
* Preserves incoming inventory cost history.
* Supports remaining inventory valuation.
* Works naturally with the Stock Ledger.
* Provides traceability through Stock Ledger Allocation.

### Tradeoffs

* One stock-consuming transaction may consume multiple layers.
* Partial layer consumption must be tracked.
* FIFO calculations require careful UOM normalization.
* Return operations must restore quantities to the appropriate original layers.

The additional complexity is acceptable because inventory costing and traceability are core requirements of StockLedger.
