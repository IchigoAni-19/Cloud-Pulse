<div align="center">

# ⚡ CloudPulse

### **Production-Grade Cloud Asset Tracking, Health Monitoring & SaaS Telemetry Platform**

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Vue 3](https://img.shields.io/badge/Vue.js-3.5-4FC08D?style=for-the-badge&logo=vuedotjs&logoColor=white)](https://vuejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-3.4-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white)](https://tailwindcss.com/)
[![Vite](https://img.shields.io/badge/Vite-8.x-646CFF?style=for-the-badge&logo=vite&logoColor=white)](https://vitejs.dev/)

<p align="center">
  <a href="#-key-features">Key Features</a> •
  <a href="#-system-architecture">System Architecture</a> •
  <a href="#-tech-stack">Tech Stack</a> •
  <a href="#-getting-started">Getting Started</a> •
  <a href="#-automated-demo-seeder">Demo Data Seeder</a> •
  <a href="#-enterprise-billing--checkout">Billing & Checkout</a> •
  <a href="#-api-reference">API Reference</a> •
  <a href="#-database-schema">Database Schema</a> •
  <a href="#-security--best-practices">Security</a> •
  <a href="#-license">License</a>
</p>

---

</div>

## 🌐 Overview

**CloudPulse** is an enterprise-ready cloud infrastructure telemetry platform engineered to monitor HTTP/S endpoints, REST APIs, databases, background workers, and microservices in real time. 

Built with **ASP.NET Core 8 Clean Architecture** on the backend and **Vue 3 (Composition API) + Shadcn / Tailwind CSS** on the frontend, CloudPulse delivers sub-second incident detection, historical latency time-series graphs, multi-tier subscription quotas, multi-factor authentication (Email/Password, Google OAuth 2.0, Phone OTP), and an enterprise-grade billing system with dynamic invoice generation.

---

## ✨ Key Features

- 🛰️ **Autonomous Telemetry Background Worker**: Non-blocking `IHostedService` executing periodic asynchronous ping sweeps every 30s with auto status degradation (`Healthy`, `Degraded`, `Down`).
- 📊 **Real-time Latency Timeline & Graphs**: Interactive SVG time-series visualization rendering response times, HTTP status codes, and uptime SLAs.
- 🚨 **Incident Detection Banner**: Instant pulsing incident detection bar triggering whenever monitored nodes drop into critical/down states.
- 🔐 **Multi-Method Enterprise Authentication**:
  - JWT Bearer Authentication with HMAC-SHA256 and custom claim validation.
  - Secure BCrypt salted password hashing.
  - Google OAuth 2.0 One-Tap and JWT validation with auto profile provisioning.
  - Phone Number Authentication with cryptographically safe 5-minute TTL OTP verification.
- 💳 **Enterprise Subscription & Billing System**:
  - **Free Tier**: 3 Cloud Assets max, 60s sweep interval, basic telemetry.
  - **Pro Tier**: Unlimited Cloud Assets, 10s ping sweeps, real-time alerts, priority 24/7 support.
  - **Monthly & Annual Cycles**: Interactive billing toggle with automatic 17% annual discount calculation.
  - **2-Column Checkout Drawer**: Live card brand detection (Visa, Mastercard, Amex, RuPay), Luhn & expiry validation, UPI/QR with live countdown timer, and Netbanking bank selector.
  - **3-Stage Payment Authorization**: AES-256 payload encryption, 3D Secure 2.0 issuing bank handshake, and subscription provisioning.
  - **Official Invoices & Receipts**: Itemized tax breakdown (18% GST / Sales Tax), printable/downloadable receipts with invoice reference numbers (`INV-YYYYMMDD-XXXXXX`), and historical invoice ledger.
  - **Active Subscription Management**: Next renewal date tracking, active payment method indicator (`Visa ending in •••• 4242`), and modal-confirmed cancellation workflow.
- 🌓 **Dual-Theme Design System**: Tailored HSL Dark Canvas (`#0B0C10` with `#9333EA` purple accent) and Light Mauve theme (`#F2EBF4` with `#2563EB` blue accent) with glassmorphism navigation.

---

## 🏛️ System Architecture

```mermaid
flowchart TB
    subgraph Frontend ["🖥️ cloudpulse-ui (Vue 3 + TypeScript + Vite)"]
        UI_VIEWS["Vue Views\n(Dashboard, AssetDetail, Billing, Auth)"]
        PINIA["Pinia Stores\n(auth.ts, assets.ts)"]
        AXIOS["Axios Interceptor Layer\n(Bearer Token Injection + Error Handlers)"]
        CHECKOUT_MODAL["Enterprise Checkout Engine\n(Card Brand Detection, UPI Timer, Validation)"]
        
        UI_VIEWS --> PINIA
        PINIA --> AXIOS
        UI_VIEWS --> CHECKOUT_MODAL
        CHECKOUT_MODAL --> AXIOS
    end

    subgraph Backend ["⚡ CloudPulse.Api (.NET 8 Web API)"]
        CONTROLLERS["API Controllers\n(AuthController, AssetController, MetricsController, PaymentController)"]
        SERVICES["Core Services\n(TokenService, OtpService)"]
        WORKER["HealthMonitoringWorker\n(Periodic Concurrent IHostedService)"]
        SEEDER["DbSeeder\n(Automated Realistic Telemetry Generator)"]
        EF_CORE["Entity Framework Core 8\n(AppDbContext)"]
        
        CONTROLLERS --> SERVICES
        CONTROLLERS --> EF_CORE
        WORKER --> EF_CORE
        SEEDER --> EF_CORE
    end

    subgraph Storage ["🐘 Data Persistence"]
        POSTGRES[("PostgreSQL 16\n(cloudpulse_db)")]
        EF_CORE --> POSTGRES
    end

    subgraph MonitoredNodes ["🌐 Monitored Infrastructure"]
        TARGET_APIS["Production APIs"]
        TARGET_SERVICES["Microservices & Workers"]
        TARGET_DBS["Database Gateways"]
        WORKER -.->|HTTP GET (5s CTS Timeout)| TARGET_APIS
        WORKER -.->|HTTP GET (5s CTS Timeout)| TARGET_SERVICES
        WORKER -.->|HTTP GET (5s CTS Timeout)| TARGET_DBS
    end

    AXIOS -->|REST API Calls /api/v1| CONTROLLERS
```

---

## 🛠️ Tech Stack

### **Backend (`CloudPulse.Api`)**
| Component | Technology | Description |
| :--- | :--- | :--- |
| **Runtime** | .NET 8.0 SDK (C# 12) | High-performance asynchronous runtime |
| **Framework** | ASP.NET Core Web API | RESTful API endpoints with structured JSON outputs |
| **ORM** | Entity Framework Core 8 | Fluent API mapping, composite indexes & cascade rules |
| **Database** | PostgreSQL 16 via Npgsql | Relational storage for users, assets, and health logs |
| **Authentication** | JWT Bearer & BCrypt.Net | Token issuance, claim management, secure salted hashing |
| **Background Runner** | `BackgroundService` | Asynchronous 30s concurrent HTTP health pollers with timeout guards |
| **Documentation** | Swagger / OpenAPI 3.0 | Interactive API explorer with Bearer authorization support |

### **Frontend (`cloudpulse-ui`)**
| Component | Technology | Description |
| :--- | :--- | :--- |
| **Framework** | Vue 3 (`<script setup lang="ts">`) | Modern Reactive Composition API |
| **Build Tool** | Vite 8 + TypeScript | Ultra-fast HMR and end-to-end type safety |
| **State Management**| Pinia | Centralized stores for Auth, Assets, and Telemetry |
| **Styling** | Tailwind CSS 3.4 + Reka UI | Custom design system with glassmorphism & dark mode tokens |
| **Icons** | Lucide Vue | Crisp modern vector iconography |
| **HTTP Client** | Axios | Request/response interceptors & global auth handling |

---

## 🚀 Getting Started

### **Prerequisites**
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18+ or v20+)](https://nodejs.org/) & `npm`
- [PostgreSQL](https://www.postgresql.org/) running on port `5432` (locally or via container)

---

### **1. PostgreSQL Setup**

Ensure PostgreSQL is running on port `5432` with username `postgres` and password `postgres`:

```bash
# Using Podman / Docker:
podman run -d --name cloudpulse-db \
  -p 5432:5432 \
  -e POSTGRES_DB=cloudpulse_db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  postgres:16-alpine
```

---

### **2. Backend Setup (`CloudPulse.Api`)**

```bash
# Navigate to backend directory
cd CloudPulse.Api

# Restore dependencies
dotnet restore

# Run the API server
dotnet run --urls "http://localhost:5000"
```

> **Note:** On startup, the backend automatically provisions `cloudpulse_db` tables (`Users`, `CloudAssets`, `AssetHealthLogs`, `PaymentRecords`) and populates rich demo telemetry via `DbSeeder.SeedAsync()`.

* **API Base URL:** `http://localhost:5000`
* **Swagger API Explorer:** `http://localhost:5000/swagger`

---

### **3. Frontend Setup (`cloudpulse-ui`)**

```bash
# Navigate to frontend directory
cd cloudpulse-ui

# Install dependencies
npm install

# Start Vite Development Server
npm run dev
```

* **Frontend URL:** `http://localhost:5173`

---

## 🧪 Automated Demo Seeder

CloudPulse includes an autonomous database seeder (`Data/DbSeeder.cs`) that provisions a pre-configured engineering account with realistic 24-hour telemetry:

* **Demo User:** `harsh@test.com`
* **Password:** `Password123!`
* **Tier:** `Pro` (Unlocked unlimited assets)

### **Pre-Seeded Mock Assets:**
| Asset Name | Type | Environment | Status | Target Endpoint |
| :--- | :--- | :--- | :--- | :--- |
| **`Production API Gateway`** | `API` | `Production` | 🟢 **Healthy** (~124ms) | `https://httpbin.org/status/200` |
| **`Postgres Cluster Primary`** | `Database` | `Production` | 🟢 **Healthy** (~42ms) | `https://httpbin.org/status/200` |
| **`Redis Cache Staging`** | `Database` | `Staging` | 🟡 **Degraded** (~1940ms) | `https://httpbin.org/delay/2` |
| **`Staging Webhook Worker`** | `Worker` | `Staging` | 🔴 **Down** (500 Server Error) | `https://httpbin.org/status/500` |

* **Historical Logs:** **120+ time-series data points** spread across the previous 24 hours for instant chart rendering upon first login.

---

## 💳 Enterprise Billing & Checkout

CloudPulse features a production-grade purchase and billing management system modeled after platforms like **Claude Pro, Gemini Advanced, and YouTube Premium**:

```
                               ┌────────────────────────────────┐
                               │  Billing Cycle & Plan Selection│
                               │  (Monthly: ₹2,499 / Annual)   │
                               └───────────────┬────────────────┘
                                               │
                                               ▼
                               ┌────────────────────────────────┐
                               │ 2-Column Checkout Drawer / UI  │
                               │  - Itemized Tax Breakdown      │
                               │  - Card / UPI / Netbanking Tabs│
                               │  - Strict Live Form Validation │
                               └───────────────┬────────────────┘
                                               │
                                               ▼
                               ┌────────────────────────────────┐
                               │ 3-Stage Security Authorization │
                               │ 1. Payload AES-256 Encryption  │
                               │ 2. 3D Secure 2.0 Bank Auth     │
                               │ 3. Pro Tier Backend Capture    │
                               └───────────────┬────────────────┘
                                               │
                                               ▼
                               ┌────────────────────────────────┐
                               │ Invoices & Subscription Hub    │
                               │ - Active Pro Subscription Card │
                               │ - Historical Invoice Ledger    │
                               │ - Downloadable/Printable PDF   │
                               └────────────────────────────────┘
```

1. **Strict Input Validation:** Form validation blocks incomplete or malformed inputs (invalid 16-digit card numbers, expired dates, missing CVV/cardholder, or bad UPI VPA formats) with inline alerts.
2. **Auto-Fill Test Helper:** Includes a `✨ Auto-Fill Test Payment Details` helper button for rapid manual and regression testing.
3. **Invoice Generation:** Every successful transaction generates an official receipt with invoice number (`INV-YYYYMMDD-XXXXXX`), itemized GST breakdown, payment method identifier, and printable layout.
4. **Subscription Lifecycle:** Active Pro subscribers can view their next billing date, active card preview, and cancel/downgrade subscription with confirmation safeguards.
5. **Developer Sandbox Controls:** Includes a `Reset to Free Tier` button to test the entire upgrade journey repeatedly.

---

## 📡 API Reference

### 🔐 **Authentication (`/api/v1/auth`)**
| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Register user with email, password & optional phone | None |
| `POST` | `/api/v1/auth/login` | Authenticate with email & password | None |
| `POST` | `/api/v1/auth/google` | Authenticate / auto-provision via Google ID Token | None |
| `POST` | `/api/v1/auth/phone/send-otp` | Dispatch phone verification OTP (with dev OTP in body) | None |
| `POST` | `/api/v1/auth/phone/verify-otp`| Validate phone OTP and generate JWT | None |
| `POST` | `/api/v1/auth/seed` | Trigger on-demand demo database seeding | None |

### ☁️ **Cloud Assets (`/api/v1/assets`)**
| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/assets` | Retrieve caller's registered cloud assets (filtered by `env` / `type`) | 🔒 JWT |
| `POST` | `/api/v1/assets` | Register a new asset (enforces Free tier max 3 limit) | 🔒 JWT |
| `GET` | `/api/v1/assets/{id}` | Fetch asset detail by ID | 🔒 JWT |
| `DELETE` | `/api/v1/assets/{id}` | Delete asset and cascade remove all health check logs | 🔒 JWT |
| `POST` | `/api/v1/assets/{id}/ping` | Trigger an immediate out-of-band health check sweep | 🔒 JWT |

### 📈 **Metrics & Telemetry (`/api/v1/metrics`)**
| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/metrics/dashboard` | Aggregated uptime percentage, total assets, and status breakdown | 🔒 JWT |
| `GET` | `/api/v1/metrics/{id}/history`| Last 50 health checks for SVG time-series latency charts | 🔒 JWT |

### 💳 **Billing & Payments (`/api/v1/payments`)**
| Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/payments/create-order` | Generate order payload for Monthly or Annual subscription | 🔒 JWT |
| `POST` | `/api/v1/payments/verify` | Verify transaction signature and upgrade caller to `Pro` | 🔒 JWT |
| `GET` | `/api/v1/payments/invoices` | Fetch all historical captured invoices and receipts | 🔒 JWT |
| `POST` | `/api/v1/payments/cancel-subscription` | Cancel active Pro subscription and revert to Free tier | 🔒 JWT |
| `POST` | `/api/v1/payments/reset-tier` | Sandbox reset to Free tier for testing | 🔒 JWT |

---

## 🗄️ Database Schema

```
Users
├── Id (Guid, Primary Key)
├── Email (string, Unique Index)
├── PhoneNumber (string, Nullable)
├── PasswordHash (string, Nullable)
├── GoogleSubjectId (string, Indexed)
├── Role ("Admin" | "Engineer")
├── SubscriptionTier ("Free" | "Pro")
└── CreatedAt (DateTime UTC)

CloudAssets
├── Id (Guid, Primary Key)
├── UserId (Guid, Foreign Key -> Users.Id, Restrict Delete)
├── Name (string, MaxLength 100)
├── TargetUrl (string)
├── ResourceType ("API" | "Database" | "VM" | "Worker")
├── Environment ("Production" | "Staging" | "Development")
├── CurrentStatus ("Healthy" | "Degraded" | "Down" | "Unknown")
├── LastLatencyMs (int)
├── LastCheckedAt (DateTime UTC, Nullable)
├── CheckIntervalSeconds (int, Default 60)
└── IsActive (bool, Default true)

AssetHealthLogs
├── Id (bigint, Primary Key AutoIncrement)
├── CloudAssetId (Guid, Foreign Key -> CloudAssets.Id, Cascade Delete)
├── HttpStatusCode (int)
├── LatencyMs (int)
├── IsSuccessful (bool)
├── ErrorMessage (string, Nullable)
└── CheckedAt (DateTime UTC, Composite Indexed with CloudAssetId)

PaymentRecords
├── Id (Guid, Primary Key)
├── UserId (Guid, Foreign Key -> Users.Id, Restrict Delete)
├── RazorpayOrderId (string)
├── RazorpayPaymentId (string, Nullable)
├── RazorpaySignature (string, Nullable)
├── Amount (decimal)
├── Currency ("INR")
├── Status ("Created" | "Captured" | "Failed")
├── TargetTier ("Free" | "Pro")
└── CreatedAt (DateTime UTC)
```

---

## 📂 Project Structure

```
Cloud-Pulse/
├── CloudPulse.Api/                 # ASP.NET Core 8 Web API
│   ├── Controllers/               # REST Controllers (Auth, Asset, Metrics, Payment)
│   ├── Data/                      # AppDbContext, DbSeeder, Fluent API configurations
│   ├── Dtos/                      # Type-safe Data Transfer Objects & validation rules
│   ├── Models/                    # Domain entities (User, CloudAsset, AssetHealthLog, PaymentRecord)
│   ├── Services/                  # TokenService, OtpService, HealthMonitoringWorker
│   ├── Properties/                # launchSettings.json with development profile
│   ├── appsettings.json           # Connection strings & JWT security configuration
│   ├── Program.cs                 # Dependency injection, CORS policies, middleware pipeline
│   └── CloudPulse.Api.csproj      # .NET 8 Project file & NuGet package dependencies
│
├── cloudpulse-ui/                 # Vue 3 Frontend Single Page Application
│   ├── src/
│   │   ├── api/                   # Central Axios client with JWT interceptors
│   │   ├── components/            # Shadcn UI primitives & custom layout components
│   │   │   ├── dashboard/         # AddAssetModal & telemetry dashboard widgets
│   │   │   ├── layout/            # Glassmorphism Navbar & theme switch
│   │   │   └── ui/                # Button, Card, Badge, Table, Dialog, Input, Dropdown
│   │   ├── composables/           # Google Identity Services SDK composable
│   │   ├── router/                # Navigation guards and route definitions
│   │   ├── stores/                # Pinia state stores (auth.ts, assets.ts)
│   │   ├── views/                 # AuthView, DashboardView, AssetDetailView, BillingView
│   │   ├── style.css              # Dual-theme design system tokens & CSS animations
│   │   ├── App.vue                # Root application shell
│   │   └── main.ts                # Application bootstrapping
│   ├── vite.config.ts             # Vite configuration with /api proxy
│   └── package.json               # Frontend dependencies & build scripts
│
├── .gitignore                     # Global gitignore configuration
└── Readme.md                      # Comprehensive project documentation
```

---

## 🔒 Security & Best Practices

- **Zero-Plaintext Passwords**: Salted password hashing with BCrypt (`BCrypt.Net.BCrypt.HashPassword`).
- **Cryptographic JWT Tokens**: Signed with HMAC-SHA256, strictly validated for expiration, audience, and issuer with zero clock skew.
- **Timing-Attack Mitigation**: Constant-time byte array comparison (`FixedTimeEquals`) used during payment signature verification.
- **Resilient Background Polling**: Isolated try/catch scopes with 5-second `CancellationTokenSource` timeouts to prevent hanging network sockets from blocking worker threads.
- **Scoped Database Operations**: Ephemeral service scopes in background services preventing database context concurrency conflicts.
- **CORS Lockdown**: Explicit origin whitelist preventing unauthorized cross-origin tampering while allowing local development on port 5173.

---

## 📄 License

This project is licensed under the **MIT License**. Feel free to use, modify, and distribute it for personal and commercial projects.
