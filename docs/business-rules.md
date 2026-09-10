# StockLedger — Business Rules

This document defines the business rules that the system must enforce, independent of how those rules are implemented in code.

Rules that have not yet been finalized are explicitly marked as **Open** or **Proposed**.

---

## 1. Company and Warehouse

1. The system supports a single company.
2. A company can have multiple warehouses.
3. Stock quantities are tracked separately for each warehouse.
4. Stock in one warehouse does not affect the available stock of another warehouse.
5. Warehouse is the smallest location unit tracked by the system.
6. Bin-level or shelf-level stock tracking is not supported.

---

## 2. Products

1. Every product must have:

   * a name,
   * a code or SKU,
   * a category,
   * a base unit of measure,
   * an active or inactive status.
2. A product may have one or more variants.
3. Each variant represents a separately stockable item.
4. Stock is tracked at the product-variant level when variants are used.
5. **Open:** Whether product variants use a fixed set of attributes or a dynamic attribute model is not yet decided.
6. **Open:** Whether product categories are flat or hierarchical is not yet decided.

---

## 3. Units of Measure

1. Every product has a base unit of measure.
2. Stock quantities are ultimately maintained in the product's base unit.
3. Transactions may be entered using a different unit of measure.
4. A conversion factor is used when a transaction unit differs from the base unit.
5. Example: 1 box may represent 12 pieces.
6. The conversion factor used for a transaction is recorded with that transaction.
7. Changing a product's unit configuration later must not change the quantity or value already recorded by historical transactions.
8. **Open:** Whether the base unit of measure can be changed after transactions exist is not yet decided.
9. **Open:** Whether existing conversion factors can be modified after transactions exist is not yet decided.

---

## 4. Inventory Tracking

1. Every stock-changing operation must create an inventory ledger entry.
2. The inventory ledger is append-only.
3. Existing ledger entries must not be modified or deleted.
4. Historical corrections must be represented through new stock movements rather than modifying historical ledger entries.
5. A separate stock balance is maintained for each warehouse and stockable product variant.
6. The stock balance represents the current quantity derived from inventory movements.
7. The stock balance can be recalculated from the inventory ledger.
8. A stock-changing operation must update the inventory ledger and stock balance as a single atomic unit of work.
9. A stock-changing operation must not leave the ledger and stock balance partially updated.
10. If the stock balance and ledger disagree, the ledger is treated as the authoritative record and the balance must be reconciled.

---

## 5. Negative Stock

1. Available stock for a product variant in a warehouse must never become negative.
2. An operation that would result in negative stock must be rejected in full.
3. The system must not partially apply a stock-changing operation simply because only part of the requested quantity is available.
4. The negative-stock rule must remain valid when multiple operations affecting the same stock occur concurrently.
5. **Open:** The exact concurrency-control mechanism used to enforce this rule under concurrent access is not yet decided.

---

## 6. Purchasing

1. Receiving stock from a supplier increases the available stock in the receiving warehouse.
2. A purchase does not require approval before received stock becomes available.
3. Receiving stock creates an inventory movement.
4. Costed incoming inventory must retain its quantity and unit cost for FIFO costing.
5. **Open:** The exact cost-layer behavior for purchase returns, free items, discounts, taxes, and other purchasing adjustments is not yet decided.

---

## 7. Sales

1. Selling stock decreases the available stock in the warehouse from which the sale is fulfilled.
2. A sale does not require approval before it takes effect.
3. A sale cannot be completed if the requested quantity would cause available stock to become negative.
4. The cost of sold inventory is determined using the FIFO layers consumed by the sale.
5. A single sale may consume stock from multiple FIFO cost layers.

---

## 8. Inventory Costing

1. Inventory is costed using the FIFO (First In, First Out) method.
2. Costed incoming inventory creates a FIFO cost layer.
3. Each cost layer tracks the quantity and unit cost of the inventory it represents.
4. When inventory is removed, the oldest available cost layer is consumed first.
5. If a stock removal exceeds the remaining quantity of one cost layer, subsequent layers are consumed until the requested quantity is fulfilled.
6. A partially consumed cost layer retains its remaining quantity for future stock removals.
7. A fully consumed cost layer cannot be used for future stock removals.
8. Cost of goods sold is calculated from the cost layers consumed by the stock removal.
9. Remaining inventory value is calculated from the remaining quantities and costs of available FIFO layers.
10. **Open:** The exact FIFO cost behavior for transfers, positive adjustments, purchase returns, sales returns, and opening stock is not yet finalized.

---

## 9. Stock Transfers

1. A stock transfer moves inventory from one warehouse to another.
2. A stock transfer requires approval before it can be dispatched.
3. Stock must not be added to the destination warehouse when the transfer is merely created or approved.
4. When a transfer is dispatched, the transferred quantity is removed from the source warehouse and placed in an in-transit state.
5. Inventory in transit is not available as stock at either the source or destination warehouse.
6. When the destination confirms receipt, the received quantity is added to the destination warehouse.
7. The system must retain the relationship between the source movement, in-transit movement, and destination movement.
8. **Open:** The exact transfer status workflow is not yet finalized.
9. **Open:** Whether partial receiving is supported and how differences between dispatched and received quantities are handled is not yet decided.
10. **Open:** The FIFO cost-layer behavior when inventory is transferred between warehouses is not yet decided.
11. **Open:** The rules for cancelling or rejecting a transfer at different stages are not yet finalized.

---

## 10. Stock Adjustments

1. A stock adjustment is used when the physically counted inventory differs from the recorded stock quantity.
2. An adjustment may increase or decrease the recorded stock quantity.
3. An adjustment must create an inventory movement rather than directly modifying the historical stock ledger.
4. An adjustment that decreases stock must not cause available stock to become negative.
5. **Proposed:** Stock adjustments require approval before they are applied.
6. **Open:** The exact adjustment workflow and status transitions are not yet finalized.
7. **Open:** The costing treatment for positive and negative adjustments is not yet decided.

---

## 11. Returns

1. Purchase returns are in scope.
2. Sales returns are in scope.
3. A return must be represented as an inventory movement.
4. A return must not modify the original inventory ledger entry.
5. **Open:** The effect of purchase returns on stock and FIFO cost layers is not yet decided.
6. **Open:** The effect of sales returns on stock and FIFO cost layers is not yet decided.
7. **Open:** Whether returns require approval is not yet decided.

---

## 12. Approvals

1. Approval is required only for operations explicitly defined as requiring approval.
2. Purchases do not require approval.
3. Sales do not require approval.
4. Stock transfers require approval before dispatch.
5. **Proposed:** Stock adjustments require approval before being applied.
6. **Open:** The roles or users authorized to approve operations are not yet decided.
7. **Open:** The exact approval workflow, including rejection and resubmission behavior, is not yet finalized.

---

## 13. Explicitly Not Supported in This Version

The following capabilities are outside the scope of the current version:

1. Multiple companies or multi-tenancy.
2. Serial-number-level inventory tracking.
3. Stock reservations.
4. Bin-level or shelf-level inventory tracking.
5. Automatic approval requirements for every stock-changing operation.

These capabilities may be considered in a future version but are not requirements of the current system.
