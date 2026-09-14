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


## Feature: Product Category, Product and Product Variant

### What

Product Category is used to group similar products.

For example:

* Product Category: Cold Drinks
* Products: Coca-Cola, Mountain Dew, Fanta

A Product represents the common definition of an item, while a Product Variant represents the actual stockable and sellable item.

For example, Coca-Cola can have different variants based on size or other characteristics.

### Why

Allows the system to organize products and represent the actual items that are stocked, purchased, transferred, and sold.

### How

Products are organized under categories, while product variants represent the specific stockable items.

Each product variant has its own SKU and unit of measure for inventory tracking.

### Dependencies

* Warehouse
* Unit of Measure

### Flow

1. Create a product category.
2. Create a product under the category.
3. Create one or more product variants.
4. Assign the required unit of measure to each variant. (it needs more discussion on how to handle unit of measure for product variants)
5. Use the product variant for stock transactions.

### Design Considerations

* Each product variant must have a unique SKU.
* Stock is maintained against the product variant rather than the product.


## Feature: Unit of Measure Group and Unit of Measure

### What

A Unit of Measure Group defines a set of related units of measure that can be used to measure the same type of stock.

For example:

* UOM Group: Weight
* Base UOM: Gram
* Other UOMs: Kilogram, Metric Ton

A Unit of Measure represents an individual measurement unit within a UOM group.

### Why

Allows stockable product variants to be measured and managed using different units while maintaining a consistent base unit for stock calculations.

### How

A UOM group contains multiple units of measure with a multiplier that defines their conversion to the base UOM.

For example:

* Gram → Multiplier: `1`
* Kilogram → Multiplier: `1000`

This means `1 Kilogram = 1000 Grams`.

The system uses the base UOM as the common unit for stock calculations and conversions.

### Dependencies

* Product Variant

### Flow

1. Create a UOM group.
2. Define the base UOM for the group.
3. Add other UOMs to the group.
4. Define the multiplier for each UOM relative to the base UOM.
5. Assign a UOM from the appropriate group to a product variant. (it needs more discussion on how to handle unit of measure for product variants)
6. Use the UOM and multiplier when performing stock-related transactions.

### Design Considerations

* Each UOM group must have exactly one base UOM.
* The base UOM must have a multiplier of `1`.
* Other UOMs must define their conversion relative to the base UOM.
* UOMs from different groups should not be convertible.


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

Stock is maintained per product variant and warehouse.

The combination of a product variant and warehouse identifies the stock held at a particular warehouse.

### Dependencies

* Product Variant
* Unit of Measure

### Flow

1. Create a warehouse.
2. Assign stock to a warehouse through inventory transactions.
3. Track stock separately for each product variant in each warehouse.
4. Use the warehouse when performing stock-related transactions.

### Design Considerations

* A product variant can have stock in multiple warehouses.
* Stock quantity is maintained separately for each warehouse.
