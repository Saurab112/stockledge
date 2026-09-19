# StockLedger — Business Rules

This document defines the business rules that the system must enforce, independent of how those rules are implemented in code.

Rules that have not yet been finalized are explicitly marked as **Open**.

---

## 1. Company and Warehouse

1. The system supports a single company.
2. The company can have multiple warehouses.
3. Stock is tracked separately for each warehouse.
4. Stock in one warehouse does not affect the stock of another warehouse.
5. A warehouse is the smallest physical location tracked by the system.
6. Bin-level or shelf-level stock tracking is not supported.

---

## 2. Products and Product Variants

1. Products are organized under product categories.
2. A Product represents the common definition of an item.
3. A Product may have one or more Product Variants.
4. A Product Variant represents a specific stockable and sellable form of a Product.
5. Each Product Variant must have a unique SKU.
6. A Product Variant is the inventory identity used for stock-related transactions.
7. A Product Variant must be assigned a UOM Group.
8. A Product Variant does not have a single fixed UOM.
9. Transactions must use a UOM belonging to the Product Variant's assigned UOM Group.
10. Product variants may represent differences such as size or other characteristics through their name and SKU.
11. Structured product attributes such as dynamic color, size, or similar attribute models are not supported in the current version.

---

## 3. Units of Measure

1. A UOM Group defines a set of related units used to measure the same type of stock.
2. Each UOM Group must have exactly one base UOM.
3. The base UOM must have a multiplier of `1`.
4. Each other UOM must define its multiplier relative to the base UOM.
5. Example:

   * Gram → `1`
   * Kilogram → `1000`
6. A Product Variant must be assigned a UOM Group before it can be used in stock-related transactions.
7. A transaction must use a UOM belonging to the Product Variant's assigned UOM Group.
8. UOMs belonging to different UOM Groups cannot be converted.
9. UOM conversion may be used to normalize quantities for FIFO calculations and inventory-layer tracking.
10. Stock Balance is maintained separately for each UOM used in the inventory.
11. The system does not automatically consume or combine stock from another UOM when the requested UOM is insufficient.
12. Historical transaction quantities must not be changed because of later changes to UOM configuration.
13. **Open:** Whether UOM configuration can be modified after transactions exist is not yet finalized.

---

## 4. Inventory Identity and Tracking

1. The inventory identity is defined by:

   * Product Variant
   * Warehouse
   * Unit of Measure
2. Stock for different Product Variants is tracked separately.
3. Stock for the same Product Variant in different warehouses is tracked separately.
4. Stock for the same Product Variant and warehouse is tracked separately for different UOMs.
5. Every actual stock-changing operation must create a Stock Ledger entry.
6. The Stock Ledger is append-only.
7. Historical Stock Ledger transaction facts must not be modified or deleted.
8. Corrections to historical inventory movements must be represented through new compensating inventory movements.
9. Each Stock Ledger entry must identify the Product Variant, Warehouse, UOM, quantity, movement type, and originating transaction.
10. `Quantity` records the original transaction quantity and is immutable.
11. `BaseQuantity` records the original quantity normalized to the UOM Group's base UOM and is immutable.
12. `RemainingBaseQuantity` represents the remaining quantity of an inbound FIFO layer and may change as inventory is consumed or returned.
13. Stock Ledger entries preserve immutable transaction facts, while `RemainingBaseQuantity` represents mutable inventory-layer state.
14. Stock Balance represents the current inventory state for a Product Variant, Warehouse, and UOM.
15. Stock Balance is a derived representation and can be reconciled against the Stock Ledger.
16. Stock Ledger and Stock Balance changes caused by the same inventory operation must be performed atomically.
17. If Stock Balance and Stock Ledger disagree, the Stock Ledger is treated as the authoritative historical record and the Stock Balance must be reconciled.

---

## 5. Stock Availability and Reservations

1. Stock Balance maintains both on-hand quantity and reserved quantity.

2. `Quantity` represents the on-hand stock for a Product Variant, Warehouse, and UOM.

3. `ReservedQuantity` represents stock committed to a pending operation but not yet physically moved.

4. Available stock is calculated as:

   `AvailableQuantity = Quantity - ReservedQuantity`

5. Reserved stock is not available for other stock-consuming operations.

6. A stock-consuming operation must not consume more than the available quantity.

7. Reservations do not represent physical inventory movements.

8. Reservations do not create Stock Ledger entries.

9. A pending operation must not reduce on-hand `Quantity` merely because stock has been reserved.

10. Reservations must not exceed the available on-hand stock.

11. Reservation creation and stock availability validation must be handled atomically to prevent over-reservation during concurrent operations.

