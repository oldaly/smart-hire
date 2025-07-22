# SmartHire Architecture - Decision Log

This document captures key architectural decisions, trade-offs, and rationale for the SmartHire project.

---

## 🔐 Authentication

**Decision:** Use JWT (JSON Web Tokens) with Amazon Cognito User Pools

**Rationale:**
- Stateless authentication, ideal for Lambda-based APIs
- Native JWT validation via API Gateway
- Secure user management with Cognito
- Easy to implement role-based access control (Admin, Recruiter, Candidate)

**Alternatives Considered:**
- OAuth/OpenID Connect with a custom provider (more complex)
- Session-based cookies (not ideal for serverless)
- API keys (not user-specific, less secure)

**Trade-off Summary:** JWT via Cognito is a simple, secure, and scalable default for serverless apps.

---

## 🧩 Microservices Architecture

**Decision:** Implement separate Lambdas for:
- Job Service
- Candidate Service
- Application Service
- Notification Service

**Rationale:**
- Clear separation of concerns
- Services can scale and evolve independently
- Enables faster, safer deployments

**Alternatives Considered:**
- Monolith (easier but tightly coupled)
- Shared service (blurs responsibilities)

**Trade-off Summary:** Modular, microservice-oriented design balances complexity and future scalability.

---

## 🗃️ Database Design

**Decision:** Use one DynamoDB table per service
- `Jobs`
- `Candidates`
- `Applications`

**Rationale:**
- Clear data ownership per service
- Supports single-writer pattern
- Scales well with serverless patterns

**Alternatives Considered:**
- Shared DynamoDB table (more complex indexing)
- Aurora/PostgreSQL (more operations overhead)

**Trade-off Summary:** Serverless-native, isolated data model is ideal for a microservices backend.

---

## 📩 Event-Driven Communication

**Decision:** Use Amazon EventBridge and Amazon SQS for asynchronous workflows

**Rationale:**
- Decouples services
- Supports fan-out and retries
- Scales cleanly without tight dependencies

**Examples:**
- Application Service publishes to EventBridge
- Notification Service consumes messages via SQS

**Alternatives Considered:**
- Direct Lambda invocations (tight coupling)
- DynamoDB Streams (limited control/filtering)
- Kafka/MSK (heavy infrastructure for MVP)

**Trade-off Summary:** EventBridge + SQS provides the right level of simplicity and scalability.

---

## 🔧 DevOps and Infrastructure

**Decision:** Use GitHub Actions + Terraform + AWS SAM

**Rationale:**
- Automates deployments (CI/CD)
- Infrastructure as Code (IaC) using Terraform
- SAM simplifies Lambda packaging/testing

**Alternatives Considered:**
- Serverless Framework (additional abstraction)
- AWS CDK (powerful but steeper learning curve)
- Manual AWS Console management (not scalable)

**Trade-off Summary:** Transparent, repeatable, toolchain-friendly setup that supports team collaboration.

---

## 📊 Monitoring and Secrets

**Decision:**
- Use Amazon CloudWatch for logs and metrics
- Use AWS Secrets Manager for credential storage

**Rationale:**
- Follows AWS best practices
- Keeps secrets secure and auditable
- Centralized logging across services

---

## 👤 Roles and User Model

**Roles:** Admin, Recruiter, Candidate

**Decision:**
- Keep multiple roles to support future permission models
- Admin role may be unused initially but retained for extensibility

**Trade-off Summary:** Keeps options open without introducing current complexity.

---

## 📦 Service Behavior Summary

| Service             | Owns Data     | Triggers Events | Notes                                    |
|---------------------|---------------|------------------|------------------------------------------|
| Job Service         | Yes (`Jobs`)  | No               | CRUD only                                 |
| Candidate Service   | Yes (`Candidates`) | No         | CRUD only                                 |
| Application Service | Yes (`Applications`) | Yes     | Publishes events to EventBridge           |
| Notification Service| No            | Consumes events  | Reads messages from SQS, no DB needed     |

---

## 📚 Future Considerations

- Add triggers to Job/Candidate Services only if business rules require
- Consider enterprise SSO later if Cognito becomes limiting
- Explore DynamoDB Streams only if fine-grained DB triggers are required

---

*Last updated: July 22, 2025*

