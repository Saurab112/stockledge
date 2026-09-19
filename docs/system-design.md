
### Features List

* Product Category, Product, and Product Variant
* Unit of Measure Group and Unit of Measure
* Vendor Group and Vendor
* Warehouse
* Purchase
* Stock Ledger
* Stock Balance
* Sale 
* Purchase Return
* Sale Return
* Stock Ledger Allocation
* Stock Transfer
* Stock Adjustment


## Feature: Product Category, Product and Product Variant

### What

Product Category is used to group similar products.

For example:

* Product Category: Cold Drinks
* Products: Coca-Cola, Mountain Dew, Fanta

A Product represents the common definition of an item, while a Product Variant represents a specific form of that product that can be stocked and sold.

For example, Coca-Cola can have variants based on size or other characteristics.

### Why

Allows the system to organize products and define the specific variants that can be stocked, purchased, transferred, and sold.

### How

Products are organized under categories, while product variants represent
the specific stockable items.

Each product variant has its own SKU and is assigned a UOM Group.
The UOM Group determines the units of measure that can be used for the
variant.

Inventory stock is identified by the combination of product variant,
warehouse, and unit of measure.

### Dependencies

* Unit of Measure Group
* Unit of Measure



## Feature: Unit of Measure Group and Unit of Measure

### What

A Unit of Measure Group defines a set of related units of measure that can be used to measure the same type of stock.

For example:

* UOM Group: Weight
* Base UOM: Gram
* Other UOMs: Kilogram, Metric Ton

A Unit of Measure represents an individual measurement unit within a UOM group.

### Why

Allows product variants to be stocked and transacted using different units of measure while providing a defined relationship between those units.

### How

A UOM group contains multiple units of measure. Each UOM has a multiplier that defines its relationship to the base UOM.

For example:

* Gram → Multiplier: `1`
* Kilogram → Multiplier: `1000`

This means `1 Kilogram = 1000 Grams`.

A product variant is assigned a UOM group, allowing its inventory transactions to use the UOMs defined within that group.

### Dependencies

* Product Variant

### Flow

1. Create a UOM group.
2. Define the base UOM for the group.
3. Add other UOMs to the group.
4. Define the multiplier for each UOM relative to the base UOM.
5. Assign the UOM group to a product variant.
6. Use a valid UOM from the assigned group when performing stock-related transactions.

### Design Considerations

* Each UOM group must have exactly one base UOM.
* The base UOM must have a multiplier of `1`.
* Other UOMs must define their conversion relative to the base UOM.
* UOMs from different groups should not be convertible.
* Stock is maintained per product variant, warehouse, and UOM.



## Feature: Vendor Group and Vendor

### What

Vendor Group is used to organize vendors into related groups.

A Vendor represents a supplier from whom products can be purchased.

For example:

* Vendor Group: Beverage Suppliers
* Vendors: ABC Beverages, XYZ Distributors

### Why

Allows purchases to be associated with the vendor from whom the stock was purchased and provides a simple way to organize vendors.

### How

Vendors are organized under vendor groups and can be selected when creating a purchase.

### Dependencies

* Purchase
* Product

### Flow

1. Create a vendor group.
2. Create a vendor under the appropriate group.
3. Select a vendor when creating a purchase.
4. The purchase records the selected vendor.

### Design Considerations

* Vendor balance and payment management are outside the current scope.
* No vendor payable or vendor statement is maintained.
* Vendor management is limited to maintaining vendor information and associating vendors with purchases.



## Feature: Warehouse

### What

A Warehouse (Store) represents a physical location where inventory stock is stored.

The inventory system supports multiple warehouses.

### Why

Allows the system to maintain and track stock separately across different warehouses.

### How

The system maintains stock separately for each product variant, warehouse, and unit of measure.

The combination of product variant, warehouse, and unit of measure identifies a specific stock balance.

### Dependencies

* Product Variant
* Unit of Measure

### Flow

1. Create a warehouse.
2. Use the warehouse when performing stock-related transactions.
3. Track stock separately for each product variant and unit of measure within the warehouse.

### Design Considerations

* A product variant can have stock in multiple warehouses.
* A product variant can have separate stock balances for different units of measure within the same warehouse.
* Stock is identified by the combination of product variant, warehouse, and unit of measure.


## Feature: Purchase

### What

A Purchase represents a transaction where the business purchases stock from a vendor for a specific warehouse.

A purchase contains one or more purchase items, with each item specifying the product variant, UOM, quantity, and purchase cost.

### Why

