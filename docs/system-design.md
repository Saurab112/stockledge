Template

## Feature / Area

### What
What is this responsible for?

### How
How will it work architecturally?

### Flow
What are the important steps?

### Dependencies
What modules/components/systems does it interact with?

### Design Considerations
Transactions, concurrency, security, performance,
or other important considerations.

### Features List
## Feature: Product Category, Product and Product Variant
## Feature: Unit of Measure Group and Unit of Measure
## Feature: Vendor Group and Vendor
## Feature: Warehouse
## Feature: Purchase
## Feature: Stock Ledger
## Feature: Stock Balance
## Feature: Sale

### Remaining features to be added:
## Feature: Purchase Return
## Feature: Sale Return
## Feature: Stock Adjustment
## Feature: Stock Transfer
## Feature: Customer (we can include for the Sale feature)

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
