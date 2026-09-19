# ADR-007: UOM-Aware Inventory Identity

## Status

Accepted

## Context

Inventory cannot always be represented using a single unit for a Product Variant.

For example, the same product variant may have stock recorded as:

```text
Frooti
Warehouse A

10 Boxes
100 Pieces
```

The system must therefore distinguish stock recorded in different UOMs.

Two approaches were considered.

### Option 1 — ProductVariant + Warehouse

Treat all stock for a Product Variant within a warehouse as one balance and convert quantities into a common base UOM.

This simplifies the stock identity but allows stock recorded in one UOM to be used to satisfy transactions requested in another UOM.

It also makes UOM-specific stock availability difficult to represent.

### Option 2 — ProductVariant + Warehouse + UOM

Treat each UOM as a separate stock identity within a warehouse.

This allows the system to maintain independent balances for each UOM while still using the UOM Group's base UOM for normalization and FIFO calculations.

This option was chosen.

## Decision

The inventory identity will be:

```text
ProductVariant + Warehouse + UOM
```

A ProductVariant must be assigned a UOM Group.

Transactions may use only UOMs belonging to that ProductVariant's UOM Group.

Stock Balance will therefore maintain a separate balance for each ProductVariant, Warehouse, and UOM combination.

For example:

```text
ProductVariant: Frooti
Warehouse: A

Box   → 10
Piece → 100
```

Stock recorded in one UOM must not automatically satisfy a transaction requesting another UOM when the requested UOM does not have sufficient available stock.

UOM conversion is still supported within the same UOM Group. The system may normalize quantities to the group's base UOM for FIFO calculations and inventory-layer tracking.

Stock Ledger entries preserve both the original transaction UOM and the normalized base quantity:

```text
Quantity     → original transaction quantity
BaseQuantity → normalized quantity in base UOM
```

`RemainingBaseQuantity` is used to track the remaining quantity of an inbound FIFO layer.

Historical transaction quantities must remain unchanged if UOM configuration is changed later.

## Consequences

### Positive

* Stock availability can be determined for the exact requested UOM.
* Prevents unintended cross-UOM stock consumption.
* Provides a clear and consistent inventory identity.
* Supports multiple UOMs for the same Product Variant within a warehouse.
* Allows FIFO calculations to use normalized base quantities.
* Preserves the original UOM used by historical transactions.

### Tradeoffs

* Stock Balance contains separate records for different UOMs.
* Inventory validation must include UOM in addition to Product Variant and Warehouse.
* FIFO calculations require UOM normalization.
* UOM configuration changes require careful handling to preserve historical inventory data.

This additional complexity is acceptable because UOM-specific stock availability is a core requirement of the system and prevents ambiguous inventory consumption.
