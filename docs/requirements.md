# StockLedger — Requirements

 Requirements & Domain Design
**Scope:** Single-company, multi-warehouse inventory management system

> A ledger-first inventory system where stock changes are recorded as immutable movements and current stock is derived from those movements.

This document defines the confirmed system requirements. Detailed business rules and design decisions are documented separately.

---

## 1. System Overview

StockLedger manages:

* Products, categories, and product variants
* Units of measure and UOM groups
* Multi-warehouse inventory
* Purchasing and sales
* Purchase and sales returns
* Stock transfers
* Stock adjustments
* FIFO inventory costing
* Configurable approval workflows
* Inventory traceability and auditability

The goal is **correct, consistent, and traceable inventory management**, not just CRUD.

---

## 2. Architecture

| Decision      | Value                                                |
| ------------- | ---------------------------------------------------- |
| Deployment    | Monolithic — single application and primary database |
| Modularity    | One Inventory module organized by feature            |
| Layering      | API → Application → Domain ← Infrastructure          |
| Multi-tenancy | Out of scope                                         |

Functional areas include Products, Categories, Warehouses, Purchasing, Sales, Returns, Transfers, Adjustments, and Reporting.

---

## 3. Company & Warehouse

* The system supports one company with multiple warehouses.
* Stock is tracked independently for each warehouse.
* Warehouse is the smallest physical location tracked.
* Bin, shelf, and other sub-location tracking are not supported.

---

## 4. Product Management

### 4.1 Products & Categories

* Products are organized under categories.
* A Product represents the common definition of an item.
* Category hierarchy is not yet finalized.

### 4.2 Product Variants

* A Product may have one or more Product Variants.
* A Product Variant represents the stockable and sellable item.
* Each Product Variant has a unique SKU.
* A Product Variant is assigned a UOM Group.
* Transactions use a UOM belonging to that UOM Group.
* Dynamic product attribute models are out of scope for the current version.

---

## 5. Units of Measure

* UOMs are organized into UOM Groups.
* Each UOM Group has exactly one base UOM.
* The base UOM has a multiplier of `1`.
* Other UOMs define their multiplier relative to the base UOM.
* UOM conversion is supported within the same UOM Group.
* Different UOM Groups cannot be converted.
* Stock is tracked separately for each Product Variant, Warehouse, and UOM.
* The system does not automatically consume stock from another UOM.

---

## 6. Inventory Tracking Model

**Decision: Immutable Stock Ledger + Derived Stock Balance.**

### Stock Ledger

* The Stock Ledger is the historical source of truth for inventory movements.
* Ledger entries are append-only.
* Original transaction facts cannot be modified or deleted.
* Each stock-changing operation creates a ledger movement.
* Inbound movements can act as FIFO inventory layers.
* Outbound movements can be linked to consumed inbound layers through Stock Ledger Allocation.

### Stock Balance

* Stock Balance represents current inventory state.
* Stock is tracked by Product Variant, Warehouse, and UOM.
* Stock Balance maintains on-hand and reserved quantities.
* Stock Balance is derived and must remain consistent with the Stock Ledger.

---

## 7. Negative Stock

* Stock must never become negative.
* Stock-consuming operations must have sufficient available stock.
* Reserved stock cannot be consumed.
* Stock validation and inventory updates must be handled atomically.
* Concurrent operations must not bypass stock availability rules.

---

## 8. Purchasing

* Purchases record stock received from a Vendor into a Warehouse.
* Purchase Items contain Product Variant, UOM, Quantity, and Unit Cost.
* Confirmed purchases increase inventory.
* Confirmed purchases create inbound Stock Ledger entries.
* Received stock becomes FIFO inventory layers.
* Purchase does not require approval.
* Vendor payment and payable management are out of scope.

---

## 9. Sales

