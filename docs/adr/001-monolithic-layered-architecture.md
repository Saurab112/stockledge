# ADR-001: Monolithic, Layered Architecture

## Status

Accepted

## Context

This is a single-developer portfolio project focused on one business domain: inventory management.

The system does not currently require independent scaling of different components, multiple teams working on separate services, or distributed deployment. Introducing microservices would add unnecessary complexity, including inter-service communication, distributed transactions, and additional infrastructure and operational overhead.

## Decision

Build the system as a single application using a layered architecture consisting of:

* API
* Application
* Domain
* Infrastructure

All functional areas, including Products, Purchasing, Sales, Purchase returns, Sales returns, Transfers, Adjustments, and Reporting, will remain within the same application. Features will be organized internally rather than being separated into independent services.

## Consequences

### Positive

* Simpler development and deployment
* One codebase and database to manage
* Easier local development and testing
* Lower operational complexity
* Clear separation of responsibilities between layers

### Tradeoffs

A specific part of the system cannot be independently deployed or scaled without further architectural changes. For example, if reporting later required independent scaling, it could be extracted into a separate service as a deliberate future decision.

This tradeoff is acceptable because independent scaling is not a current requirement.