Allows the system to record what was purchased, from which vendor, and for which warehouse, while increasing the available inventory.

### How

A purchase is created with its purchase items and associated with a vendor and warehouse.

When the purchase is confirmed, each purchase item creates an inbound Stock Ledger entry and updates the corresponding Stock Balance.

The received stock also becomes a FIFO inventory layer using the purchase cost.

### Dependencies

* Vendor
* Product Variant
* Unit of Measure
* Warehouse
* Stock Ledger
* Stock Balance

### Flow

1. Create a purchase with vendor and warehouse.
2. Add one or more purchase items.
3. Select the product variant and UOM for each item.
4. Enter the quantity and purchase cost.
5. Validate the purchase.
6. Confirm the purchase.
7. Create inbound Stock Ledger entries.
8. Create or update the corresponding Stock Balance.
9. Add the received quantity as a FIFO layer.

### Design Considerations

* Purchase does not directly modify stock; inventory changes are recorded through the Stock Ledger.
* Purchase confirmation and inventory updates should happen atomically.
* The purchase UOM is the UOM used for the corresponding stock balance.
* The purchase cost is used for FIFO costing.
* A purchase does not require an approval workflow.
* Vendor payment, payable, balance, and statement management are outside the current scope.

## Feature: Stock Ledger

### What

Stock Ledger records the history of all inventory movements.

Each stock-changing operation creates a ledger entry representing the quantity, UOM, warehouse, movement type, cost, and reference to the originating transaction.

### Why

Provides a reliable and traceable history of inventory movements and acts as the authoritative source for inventory history.

### How

Stock Ledger entries are created when inventory changes through operations such as purchases, sales, adjustments, and transfers.

Each entry records the original transaction facts, including the transaction quantity and its equivalent base quantity.

For inbound FIFO layers, `RemainingBaseQuantity` tracks the quantity that is still available for consumption.

### Dependencies

* Product Variant
* Warehouse
* Unit of Measure
* Purchase
* Sale
* Stock Adjustment
* Stock Transfer

### Flow

1. An inventory transaction is confirmed.
2. The system validates the stock operation.
3. A Stock Ledger entry is created.
4. The transaction quantity and base quantity are recorded.
5. For inbound stock, the remaining base quantity is initialized for FIFO tracking.
6. The corresponding Stock Balance is created or updated.

### Design Considerations

* Ledger transaction facts are immutable.
* `Quantity` and `BaseQuantity` preserve the original transaction values.
* `RemainingBaseQuantity` is mutable inventory-layer state used for FIFO consumption.
* Corrections to historical movements are made through compensating movements rather than modifying existing entries.
* Each ledger entry references its originating business transaction.

## Feature: Stock Balance

### What

Stock Balance represents the current available inventory for a product variant at a warehouse using a specific unit of measure.

The stock identity is:

`Product Variant + Warehouse + Unit of Measure`

### Why

Provides fast access to the current stock quantity without calculating the entire inventory history from the Stock Ledger for every stock query.

### How

Stock Balance is created or updated when a stock-changing transaction creates a Stock Ledger entry.

The balance is maintained separately for each product variant, warehouse, and UOM.

### Dependencies

* Product Variant
* Warehouse
* Unit of Measure
* Stock Ledger

### Flow

1. An inventory transaction creates a Stock Ledger entry.
2. The system identifies the corresponding Product Variant, Warehouse, and UOM.
3. The existing Stock Balance is created or updated.
4. The balance reflects the current available quantity.
5. Stock Balance and Stock Ledger changes are committed atomically.

### Design Considerations

* Stock Balance is a derived current-state representation, not the historical source of truth.
* A unique balance exists for each Product Variant + Warehouse + UOM combination.
* Negative stock is not allowed.
* Stock Balance should be reconcilable against the Stock Ledger.


## Feature: Sale

### What

A Sale represents a transaction where the business sells stock from a specific warehouse.

A sale contains one or more sale items, with each item specifying the product variant, UOM, quantity, and selling price.

### Why

Allows the system to record stock sold from a warehouse and reduce the corresponding available inventory while maintaining a traceable inventory history.

### How

A sale is created with its sale items and associated with a warehouse.

When the sale is confirmed, each sale item is validated against the available Stock Balance for the specified product variant, warehouse, and UOM.

The system consumes the required quantity using FIFO costing, creates an outbound Stock Ledger entry, and updates the corresponding Stock Balance.

### Dependencies

* Product Variant
* Unit of Measure
* Warehouse
* Stock Ledger
* Stock Balance

### Flow