* Sales record stock sold from a Warehouse.
* Sale Items contain Product Variant, UOM, Quantity, and Unit Price.
* Customer association is optional.
* Confirmed sales decrease inventory.
* Sales consume available stock using FIFO.
* FIFO consumption is recorded through Stock Ledger Allocation.
* Selling price is independent of FIFO inventory cost.
* Sales do not require approval.

---

## 10. Returns

### Purchase Returns

* Return stock from the business to the Vendor.
* Reference the original Purchase and relevant inventory layer.
* Reduce the corresponding inventory and FIFO layer.

### Sales Returns

* Return stock from the Customer to the business.
* Reference the original Sale.
* Restore inventory against the original FIFO allocations.
* Increase the corresponding inventory and FIFO layers.

Both return types use the common Stock Ledger.

---

## 11. FIFO Costing

* FIFO is used for inventory costing.
* Inbound stock creates FIFO layers with an associated unit cost.
* Outbound stock consumes the oldest eligible layers first.
* A single outbound movement may consume multiple layers.
* FIFO allocations are recorded for traceability.
* FIFO costing supports inventory valuation and cost-of-goods-sold calculation.

---

## 12. Stock Transfers

* Transfers move stock between two different warehouses.
* A transfer contains one or more Transfer Items.
* Transfers preserve Product Variant, UOM, and Quantity.
* Transfers do not use an in-transit inventory state.
* Automatic transfers execute the source and destination movements immediately.
* Approval-required transfers reserve source stock until approval.
* Transfer Out consumes source FIFO layers.
* Transfer In creates corresponding destination inventory layers while preserving the transferred cost composition.
* Transfer execution is atomic.

---

## 13. Stock Adjustments

* Adjustments correct differences between physical and system stock.
* Adjustments can increase or decrease inventory.
* Positive adjustments require a Unit Cost and create a FIFO layer.
* Negative adjustments consume existing FIFO layers.
* Adjustments support configurable Direct or Approval Required workflows.
* Pending adjustments do not change inventory.
* Confirmed adjustments are applied atomically.

---

## 14. Approval Workflow

Approval is configurable for supported operations.

| Operation        | Approval     |
| ---------------- | ------------ |
| Purchase         | Direct       |
| Sale             | Direct       |
| Purchase Return  | Direct       |
| Sales Return     | Direct       |
| Stock Transfer   | Configurable |
| Stock Adjustment | Configurable |

* Supported operations can be configured for Direct or Approval Required processing.
* Approval-required transactions remain pending until approved.
* Approval does not bypass normal inventory validation.
* Stock Transfers reserve stock while awaiting approval.
* Stock Adjustments do not reserve stock while awaiting approval.
* Rejected or cancelled pending transactions do not change inventory.

---

## 15. Auditability & Consistency

* Every actual stock change must be traceable to its originating business transaction.
* Stock Ledger entries preserve historical transaction facts.
* Corrections use new inventory movements rather than modifying historical ledger entries.
* Stock Ledger and Stock Balance changes must occur atomically.
* Inventory state must be reconcilable against the Stock Ledger.
* FIFO allocations provide traceability between outbound movements and inbound inventory layers.

---

## 16. Explicitly Out of Scope

The current version does not support:

* Multiple companies or multi-tenancy
* Bin/shelf-level inventory
* Negative inventory
* Serial number tracking
* Batch/lot tracking
* Expiry-date tracking
* In-transit inventory
* Dynamic product attributes
* Vendor/customer accounting balances
* Payments and full accounting
* Advanced pricing, discounts, promotions, and price lists
* Microservices or distributed architecture

---

## 17. Core Domain

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
 ├── Purchase       → Stock Ledger (IN)
 ├── Sale           → Stock Ledger (OUT)
 ├── Purchase Return → Stock Ledger (OUT)
 ├── Sales Return   → Stock Ledger (IN)
 ├── Stock Transfer → Stock Ledger (OUT + IN)
 ├── Stock Adjustment → Stock Ledger (IN/OUT)
 │
 └── Inventory
      ├── Stock Ledger
      ├── Stock Ledger Allocation
      └── Stock Balance
```
