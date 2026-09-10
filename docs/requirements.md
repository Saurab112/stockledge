# StockLedger — Requirements

**Version:** 0.1 — Requirements & Domain Design
**Status:** Living document — Phase 0 (Design)
**Scope:** Single-company, multi-warehouse inventory management system

> A ledger-first inventory system — every stock change is an immutable event, not a mutable number.

This document tracks confirmed decisions and open questions separately, so assumptions are never mistaken for requirements. 

---

## 1. System Overview

StockLedger manages:

- Products, categories, and product variants
- Units of measure and conversions
- Warehouse stock (multi-warehouse, single company)
- Purchasing/receiving
- Sales
- Purchase returns 
- Sale returns 
- Stock transfers between warehouses
- Stock adjustments
- FIFO inventory costing
- Approval workflows for sensitive operations
- Full auditability and stock consistency

The goal is **correct and traceable inventory management**, not just CRUD.

---

## 2. Architecture 

| Decision | Value |
|---|---|
| Deployment | Monolithic — single app, single primary database |
| Modularity | One Inventory module, organized internally by feature (not separate services) |
| Layering | API → Application → Domain ← Infrastructure |
| Multi-tenancy | Out of scope |

Functional areas within the monolith: Products, Categories, Warehouses, Purchasing, Sales, Purchase Returns, Sale Returns, Stock Transfers, Stock Adjustments, Reporting.

---

## 3. Company & Warehouse 

- One company, multiple warehouses.
- Stock is tracked **per warehouse** — quantity in Warehouse A has no relationship to quantity in Warehouse B.
- Warehouse is the finest tracked location unit for v1 (no bin/shelf-level tracking).

---

## 4. Product Management

### 4.1 Product 


### 4.2 Categories —  Open


### 4.3 Product Variants —  Open (next design topic)

Variants are supported (e.g. T-Shirt → Red/Small, Red/Medium). Not yet decided: fixed attribute columns vs. a dynamic attribute model. The stockable unit must remain unambiguous regardless of approach — this decision drives the core schema and blocks the ERD.


## 5. Unit of Measure 
- we will have the base UOM for each product, and allow conversion to other UOMs.
- UOM conversion required (e.g. 1 Carton = 10 Boxes = 120 Pieces).
- Products transact in different UOMs while maintaining a consistent base-UOM inventory quantity.
- **Open:** 
- everything is not decided yet .


---

## 6. Inventory Tracking Model 

**Decision: Immutable ledger + derived balance (Option C).** See ADR (to be written): *Inventory Ledger vs Balance Model*.

### StockLedgerEntry (immutable, append-only) — source of truth



### StockBalance (mutable, derived/projected) — read-optimized cache



## 7. Negative Stock 

Stock must never go negative. This is enforced **transactionally at the ledger-write boundary** (inside the same DB transaction that writes the ledger entry and updates the balance), not as a UI-level or pre-check validation — pre-checks fail under concurrent requests.

**Open:** exact concurrency mechanism (optimistic `RowVersion` vs. pessimistic locking vs. serializable transaction isolation). This is the next major design topic after Product/Variant.

---

## 8. Purchasing / Receiving 

- Flow: Supplier → Receive → Warehouse → stock increases.
- Creates ledger IN-entries (FIFO layers) and batch/lot info where applicable.
- **No approval required.**

## 9. Sales 

- Flow: Warehouse → Sale → stock decreases.
- Consumes FIFO layers via ledger OUT-entries; enables cost-of-goods-sold calculation.
- **No approval required.**

## 10. Purchase Returns

## 11. Sales Returns

## 12. FIFO Costing 

First-in-first-out via ledger IN-entries acting as cost layers. Supports inventory valuation, COGS and remaining inventory cost.

## 13. Stock Transfer - Proposed, not locked

- **Approval required.**
- Three-phase stock movement — not a naive atomic move:
  ```
  Source Warehouse  -Qty
        ↓
  In-Transit         +Qty
        ↓ (on receipt)
  In-Transit         -Qty
  Destination        +Qty
  ```
- Proposed lifecycle: `Draft → Submitted → Approved → Dispatched → In Transit → Received`. Cancellation/rejection states not yet finalized.

## 14. Stock Adjustment —  Proposed, not locked

- Used when physical count differs from system quantity.
- Proposed: requires approval, lifecycle `Draft → Submitted → Approved → Applied`.
- **Open:** confirm approval requirement formally.

## 15. Approval  

Not every operation needs approval — it's a control reserved for sensitive/high-risk operations, not a blanket workflow.

| Operation | Approval |
|---|---|
| Purchase | ❌ No |
| Sale | ❌ No |
| Purchase Return | ❌ No |
| Sale Return | ❌ No |
| Stock Transfer | ✅ Yes |
| Stock Adjustment |  Proposed (yes) |

**Open:** actor/role model — who is permitted to approve. Blocks finalizing the approval workflow design.

## 16. Stock Reservations — ❌ Out of scope (v1)

Not implemented initially. Revisit if a Sales Order → Fulfillment workflow is introduced later.

---

## 17. Explicitly Out of Scope (v1)

- Multi-tenancy
- Microservices
- Serial number tracking
- Stock reservations
- Approval on every transaction
- Bin/shelf-level location tracking
- AI-based inventory features
- Complex distributed architecture

---

## 18. Open Design Decisions — tracked as GitHub issues

| # | Decision | Status | Issue | ADR |
|---|---|---|---|---|
| 1 | Product/Variant data model | Next up | — | — |
| 2 | Concurrency control mechanism | Not started | — | — |
| 3 | Actor/role model (who can approve) | Not started | — | — |
| 4 | Stock adjustment approval — confirm | Not started | — | — |
| 5 | UOM change/conversion immutability rule | Leaning decided, not formalized | — | — |
| 6 | Category hierarchy vs. flat | Not started | — | — |
| 7 | StockBalance batch-level granularity | Not started | — | — |

---

## 19. Core Domain (conceptual, not final schema)

```
Company
 │
 ├── Warehouse
 │
 ├── Product
 │     ├── Category
 │     ├── Variant
 │     └── UOM Group 
 │     └── UOM 
 │
 ├── Purchase         → writes StockLedgerEntry (IN)
 ├── Sale             → writes StockLedgerEntry (OUT)
 ├── Purchase Return  → writes StockLedgerEntry (OUT)
 ├── Sale Return      → writes StockLedgerEntry (IN)
 ├── Stock Transfer   → writes StockLedgerEntry (OUT + IN, phased)
 ├── Stock Adjustment → writes StockLedgerEntry (IN/OUT)
 │
 └── Inventory
       ├── StockLedgerEntry  (immutable, source of truth)
       ├── StockBalance      (derived, cached)
```