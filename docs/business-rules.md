# StockLedger — Business Rules

This document lists the business rules the system must enforce, independent of how they are implemented in code. Each rule is stated as a plain requirement. Rules that are still under discussion are marked as open at the end of their section.

## Company and Warehouse

1. The system supports a single company.
2. A company can have multiple warehouses.
3. Stock quantities are tracked separately per warehouse. Stock in one warehouse has no effect on stock in another warehouse.
4. Warehouse is the smallest location unit tracked. Bin or shelf level tracking is not supported.

## Products

1. Every product must have a name, a code or SKU, a category, a base unit of measure, and an active or inactive status.
2. A product can have one or more variants (for example, different colors or sizes). Each variant is tracked as its own stockable unit.
3. Whether product variants use fixed attributes or a dynamic attribute model is not yet decided.
4. Whether product categories are flat or hierarchical is not yet decided.

## Units of Measure

1. A product has a base unit of measure. All stock quantities are ultimately expressed in this base unit.
2. Transactions may be recorded in a different unit of measure than the base unit, using a conversion factor (for example, 1 box equals 12 pieces).
3. The conversion factor used in a transaction is fixed at the time the transaction occurs. Changing a product's unit of measure or conversion factor later does not change the value already recorded on past transactions.
4. Whether a base unit of measure or conversion factor can be changed after transactions already exist is not yet decided.

## Inventory Tracking

1. Every change to stock is recorded as an entry in an inventory ledger. The ledger is the source of truth for inventory history.
2. The ledger is append only. Existing ledger entries are never modified or deleted.
3. A separate stock balance is kept for each warehouse and product variant as a cached total. This balance is derived from the ledger and can be recalculated from it at any time.
4. Any operation that changes stock must update the ledger and the stock balance together, as a single unit of work. Partial updates are not allowed.
5. If the stock balance and the ledger ever disagree, the ledger is treated as correct.

## Negative Stock

1. Stock quantity for a product variant in a warehouse can never go below zero.
2. An operation that would cause stock to go below zero must be rejected in full. It is not allowed to partially apply the operation.
3. This rule must hold even when two or more operations affecting the same stock happen at the same time. The exact mechanism for enforcing this under concurrent access is not yet decided.

## Purchasing

1. Receiving stock from a supplier increases the stock balance in the receiving warehouse.
2. A purchase does not require approval before it takes effect.
3. Receiving stock creates a new inventory ledger entry, which also serves as a cost layer for FIFO costing.

## Sales

1. Selling stock decreases the stock balance in the warehouse the sale is made from.
2. A sale does not require approval before it takes effect.
3. A sale cannot be completed if it would cause stock to go below zero.

## Inventory Costing

1. Inventory cost is calculated using first in, first out (FIFO).
2. When stock is sold or otherwise removed, the oldest available cost layer is consumed first.
3. Cost of goods sold and remaining inventory value are both derived from which cost layers have been consumed.

## Stock Transfers

1. Moving stock between warehouses requires approval before it takes effect.
2. A transfer does not move stock directly from the source warehouse to the destination warehouse in one step. Stock is first removed from the source warehouse and placed into an in-transit state, and only added to the destination warehouse once it is marked as received.
3. The expected transfer states are: draft, submitted, approved, dispatched, in transit, received. Cancellation and rejection states are not yet finalized.

## Stock Adjustments

1. A stock adjustment is used when the physical count of inventory differs from the recorded stock balance.
2. Whether a stock adjustment requires approval is proposed but not yet confirmed.
3. The proposed adjustment states are: draft, submitted, approved, applied.

## Returns

1. Purchase returns and sales returns are in scope for this system.
2. The detailed rules for how a return affects stock balance, ledger entries, and FIFO cost layers are not yet decided.

## Approvals

1. Approval is required only for operations considered sensitive to the business. Not every operation requires approval.
2. Purchases and sales do not require approval. Stock transfers require approval. Stock adjustments are proposed to require approval.
3. Who is allowed to approve an operation is not yet decided.

## Explicitly Not Supported in This Version

1. Multiple companies or tenants are not supported.
2. Individual serial number tracking is not supported.
3. Stock reservations are not supported.
4. Approval is not required for every operation by default.