1. Create a sale with a warehouse.
2. Add one or more sale items.
3. Select the product variant and UOM for each item.
4. Enter the quantity and selling price.
5. Validate the sale and available stock.
6. Confirm the sale.
7. Determine the applicable FIFO inventory layers.
8. Create an outbound Stock Ledger entry.
9. Reduce the corresponding Stock Balance.
10. Update the remaining quantity of the consumed FIFO layers.

### Design Considerations

* Sale confirmation and inventory updates should happen atomically.
* Stock availability is checked for the exact Product Variant + Warehouse + UOM combination.
* Stock from another UOM is not automatically converted or consumed when the requested UOM has insufficient stock.
* FIFO consumes the oldest available inventory layers first.
* Negative stock is not allowed.
* The original Stock Ledger entries remain immutable; FIFO consumption updates only `RemainingBaseQuantity`.
* Selling price is independent of the inventory cost used for FIFO costing.


## Feature: Purchase Return

### What

A Purchase Return represents a transaction where previously purchased stock is returned to the vendor from a specific warehouse.

A purchase return contains one or more return items referencing the original purchase items and the inventory layers from which the stock is returned.

### Why

Allows the business to reverse received stock that needs to be returned to the vendor while maintaining traceability to the original purchase and inventory movement.

### How

A purchase return is created against a completed purchase and records the vendor, warehouse, and items being returned.

When the return is confirmed, the system validates that the requested quantity is available in the referenced inbound FIFO layer.

The system reduces the layer's `RemainingBaseQuantity`, creates an outbound Stock Ledger entry, and updates the corresponding Stock Balance.

### Dependencies

* Purchase
* Purchase Item
* Product Variant
* Unit of Measure
* Warehouse
* Stock Ledger
* Stock Balance

### Flow

1. Select the original purchase.
2. Create a purchase return for the warehouse where the stock is held.
3. Select the purchase items and inventory layers being returned.
4. Enter the return quantity.
5. Validate the available quantity in the referenced FIFO layer.
6. Confirm the purchase return.
7. Reduce the affected FIFO layer's `RemainingBaseQuantity`.
8. Create an outbound Stock Ledger entry.
9. Update the corresponding Stock Balance.

### Design Considerations

* A purchase return must reference the original purchase item.
* The return references the specific inbound Stock Ledger layer from which stock is returned.
* The original Stock Ledger transaction facts remain immutable.
* `RemainingBaseQuantity` is reduced as part of the return.
* Purchase return cost uses the cost of the referenced inventory layer.
* Return processing and inventory changes are atomic.


## Feature: Sales Return

### What

A Sales Return represents a transaction where previously sold stock is returned by a customer to a specific warehouse.

A sales return contains one or more return items referencing the original sale items.

### Why

Allows the business to receive previously sold stock back into inventory while maintaining traceability to the original sale and restoring the inventory consumed by that sale.

### How

A sales return is created against a completed sale and records the warehouse receiving the returned stock.

When the return is confirmed, the system identifies the original sale's Stock Ledger movement and its FIFO allocations.

The returned quantity is restored against the original FIFO allocations in reverse allocation order. The system then creates an inbound Stock Ledger entry and updates the corresponding Stock Balance.

### Dependencies

* Sale
* Sale Item
* Product Variant
* Unit of Measure
* Warehouse
* Stock Ledger
* Stock Ledger Allocation
* Stock Balance

### Flow

1. Select the original sale.
2. Create a sales return for the receiving warehouse.
3. Select the sale items being returned.
4. Enter the return quantity.
5. Identify the original sale's FIFO allocations.
6. Restore the returned quantity against the allocations in reverse order.
7. Create an inbound Stock Ledger entry.
8. Update the corresponding Stock Balance.
9. Confirm the sales return.

### Design Considerations

* A sales return must reference the original sale item.
* FIFO restoration is based on the original sale's `StockLedgerAllocation` records.
* Returned quantity is restored against allocations in reverse order.
* The original Sale Stock Ledger entry remains immutable.
* `RemainingBaseQuantity` is increased on the affected inbound layers.
* The system does not track individual physical units, serial numbers, or batches.
* Sales return processing and inventory changes are atomic.

## Feature: Stock Ledger Allocation

### What

Stock Ledger Allocation records which inbound inventory layers were consumed by an outbound Stock Ledger movement.

A single outbound movement may consume stock from multiple FIFO layers, so the allocation records establish the relationship between the outbound movement and the inbound layers it consumed.

### Why

Allows the system to maintain a traceable relationship between stock consumption and the original inventory layers used to fulfill it.

