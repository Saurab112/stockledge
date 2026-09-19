# ADR-008: Configurable Approval Workflow

## Status

Accepted

## Context

Some inventory operations may need approval before they affect inventory, while others can be applied immediately.

A fixed approval workflow would make the system less flexible because the required control may differ by operation or business configuration.

The system therefore needs to support both:

* Direct execution without approval
* Approval-required execution

The approval workflow must not change the underlying inventory rules. Stock availability, FIFO, reservations, ledger creation, and balance updates must behave the same regardless of whether the transaction is direct or approved.

## Decision

Approval behavior is configurable per supported transaction type.

Each supported operation can be configured as:

* **Direct** — transaction is applied immediately after normal validation.
* **Approval Required** — transaction remains pending until approved.

Currently, configurable approval applies to:

* Stock Transfer
* Stock Adjustment

Purchases, Sales, Purchase Returns, and Sales Returns are currently executed directly.

### Direct Workflow

```text
Create Transaction
      ↓
Validate
      ↓
Apply Inventory Changes
      ↓
Confirmed
```

### Approval-Required Workflow

```text
Create Transaction
      ↓
Validate
      ↓
Pending
      ↓
Approve ─────→ Apply Inventory Changes → Confirmed
   │
   └── Reject/Cancel → No Inventory Change
```

Pending transactions do not create Stock Ledger movements.

For transfers, requested stock is reserved while the transfer is pending:

```text
Pending Transfer
      ↓
Reserve Source Stock
      ↓
Approval
      ↓
Release Reservation
      ↓
Transfer Out + Transfer In
```

The reserved stock remains physically in the source warehouse but cannot be consumed by other operations.

Adjustments do not reserve stock while pending. A negative adjustment revalidates available stock when it is approved.

Approval must revalidate all relevant inventory conditions before applying the transaction. Approval does not bypass normal inventory rules.

The final inventory result of a transaction must be the same whether it was executed directly or through approval.

Approval configuration controls **when** a transaction is applied, not **how** inventory rules are enforced.

## Consequences

### Positive

* Approval behavior can be changed without redesigning transaction logic.
* Sensitive operations can require approval while simpler operations remain direct.
* Transfer reservations prevent pending transfers from consuming stock that may be used elsewhere.
* Approval does not weaken inventory validation.
* The same inventory logic can be reused for both direct and approved workflows.

### Tradeoffs

* Transactions may require additional workflow states.
* Approval-required transfers need reservation management.
* Approval requires revalidation because stock may change while a transaction is pending.
* Approval history and detailed authorization rules will add complexity.

## Open Considerations

The following are intentionally left for later design:

* Who is allowed to approve each transaction type.
* Role/permission requirements for approval.
* Approval audit details such as approver and approval timestamp.
* Whether multi-level approval is required in the future.
