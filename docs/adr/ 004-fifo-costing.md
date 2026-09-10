# ADR-004: FIFO Costing

## Status

Accepted

## Context

The system needs a costing method to calculate the cost of goods sold (COGS) and the value of remaining inventory.

Common inventory costing approaches include FIFO (First In, First Out), LIFO (Last In, First Out), and weighted average cost.

FIFO was selected because it provides a clear cost-flow method for tracking inventory layers and is particularly suitable for inventory where older stock should generally be consumed before newer stock, such as products with expiry dates or a risk of becoming obsolete.

## Decision

Inventory will be costed using FIFO.

Each incoming stock movement that adds costed inventory will create a FIFO cost layer containing:

* Quantity received
* Unit cost
* Remaining quantity
* Reference to the originating stock movement

When inventory is removed, the oldest available cost layer will be consumed first. If the requested quantity exceeds the remaining quantity of the oldest layer, the system will continue consuming subsequent layers until the entire quantity has been fulfilled.

For example:

```text
Layer 1: 100 units @ Rs. 100
Layer 2:  50 units @ Rs. 120

Sale: 120 units

100 units → Layer 1
 20 units → Layer 2
```

The cost of the sale is therefore calculated from the individual FIFO layers consumed rather than from a single average unit cost.

## Consequences

### Positive

* Provides a deterministic method for calculating COGS.
* Preserves the cost history of incoming inventory.
* Supports inventory valuation based on the remaining cost layers.
* Works naturally with the append-only stock ledger.
* Handles inventory with different purchase costs without losing cost-layer information.

### Tradeoffs

* A single stock removal may consume multiple cost layers.
* Stock deduction logic is more complex than using a single average cost.
* Cost layers must be tracked accurately as inventory is added and removed.
* The system must handle partial consumption of a layer and continue to the next layer when necessary.

FIFO costing depends on the stock ledger retaining the history of incoming inventory movements. Because the ledger is immutable, the system can reliably determine the order in which cost layers were created.

The additional complexity is acceptable because accurate inventory costing and traceability are important requirements of the system.
