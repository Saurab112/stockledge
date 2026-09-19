# ADR-006: Product and ProductVariant Modeling

## Status

Accepted

## Context

Products may have multiple variations that need to be tracked independently in inventory.

For example:

```text
Product: T-Shirt

Red / Small
Red / Medium
Blue / Small
```

Each variation represents a distinct stockable and sellable item.

The inventory system needs a consistent stock identity for Stock Ledger, Stock Balance, purchasing, sales, and other inventory operations.

Several approaches were considered.

### Option 1 — Fixed Attribute Columns

Store common attributes such as `Color` and `Size` as columns on `ProductVariant`.

This is simple and queryable but becomes rigid when different product categories require different attributes.

### Option 2 — Dynamic Attribute Model

Create a generic attribute/value model for product variants.

This provides flexibility but introduces additional entities, relationships, validation, and query complexity.

### Option 3 — SKU-Based ProductVariant

Use ProductVariant as the stockable entity and represent descriptive variation information through the variant's name and SKU.

This provides a simple and consistent model without introducing a generalized product-attribute system.

## Decision

The system will model `Product` and `ProductVariant` as separate entities.

`ProductVariant` will contain:

* `Id`
* `ProductId`
* `Name`
* `SKU`
* `Barcode` (nullable)
* `IsActive`
* `UOMGroupId`

Structured attributes such as `Color`, `Size`, or similar product-specific properties will not be modeled separately.

They may be represented through the variant name when needed.

Each Product must have at least one ProductVariant, even when the product has no meaningful variations.

The ProductVariant is the stockable and sellable inventory identity.

All inventory records therefore reference:

```text
ProductVariantId
```

Stock identity is:

```text
ProductVariant + Warehouse + UOM
```

Each ProductVariant must be assigned a UOM Group, and transactions may use only UOMs belonging to that group.

Example:

```text
Product: T-Shirt

ProductVariant:
- Name: Red / Small
  SKU: TS-RED-S
  UOM Group: Piece

- Name: Red / Medium
  SKU: TS-RED-M
  UOM Group: Piece
```

## Consequences

### Positive

* Every stockable item has a consistent identity.
* Inventory always references ProductVariant.
* UOM rules are associated with the stockable variant.
* Products without meaningful variations do not require special inventory handling.
* The database model remains relatively simple.
* New descriptive variations do not require schema changes.

### Tradeoffs

* Attributes such as color and size are not structured data.
* Attribute-specific searching and reporting are limited.
* Product-specific attribute validation is not supported.
* A future product catalog may require a more structured attribute model.

The reduced flexibility is acceptable because the current system focuses on inventory management rather than generalized product catalog management.
