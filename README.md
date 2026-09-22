# StockLedger

A ledger-first inventory management system. Stock changes are recorded as immutable movements; current stock is derived from those movements, never edited directly.

Single-company, multi-warehouse, FIFO-costed, with configurable approval workflows for transfers and adjustments.

> **Work in progress** — design and business rules are finalized; implementation is underway.

## Why StockLedger?

Inventory systems look simple on the surface but get complex fast once you add multiple warehouses, mixed units of measure, FIFO costing, transfers, returns, reservations, and corrections.

StockLedger is designed domain-first: requirements and business rules were written before any code, and the design decisions — including the ones rejected along the way — are documented as they were made.

The goal isn't an inventory CRUD app. It's to show how a real backend system gets analyzed, designed, and reasoned about.

**Covers:** multi-warehouse stock, Product/ProductVariant modeling, UOM-aware tracking, an append-only Stock Ledger with a derived Stock Balance, FIFO costing and allocation, purchases, sales, returns, transfers, adjustments, configurable approvals, reservations, and concurrency-safe updates with negative-stock prevention.

## Core domain

```text
Company
 │
 ├── Warehouse
 │
 ├── Product
 │    ├── Category
 │    └── ProductVariant
 │         └── UOM Group
 │              └── UOM
 │
 ├── Purchase        → Stock Ledger (IN)
 ├── Sale            → Stock Ledger (OUT)
 ├── Purchase Return  → Stock Ledger (OUT)
 ├── Sales Return     → Stock Ledger (IN)
 ├── Stock Transfer   → Stock Ledger (OUT + IN)
 ├── Stock Adjustment → Stock Ledger (IN/OUT)
 │
 └── Inventory
      ├── Stock Ledger
      ├── Stock Ledger Allocation
      └── Stock Balance
```

Stock identity is always `ProductVariant + Warehouse + UOM`. The Stock Ledger is the historical source of truth; Stock Balance is a derived, reconcilable current-state view.

```text
Purchase                    Sale
   ↓                           ↓
Stock Ledger              FIFO Allocation
   ↓                           ↓
FIFO Layer                 Stock Ledger
   ↓                           ↓
Stock Balance              Stock Balance
```

## Architecture

Modular monolith, layered: **API → Application → Domain → Infrastructure**. Kept intentionally as a monolith — the project doesn't need the operational cost of distributed services (see [ADR-001](./docs/adr/001-monolithic-layered-architecture.md)).

## Design highlights

**Ledger-first inventory.** Every stock-changing operation creates an immutable ledger movement. Nothing historical is edited — corrections are new movements. → [ADR-003](./docs/adr/%20003-stock-ledger-as-source-of-truth.md)

**FIFO costing.** Inbound stock creates cost layers; outbound stock consumes the oldest eligible layer first, possibly spanning several layers. `StockLedgerAllocation` records exactly which inbound layers fed each outbound movement. → [ADR-004](./docs/adr/%20004-fifo-costing.md)

**UOM-aware inventory.** The same variant can hold separate balances per unit (e.g. `Frooti / Warehouse A` tracks `Box → 10` and `Piece → 100` independently) — a transaction in one UOM never silently draws from another. → [ADR-007](./docs/adr/007-uom-aware-inventory-identity.md)

**Configurable approvals.** Transfers and adjustments can run Direct or Approval Required. Pending transfers reserve source stock without moving it; approval revalidates before the movement is applied — same inventory rules either way. → [ADR-008](./docs/adr/008-configurable-approval-workflow.md)

**Traceable returns.** Sales Returns restore the original FIFO allocations in reverse allocation order, preserving the exact cost relationship between the sale and the layers it consumed. → [ADR-009](./docs/adr/009-reverse-order-allocation-restoration.md)

**Product/Variant modeling.** Variants carry name + SKU rather than structured color/size attributes — a deliberate simplicity trade-off over a generic attribute model. → [ADR-006](./docs/adr/006-product-and-product-variant-modeling.md)

## Documentation

| Doc | Purpose |
|---|---|
| [`docs/requirements.md`](./docs/requirements.md) | System scope, architecture, domain model |
| [`docs/business-rules.md`](./docs/business-rules.md) | The formal, enforceable rule set |
| [`docs/system-design.md`](./docs/system-design.md) | Feature-by-feature rationale and workflow |
| [`docs/adr/`](./docs/adr) | All 9 decisions, including the alternatives that were rejected |

## Explicitly out of scope

Multi-tenancy, sub-warehouse (bin/shelf) locations, negative inventory, serial/batch/lot tracking, in-transit transfer state, dynamic product attributes, vendor/customer accounting, full general ledger functionality. Full list and reasoning in `docs/requirements.md §16`.

## Current Status

**Design:** requirements ✓ · business rules ✓ · architecture decisions ✓ · system design ✓ · database design — in progress · API design — not started

**Implementation:** not started — following directly from the design above.

## Project goals

This is a portfolio project focused on demonstrating backend system design, domain modeling, business-rule-driven development, transaction and consistency design, and technical decision-making — documenting *why* the system works the way it does, not just building features.

## License

For learning and portfolio purposes.