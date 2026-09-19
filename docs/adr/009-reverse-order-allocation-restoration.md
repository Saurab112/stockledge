# ADR-009: Reverse-Order Allocation Restoration on Sales Returns

## Status

Accepted

## Context

A Sale may consume inventory from multiple FIFO layers.

For example:

```text
FIFO Layers

Layer A: 10 units @ 100
Layer B: 10 units @ 120
```

A sale of 15 units consumes:

```text
Layer A → 10 units
Layer B → 5 units
```

The Sale therefore has multiple `StockLedgerAllocation` records.

When part of the sale is returned, the system must determine which original FIFO allocations should be restored.

Possible approaches include:

1. Restore from the oldest allocation first.
2. Restore from the newest allocation first.
3. Create a new FIFO layer using the current cost.
4. Restore the original allocations.

Creating a new layer with the current cost would lose the original cost relationship. Restoring the original allocations preserves the relationship between the Sale and the inventory layers it consumed.

## Decision

Sales Returns restore the original FIFO allocations in **reverse allocation order**.

The return uses the original Sale and its `StockLedgerAllocation` records to determine which inbound FIFO layers are restored.

For example:

```text
Original Sale

Layer A → 10 units
Layer B → 5 units

Sales Return: 5 units

Restore:
Layer B → 5 units
```

If a larger quantity is returned:

```text
Original Sale

Layer A → 10 units
Layer B → 5 units

Sales Return: 12 units

Restore:
Layer B → 5 units
Layer A → 7 units
```

The returned stock therefore restores the most recently consumed allocation first.

The original Sale, outbound Stock Ledger entry, and allocation records remain immutable.

The Sales Return creates a new inbound Stock Ledger entry and updates the affected FIFO layers by increasing their `RemainingBaseQuantity`.

The restoration and balance update must occur atomically.

## Consequences

### Positive

* Preserves the original FIFO cost relationship.
* Maintains traceability between the Sale, its allocations, and the returned inventory.
* Avoids inventing a new cost for returned inventory.
* Works when a Sale consumed stock from multiple FIFO layers.
* Keeps the original Sale and ledger history immutable.

### Tradeoffs

* Sales Returns require access to the original allocation records.
* Partial returns require allocation-level quantity tracking.
* The method represents a costing assumption rather than proof of the exact physical units returned, since the system does not track serial numbers or physical item identity.

## Example

```text
Inbound Layers

Layer A: 10 units @ 100
Layer B: 10 units @ 120

Sale: 15 units

Allocation:
Layer A → 10
Layer B → 5

Remaining:
Layer A → 0
Layer B → 5
```

If the customer returns 8 units:

```text
Sales Return

Restore Layer B → 5
Restore Layer A → 3
```

Result:

```text
Layer A → 3 remaining
Layer B → 10 remaining
```

The return therefore reverses the original consumption order rather than creating a new FIFO cost layer.
