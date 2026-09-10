# ADR-006: Product and ProductVariant Modeling

## Status

Accepted

## Context

Products may have multiple variations that need to be tracked independently in inventory.

For example, a T-Shirt may have:

* Red / Small
* Red / Medium
* Blue / Small

Each variation represents a distinct stockable unit.

The inventory system needs a consistent stock identity so that `StockLedgerEntry` and `StockBalance` can always reference the same type of entity, regardless of whether a product has meaningful variations.

Several approaches were considered.

### Option 1 — Fixed Attribute Columns

Store common variant attributes such as `Color` and `Size` as columns on `ProductVariant`.

This provides a simple and queryable structure but becomes rigid when different product categories require different attributes.

### Option 2 — Dynamic Attribute Model

Create a generic `Attribute` / `AttributeValue` structure that allows different attributes to be associated with different variants.

This provides greater flexibility across product categories but introduces additional entities, relationships, validation, and query complexity that are not required by the current system.

### Option 3 — SKU-Based ProductVariant

Use `ProductVariant` as the stockable entity and represent descriptive variation information through the variant's name rather than maintaining structured attribute fields.

This provides a simple and consistent model while still allowing products to have meaningful variations.

## Decision

The system will model `Product` and `ProductVariant` as separate entities.

`ProductVariant` will contain:

* `Id`
* `ProductId`
* `Name`
* `SKU`
* `Barcode` (nullable)
* `IsActive`

Structured attributes such as `Color`, `Size`, or similar product-specific properties will not be modeled separately. They will be represented through the variant name when needed.

For example:

```text
Product: T-Shirt

ProductVariant:
- Name: Red / Small
  SKU: TS-RED-S

- Name: Red / Medium
  SKU: TS-RED-M

- Name: Blue / Small
  SKU: TS-BLUE-S
```

The SKU uniquely identifies a stockable product variant.

Every `Product` must have at least one `ProductVariant`, even when the product has no meaningful variations.

Therefore, all inventory records will reference `ProductVariantId`:

```text
StockLedgerEntry → ProductVariantId
StockBalance     → ProductVariantId
```

There will be no separate path where inventory references either `ProductId` or `ProductVariantId`.

## Consequences

### Positive

* Every stockable item has a consistent identity.
* `StockLedgerEntry` and `StockBalance` can always reference `ProductVariantId`.
* Products without meaningful variations do not require special inventory handling.
* The database model remains relatively simple.
* New descriptive variation combinations can be represented without changing the database schema.
* SKU provides a clear business identifier for stockable inventory.

### Tradeoffs

* Attributes such as color and size are not stored as structured data.
* Searching, filtering, or reporting specifically by attributes such as `Color` or `Size` is more limited.
* Product-specific attribute validation cannot be enforced through dedicated attribute fields.
* If the system later requires structured product attributes, the product model may need to be redesigned.

The reduced flexibility is acceptable because the current system focuses on inventory management rather than building a generalized product catalog or attribute-management platform.

The decision can be revisited if future requirements introduce significant needs for structured product attributes.
