# ADR-003: Stock Ledger as Source of Truth

## Status

Accepted

## Context

The system needs two things that pull in different directions: a complete history of every stock movement, which is required for traceability and FIFO costing, and a fast way to determine the current stock quantity for a product in a warehouse.

Three approaches were considered.

### Option 1: Ledger Only

Store every stock movement in a ledger and calculate the current quantity by aggregating ledger entries whenever stock is queried.

This provides complete history but makes frequent stock checks more expensive. It also makes current-stock validation at write time more complex because the available quantity must be calculated from the ledger.

### Option 2: Balance Only

Maintain only the current stock quantity.

This provides fast reads but loses the history of how the quantity was produced. It also cannot represent the individual incoming stock layers required for FIFO costing.

This option was rejected.

### Option 3: Ledger + Balance

Maintain an append-only stock ledger as the source of truth and a separate stock balance as a derived representation of the current quantity.

The balance is updated together with the ledger within the same database transaction.

This option was chosen.

## Decision

Every stock-changing operation will create one or more entries in an append-only stock ledger.

Ledger entries are immutable: they are not updated or deleted after being recorded. Corrections must be represented through new compensating stock movements rather than modifying historical entries.

A separate stock balance will maintain the current quantity for each relevant product and warehouse combination. The balance is a derived representation of the ledger, not an independent source of truth.

Stock-changing operations must update both the ledger and balance within the same database transaction so that a successful operation cannot persist one without the other.

The balance must be recoverable by recalculating the relevant movements from the ledger.

## Consequences

### Positive

* Complete and auditable history of stock movements.
* FIFO costing can use incoming ledger entries as individual cost layers.
* Current stock can be read efficiently without aggregating the entire ledger.
* Stock validation can use the current balance during stock-changing operations.
* Historical inventory movements remain immutable and traceable.
* The balance can be reconstructed from the ledger if required.

### Tradeoffs

* Every stock-changing operation must write to both the ledger and the balance.
* Transaction boundaries become important because both representations must remain consistent.
* The system needs a reconciliation mechanism to detect or correct any unexpected drift between the ledger and balance.
* The balance introduces additional data that must be maintained correctly.

The additional complexity is acceptable because inventory correctness, traceability, and efficient stock reads are more important for this system than minimizing the number of database writes.