This relationship is required for FIFO costing and for restoring the correct inventory layers when a sale is returned.

### How

When an outbound inventory transaction is confirmed, the system identifies the eligible inbound FIFO layers and consumes them in FIFO order.

For each consumed layer, a `StockLedgerAllocation` record is created containing the outbound ledger entry, inbound ledger entry, consumed base quantity, and applicable unit cost.

For example:

```text
Sale Ledger #201
       │
       ├── Allocation → Inbound #101
       │                 10 Box @ 1,000
       │
       └── Allocation → Inbound #102
                         5 Box @ 1,200
```

The allocation records represent how the outbound movement was fulfilled without modifying the historical transaction facts of either ledger entry.

### Dependencies

* Stock Ledger
* Product Variant
* Unit of Measure
* FIFO Costing
* Sales Return

### Flow

1. An outbound inventory transaction is confirmed.
2. Identify available inbound FIFO layers for the required stock.
3. Select layers in FIFO order.
4. Determine the quantity consumed from each layer.
5. Create a `StockLedgerAllocation` for each consumed layer.
6. Reduce the corresponding inbound layer's `RemainingBaseQuantity`.
7. Complete the outbound Stock Ledger movement.
8. When a Sales Return occurs, use the allocations to identify the original FIFO layers.
9. Restore the returned quantity against those allocations in reverse order.

### Design Considerations

* One outbound Stock Ledger entry can have multiple allocation records.
* Each allocation references one inbound Stock Ledger layer.
* `BaseQuantity` represents the quantity allocated from the inbound layer in base-UOM terms.
* `UnitCost` preserves the cost applied from the corresponding inbound layer.
* Allocation records are historical relationships and should not be modified to change past FIFO consumption.
* Sales Return uses the original allocations to restore inventory layers rather than creating an unrelated FIFO relationship.
* FIFO allocation is performed only among compatible stock identified by the required Product Variant, Warehouse, and UOM rules.
* The allocation process and related Stock Ledger/Stock Balance changes are performed atomically.



## Feature: Stock Transfer

### What

A Stock Transfer represents the movement of inventory from one warehouse to another within the company.

A transfer contains one or more transfer items, with each item specifying the product variant, UOM, and quantity to be transferred.

Transfers can either be executed immediately or require approval before the inventory is moved.

### Why

Allows inventory to be moved between warehouses while maintaining accurate stock quantities, FIFO costing, reservation of pending stock, and traceability of the transfer.

### How

A transfer identifies a source warehouse and a destination warehouse.

When approval is not required, the transfer is executed immediately by creating `TransferOut` and `TransferIn` Stock Ledger movements.

When approval is required, the requested stock is first reserved in the source warehouse. The reserved quantity remains physically in the source warehouse but is unavailable for other stock-consuming transactions.

Once the transfer is approved, the reserved quantity is converted into the actual stock movement. The reservation is released as part of the same atomic transaction in which TransferOut and TransferIn are created.

### Dependencies

* Product Variant
* Unit of Measure
* Warehouse
* Stock Ledger
* Stock Ledger Allocation
* Stock Balance
* Approval

### Flow

#### Automatic Transfer

1. Create a transfer with source and destination warehouses.
2. Add one or more transfer items.
3. Validate the requested stock availability.
4. Create `TransferOut` Stock Ledger entries.
5. Create FIFO allocations for the source inventory.
6. Create `TransferIn` Stock Ledger entries at the destination.
7. Update both Stock Balances.
8. Mark the transfer as completed.

#### Approval Required

1. Create a transfer with source and destination warehouses.
2. Add one or more transfer items.
3. Validate the available stock.
4. Increase `ReservedQuantity` for the requested stock.
5. Keep the transfer pending for approval.
6. If rejected, release the reservation.
7. If approved, execute the transfer, create the ledger movements, and release the reservation atomically.
8. Update the source and destination Stock Balances.
9. Mark the transfer as completed.

### Design Considerations

* `StockBalance.Quantity` represents on-hand stock.
* `StockBalance.ReservedQuantity` represents stock committed to pending transfers.
* Available stock is calculated as `Quantity - ReservedQuantity`.
* Reserved stock cannot be consumed by another inventory transaction.
* Reservation is not a Stock Ledger movement because the stock has not physically moved.
* There is no separate in-transit inventory state.
* `TransferOut` reduces source stock and `TransferIn` increases destination stock.
* FIFO allocations are created for the source warehouse's outbound movement.
* The TransferIn movement preserves the FIFO cost composition consumed from the source warehouse.
* The destination receives the transferred quantity in the specified UOM.
* Source and destination inventory changes are performed atomically.
* Stock availability and reservation must be handled atomically to prevent over-reservation.
* A transfer cannot move stock between different UOM groups.