12. Approval-based Stock Transfers reserve the requested stock while awaiting approval.

13. A rejected or cancelled pending transfer releases its reservation.

14. When an approved transfer is executed, its reservation is released as the actual inventory movement is created.

15. A pending Stock Adjustment does not reserve stock.

16. Sales, negative Stock Adjustments, and other stock-consuming operations can consume only available stock.

17. The system must not automatically use stock from another UOM to satisfy an operation when the requested UOM does not have sufficient available stock.

## 6. Negative Stock

1. The system must not allow stock to become negative.
2. A stock-consuming operation must have sufficient available stock before it can be confirmed.
3. Available stock is calculated using:

   `AvailableQuantity = Quantity - ReservedQuantity`

4. Reserved stock cannot be consumed by another stock-consuming operation.
5. Negative stock validation must be performed for each Product Variant, Warehouse, and UOM combination affected by the transaction.
6. A transaction that would result in negative stock must be rejected.
7. Negative stock validation and the corresponding inventory update must be performed atomically.
8. Concurrent stock-consuming operations must not be allowed to bypass the negative stock validation.
9. Failed stock-consuming transactions must not create partial Stock Ledger or Stock Balance changes.
10. The system does not support negative inventory as a valid stock state.

---

## 7. Purchasing

1. A Purchase must be associated with a Vendor and a Warehouse.
2. A Purchase must contain one or more Purchase Items.
3. Each Purchase Item must specify:
   - Product Variant
   - UOM
   - Quantity
   - Unit Cost
4. The selected UOM must belong to the Product Variant's assigned UOM Group.
5. Purchase quantity must be greater than zero.
6. Purchase unit cost must be greater than or equal to zero.
7. Confirming a Purchase increases stock in the specified Warehouse.
8. Each confirmed Purchase Item creates an inbound Stock Ledger entry.
9. Received stock must be added to the corresponding Stock Balance.
10. Received stock becomes a new FIFO inventory layer.
11. The Purchase Item's Unit Cost is used as the cost of its FIFO inventory layer.
12. A Purchase does not require approval.
13. A Purchase must not directly modify historical Stock Ledger entries.
14. Purchase confirmation and its inventory changes must be performed atomically.
15. Vendor payment, payable, balance, and statement management are outside the scope of the current system.
16. A cancelled or unconfirmed Purchase must not affect inventory.

---

## 8. Sales

1. A Sale must be associated with a Warehouse.
2. A Sale may optionally be associated with a Customer.
3. A Sale must contain one or more Sale Items.
4. Each Sale Item must specify:
   - Product Variant
   - UOM
   - Quantity
   - Unit Price
5. The selected UOM must belong to the Product Variant's assigned UOM Group.
6. Sale quantity must be greater than zero.
7. Unit Price must be greater than or equal to zero.
8. A Sale can be confirmed only when sufficient available stock exists for each Product Variant, Warehouse, and UOM combination.
9. Reserved stock must not be consumed by a Sale.
10. A confirmed Sale decreases the corresponding Stock Balance.
11. A confirmed Sale creates an outbound Stock Ledger entry.
12. Stock consumed by a Sale must be allocated against FIFO inventory layers.
13. FIFO allocations must be recorded through Stock Ledger Allocation.
14. A Sale does not automatically consume stock from another UOM when the requested UOM is insufficient.
15. The Sale Unit Price represents the selling price and is independent of the FIFO inventory cost.
16. FIFO inventory cost is used to determine the cost of stock consumed by the Sale.
17. Sale confirmation and its inventory changes must be performed atomically.
18. A cancelled or unconfirmed Sale must not affect inventory.
19. Negative stock is not permitted as a result of a Sale.

---

## 9. FIFO Costing

1. The system uses FIFO (First-In, First-Out) for inventory costing.
2. Inbound inventory that contributes stock must create an inventory layer.
3. Each inventory layer retains its applicable unit cost.
4. Purchase receipts create FIFO layers using the Purchase Item's Unit Cost.
5. Positive Stock Adjustments create FIFO layers using the specified adjustment Unit Cost.
6. Transfer In inventory preserves the cost composition of the stock transferred from the source Warehouse.
7. Sales consume eligible FIFO layers in chronological order, oldest first.
8. A single outbound transaction may consume stock from multiple FIFO layers.
9. Each FIFO consumption must be recorded through Stock Ledger Allocation.
10. `RemainingBaseQuantity` represents the remaining quantity of an inbound FIFO layer.
11. `RemainingBaseQuantity` may decrease when a layer is consumed.
12. `RemainingBaseQuantity` may increase when inventory is restored to an original layer, such as through a Sales Return.
13. Original Stock Ledger transaction facts must remain immutable even when `RemainingBaseQuantity` changes.
14. FIFO allocation and the related Stock Ledger and Stock Balance changes must be performed atomically.
15. FIFO consumption must respect the Product Variant, Warehouse, and UOM inventory identity.
16. Stock from a different Product Variant, Warehouse, or UOM must not be included in the FIFO calculation.
17. The system must maintain sufficient information to trace an outbound movement back to the inbound layers from which it was consumed.

