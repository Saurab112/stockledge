# ADR-002: Single-Tenant Scope

## Status

Accepted

## Context

Multi-tenancy, where multiple companies use the same system while keeping their data isolated, is common in production inventory software. However, supporting it introduces additional architectural and data-isolation concerns, such as tenant identification, database isolation, authorization boundaries, and preventing cross-tenant data access.

The primary goal of this project is to explore inventory-domain problems such as stock movements, ledger tracking, FIFO costing, warehouse transfers, and inventory rules. Introducing multi-tenancy at this stage would add significant infrastructure and data-isolation concerns that are not required by the current requirements.

## Decision

The system will support a single company with multiple warehouses.

Multi-tenancy is explicitly out of scope for the current version. All data belongs to the single company represented by the system.

## Consequences

### Positive

* The data model remains focused on inventory-domain concerns.
* Authorization and data-access rules are simpler.
* There is no need for tenant-resolution or cross-tenant isolation logic.
* Development and testing remain focused on the core inventory workflows.

### Tradeoffs

The system cannot support multiple independent companies within the same application and database.

If multi-tenancy is introduced later, the system would need a deliberate redesign around tenant identification, data isolation, authorization, and database access. This would be treated as a separate architectural change rather than something implicitly supported by the current design.