## Feature: Stock Adjustment

### What

A Stock Adjustment represents a correction to inventory when the physical stock differs from the quantity recorded by the system.

A stock adjustment contains one or more adjustment items, with each item specifying the product variant, UOM, quantity, direction, reason, and applicable unit cost.

Stock adjustments can either be applied immediately or require approval based on the system's Stock Adjustment Approval setting.

### Why

Allows inventory quantities to be corrected for situations that cannot be represented through normal business transactions such as purchases, sales, transfers, or returns.

Common adjustment reasons include:

* Physical Count Difference
* Damaged Stock
* Lost Stock
* Expired Stock
* Found Stock
* Opening Stock
* Other Inventory Correction

The adjustment process maintains UOM-specific inventory, FIFO costing, Stock Balance accuracy, and a complete inventory history.

### How

A stock adjustment identifies the warehouse where the inventory correction will occur.

Each adjustment item specifies the product variant, UOM, quantity, direction, reason, and applicable cost.

For a **positive adjustment**, the specified unit cost is used to create a new FIFO inventory layer.

For a **negative adjustment**, the system consumes existing inventory using FIFO and determines the cost from the consumed inventory layers.

The system checks the Stock Adjustment Approval setting to determine whether the adjustment is applied immediately or requires approval.

When approval is required, the adjustment remains `Pending` and does not affect inventory until it is approved.

### Dependencies

* Product Variant
* Unit of Measure
* Warehouse
* Stock Ledger
* Stock Ledger Allocation
* Stock Balance
* FIFO Costing
* Approval
* System Settings

### Flow

#### Direct Adjustment

When Stock Adjustment Approval is disabled:

1. Create a stock adjustment with one or more adjustment items.
2. Validate the warehouse, product variants, UOMs, quantities, and applicable costs.
3. Validate available stock for negative adjustments.
4. Create the required Stock Ledger entries.
5. Create FIFO allocations for negative adjustments.
6. Create new FIFO layers for positive adjustments.
7. Update the affected Stock Balances.
8. Mark the adjustment as `Confirmed`.

All inventory changes are performed within the same transaction.

#### Approval Required

When Stock Adjustment Approval is enabled:

1. Create a stock adjustment with one or more adjustment items.
2. Validate the warehouse, product variants, UOMs, quantities, and applicable costs.
3. Create the adjustment with `Pending` status.
4. Do not create Stock Ledger movements.
5. Do not modify Stock Balance.
6. Submit the adjustment for approval.
7. If rejected, mark the adjustment as `Rejected`.
8. If approved, revalidate the adjustment.
9. Apply the inventory changes.
10. Create the required Stock Ledger entries.
11. Create FIFO allocations for negative adjustments.
12. Create new FIFO layers for positive adjustments.
13. Update the affected Stock Balances.
14. Mark the adjustment as `Confirmed`.

Approval, inventory changes, FIFO processing, Stock Ledger creation, and Stock Balance updates are performed atomically when the adjustment is approved.

### Design Considerations

* Stock Adjustment is a business document separate from Stock Ledger.
* Stock Adjustment approval is controlled by a system setting.
* A pending adjustment does not create Stock Ledger movements, modify Stock Balance, or reserve stock.
* `Confirmed` means the adjustment has been applied to inventory.
* `Rejected` means the adjustment does not affect inventory.
* Positive adjustments require a Unit Cost and create new inbound FIFO layers.
* Negative adjustments consume existing inventory layers using FIFO.
* Negative adjustments create `StockLedgerAllocation` records linking the adjustment to the consumed inbound layers.
* Negative adjustments can consume only available stock.
* Available stock is calculated as `Quantity - ReservedQuantity`.
* Stock is maintained separately for each `ProductVariant + Warehouse + UOM` combination.
* The adjustment UOM must belong to the Product Variant's assigned UOM Group.
* The system does not automatically consume stock from another UOM when the requested UOM is insufficient.
* Opening stock can be recorded as a positive adjustment using the `OpeningStock` reason.
* Original Stock Ledger transaction facts remain immutable after confirmation.
* Corrections to a confirmed adjustment are represented by a new inventory transaction rather than modifying the original transaction.
* Direct and approved adjustments use the same inventory-processing logic; only the workflow before execution differs.
* Inventory validation and updates must be performed atomically to prevent concurrent operations from causing negative or inconsistent stock.