---

## 10. Stock Transfers

1. A Stock Transfer moves stock from one Warehouse to another Warehouse.
2. A Stock Transfer must have a source Warehouse and a destination Warehouse.
3. Source and destination Warehouses must be different.
4. A Stock Transfer must contain one or more Transfer Items.
5. Each Transfer Item must specify:
   - Product Variant
   - UOM
   - Quantity
6. The selected UOM must belong to the Product Variant's assigned UOM Group.
7. Transfer quantity must be greater than zero.
8. A Transfer must have sufficient available stock in the source Warehouse before inventory can be moved.
9. Reserved stock must not be consumed by a Transfer.
10. A Transfer does not create an in-transit inventory state.
11. An Automatic Transfer creates both the source outbound movement and destination inbound movement as part of the same operation.
12. An Approval Required Transfer reserves the requested stock while waiting for approval.
13. A pending transfer does not create Stock Ledger movements.
14. Reserved stock remains physically in the source Warehouse until the transfer is approved and executed.
15. Approval of a pending Transfer releases its reservation and executes the inventory movement.
16. Rejection or cancellation of a pending Transfer releases its reservation without changing on-hand stock.
17. Transfer Out creates an outbound Stock Ledger entry in the source Warehouse.
18. Transfer In creates an inbound Stock Ledger entry in the destination Warehouse.
19. Transfer Out consumes source inventory using FIFO.
20. FIFO consumption for Transfer Out must be recorded through Stock Ledger Allocation.
21. Transfer In must preserve the applicable inventory cost composition of the stock transferred from the source Warehouse.
22. The Product Variant, UOM, and quantity transferred into the destination Warehouse must correspond to the stock transferred from the source Warehouse.
23. Source and destination inventory changes must be performed atomically.
24. Transfer reservation, availability validation, and execution must be handled safely under concurrent operations.
25. A failed transfer must not leave partial stock movements or unreleased reservations.

## 11. Stock Adjustments

1. A Stock Adjustment is used to correct inventory when the physical stock differs from the system stock.
2. A Stock Adjustment must contain one or more Adjustment Items.
3. Each Adjustment Item must specify:
   - Product Variant
   - UOM
   - Quantity
   - Direction
   - Reason
4. The selected UOM must belong to the Product Variant's assigned UOM Group.
5. Adjustment quantity must be greater than zero.
6. An Adjustment must specify whether stock is being increased or decreased.
7. Supported adjustment reasons include:
   - Physical Count Difference
   - Damaged
   - Lost
   - Expired
   - Found
   - Opening Stock
   - Other
8. A positive Stock Adjustment increases inventory and creates an inbound Stock Ledger entry.
9. A positive Stock Adjustment must specify a Unit Cost.
10. Stock received through a positive adjustment becomes a new FIFO inventory layer.
11. A negative Stock Adjustment decreases inventory and creates an outbound Stock Ledger entry.
12. A negative Stock Adjustment must have sufficient available stock before it can be confirmed.
13. Negative Stock Adjustments consume existing FIFO layers.
14. FIFO consumption from a negative Stock Adjustment must be recorded through Stock Ledger Allocation.
15. Reserved stock must not be consumed by a negative Stock Adjustment.
16. A pending Stock Adjustment does not change Stock Balance.
17. A pending Stock Adjustment does not create Stock Ledger entries.
18. An approved Stock Adjustment must revalidate the affected inventory before applying the adjustment.
19. Confirmed Stock Adjustments and their inventory changes must be performed atomically.
20. Corrections to a confirmed Stock Adjustment must be made through a new adjustment rather than modifying the original transaction.
21. Direct and approval-required adjustments must use the same inventory rules; only the approval workflow differs.
22. Opening Stock is represented as a positive Stock Adjustment using the appropriate opening stock cost.

---

## 12. Returns

1. The system supports two types of inventory returns:
   - Purchase Return
   - Sales Return
2. A Purchase Return moves stock from the business back to the Vendor.
3. A Sales Return moves stock from the Customer back into the business.
4. Returns use the common Stock Ledger and do not have separate inventory ledgers.

