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

For example, a Coca-Cola product may have variants based on size or other characteristics.

### Why

Allows the system to organize products and represent the actual items that are stocked, purchased, transferred, and sold.

### How

Products are organized under categories, while product variants represent the stockable items.

Each product variant has its own SKU and unit of measure for inventory tracking.

### Dependencies

* Product Category
* Product
* Product Variant
* Warehouse
* Unit of Measure

### Flow

1. Create a product category.
2. Create a product under the category.
3. Create one or more product variants for the product.
4. Assign the required unit of measure to the variant. (it needs more discussion on how to handle unit of measure for product variants)
5. Use the product variant when recording stock transactions.

### Design Decisions

Product variants have a unique SKU (Stock Keeping Unit) that identifies the specific stockable item.