### Purchase Returns

5. A Purchase Return must reference the original Purchase and the relevant Purchase Item.
6. A Purchase Return Item must identify the original inbound Stock Ledger layer from which the stock is being returned.
7. The quantity returned must not exceed the eligible quantity available from the referenced inventory layer.
8. A confirmed Purchase Return creates an outbound Stock Ledger entry.
9. The returned quantity must reduce the corresponding inbound FIFO layer's `RemainingBaseQuantity`.
10. The corresponding Stock Balance must be reduced by the returned quantity.
11. The original Purchase and Stock Ledger transaction facts must remain unchanged.
12. Purchase Return inventory changes must be performed atomically.

### Sales Returns

13. A Sales Return must reference the original Sale and the relevant Sale Item.
14. A Sales Return must use the original Sale's UOM and Product Variant.
15. The returned quantity must not exceed the eligible quantity previously sold through the referenced Sale.
16. A confirmed Sales Return creates an inbound Stock Ledger entry.
17. A Sales Return restores inventory to the FIFO layers originally consumed by the Sale.
18. Original Stock Ledger Allocation records are used to identify the layers consumed by the original Sale.
19. When restoring multiple allocations, the return must restore the affected layers in reverse allocation order.
20. Restored quantities increase the corresponding inbound layers' `RemainingBaseQuantity`.
21. The returned stock increases the corresponding Stock Balance.
22. The original Sale, outbound Stock Ledger entries, and Stock Ledger Allocations must remain unchanged.
23. Sales Return inventory changes must be performed atomically.
24. Because the system does not track serial numbers or individually identifiable physical units, FIFO restoration represents the inventory-costing assumption for returned stock rather than proof of the exact physical units returned.

### Common Return Rules

25. Return quantities must be greater than zero.
26. The selected UOM must belong to the Product Variant's assigned UOM Group.
27. Returns must not create negative stock.
28. A return must create the appropriate Stock Ledger movement only when it is confirmed.
29. Corrections to a confirmed return must be represented through a new inventory transaction rather than modifying historical ledger facts.

---

## 13. Approvals

1. Approval requirements are configurable through system settings.
2. Supported workflow modes are:
   - Direct
   - Approval Required
3. The approval setting may be configured independently for supported transaction types.
4. A transaction configured as Direct can be applied without an approval step.
5. A transaction configured as Approval Required must remain pending until it is approved.
6. Supported approval-based inventory operations include Stock Transfers and Stock Adjustments.
7. A pending transaction must not create Stock Ledger entries unless explicitly defined by the transaction's workflow rules.
8. A pending Stock Transfer reserves the requested stock.
9. A pending Stock Adjustment does not reserve stock.
10. Approval does not bypass normal inventory validation.
11. An approval-based transaction must be revalidated when it is approved.
12. If sufficient stock is no longer available at approval time, a stock-consuming transaction must not be executed.
13. Approval and the resulting inventory changes must be performed atomically.
14. Approving a Stock Transfer releases its reservation as the actual inventory movement is created.
15. Rejecting or cancelling a pending Stock Transfer releases its reservation.
16. Rejecting a pending Stock Adjustment does not affect inventory.
17. A confirmed transaction cannot be approved again.
18. A rejected or cancelled transaction cannot be executed without creating a new transaction.
19. Direct and approval-required workflows must produce the same final inventory result when the same transaction is successfully applied.
20. Approval workflow controls when a transaction is applied; it does not change the underlying inventory rules.

---

## 14. Explicitly Not Supported

The following capabilities are outside the scope of the current version:

1. Multiple companies or multi-tenancy.
2. Bin, shelf, rack, or other sub-warehouse inventory locations.
3. Negative inventory.
4. Automatic cross-UOM stock consumption.
5. Automatic conversion of stock between different UOM Groups.
6. Dynamic product attributes such as configurable color, size, or material attribute models.
7. Serial number tracking.
8. Batch or lot number tracking.
9. Expiry-date-based inventory tracking.
10. In-transit inventory for Stock Transfers.
11. Partial or staged transfer workflows.
12. Vendor balance, payable, payment, or vendor statement management.
13. Customer balance, receivable, payment, or customer statement management.
14. Full accounting or general ledger functionality.
15. Automatic selling-price calculation from FIFO cost.
16. Automatic pricing rules, discounts, promotions, or price lists.
17. Barcode scanning workflows.
18. Purchase approval workflows.
19. Sale approval workflows.
20. Modification of historical Stock Ledger transaction facts.
21. Separate inventory ledgers for different transaction types.
22. Microservice-based architecture.
23. Multiple companies sharing the same inventory system.

---

