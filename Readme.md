<div align="center">

# ⚡ CloudPulse

### **Production-Grade Cloud Asset Tracking, Health Monitoring & SaaS Telemetry Platform**

[![Live Demo](https://img.shields.io/badge/🌐_Live_Demo-Online_%26_Healthy-00C853?style=for-the-badge&logo=azure&logoColor=white)](https://cloudpulse-window-huc9hafmf5dpd6aw.indiasouthcentral-01.azurewebsites.net)
[![Docker Hub](https://img.shields.io/badge/Docker_Hub-patelharsh19%2Fcloudpulse-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://hub.docker.com/r/patelharsh19/cloudpulse)
[![Neon Database](https://img.shields.io/badge/Neon-Serverless_Postgres-00E599?style=for-the-badge&logo=postgresql&logoColor=black)](https://neon.tech)
<br/>
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Vue 3](https://img.shields.io/badge/Vue.js-3.5-4FC08D?style=for-the-badge&logo=vuedotjs&logoColor=white)](https://vuejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-3.4-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white)](https://tailwindcss.com/)
[![Vite](https://img.shields.io/badge/Vite-8.x-646CFF?style=for-the-badge&logo=vite&logoColor=white)](https://vitejs.dev/)
[![Azure App Service](https://img.shields.io/badge/Azure-App_Service_B1-0089D6?style=for-the-badge&logo=microsoftazure&logoColor=white)](https://azure.microsoft.com/)

<p align="center">
  <a href="#-live-deployment--cloud-architecture">Live Deployment</a> •
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

## 🚀 Live Deployment & Cloud Architecture

CloudPulse has transitioned from local development into a resilient, containerized **live cloud production environment**. To bypass subscription and regional quota constraints, the system utilizes a high-performance hybrid architecture connecting **Azure Web App for Containers** with **Neon Serverless PostgreSQL**.

<div align="center">

[![Live Application](https://img.shields.io/badge/🚀_LAUNCH_LIVE_APP-cloudpulse--window.azurewebsites.net-00C853?style=for-the-badge&logo=googlechrome&logoColor=white)](https://cloudpulse-window-huc9hafmf5dpd6aw.indiasouthcentral-01.azurewebsites.net)

</div>

### 🏗️ Production Topology & Data Flow

```mermaid
graph TD
    subgraph CLIENTS ["🌐 Client Access Layer"]
        LIVE_USER["🖥️ Production Users<br/><b>Azure Web App HTTPS</b>"]
        DEV_USER["💻 Local Development<br/><b>localhost:5173 / 8080</b>"]
    end

    subgraph AZURE_HOST ["☁️ Microsoft Azure — App Service (Linux B1 Tier)"]
        CORS_LAYER["🛡️ Strict CORS Guard<br/><i>Domain Whitelist Filter</i>"]
        
        subgraph DOCKER_CONTAINER ["📦 Monolithic Container (patelharsh19/cloudpulse:latest)"]
            FRONTEND_UI["🎨 Vue 3 SPA<br/><i>Static Web Server (wwwroot)</i>"]
            BACKEND_API["⚡ ASP.NET Core 8 Web API<br/><i>Clean Architecture Controllers</i>"]
            WORKER_PROCESS["🛰️ HealthMonitoringWorker<br/><i>Autonomous BackgroundService</i>"]
        end
        
        APP_CONFIG["🔐 Azure App Settings<br/><i>Connection Strings & Secrets</i>"]
    end

    subgraph CLOUD_DB ["🐘 Cloud Storage Layer"]
        NEON_PG[("🗄️ Neon Serverless PostgreSQL<br/><b>SSL/TLS Encrypted</b>")]
    end

    subgraph MONITORED_TARGETS ["🎯 Monitored Cloud Infrastructure"]
        ENDPOINTS["🌐 Target APIs & Microservices<br/><i>Latency & Uptime Sweeps</i>"]
    end

    LIVE_USER -->|HTTPS:443| CORS_LAYER
    DEV_USER -->|HTTP:5173/8080| CORS_LAYER
    CORS_LAYER --> FRONTEND_UI
    CORS_LAYER --> BACKEND_API
    APP_CONFIG -.->|Environment Injection| BACKEND_API
    BACKEND_API <-->|EF Core 8 / Npgsql| NEON_PG
    WORKER_PROCESS -->|30s Non-blocking Pings| ENDPOINTS

    classDef clientStyle fill:#1e293b,stroke:#38bdf8,stroke-width:2px,color:#f8fafc;
    classDef azureStyle fill:#0f172a,stroke:#0ea5e9,stroke-width:2px,color:#f8fafc;
    classDef containerStyle fill:#1e1b4b,stroke:#a855f7,stroke-width:2px,color:#f8fafc;
    classDef dbStyle fill:#064e3b,stroke:#10b981,stroke-width:2px,color:#f8fafc;
    classDef targetStyle fill:#701a75,stroke:#ec4899,stroke-width:2px,color:#f8fafc;
    classDef secStyle fill:#451a03,stroke:#f59e0b,stroke-width:2px,color:#f8fafc;

    class LIVE_USER,DEV_USER clientStyle;
    class CORS_LAYER,APP_CONFIG secStyle;
    class FRONTEND_UI,BACKEND_API,WORKER_PROCESS containerStyle;
    class NEON_PG dbStyle;
    class ENDPOINTS targetStyle;
```

---

### 🌟 Technical Achievements & Cloud Infrastructure Highlights

| Dimension | Implementation Details |
| :--- | :--- |
| **🌐 Production URL** | [`https://cloudpulse-window-huc9hafmf5dpd6aw.indiasouthcentral-01.azurewebsites.net`](https://cloudpulse-window-huc9hafmf5dpd6aw.indiasouthcentral-01.azurewebsites.net) |
| **🐳 Unified Monolithic Container** | Multi-stage Docker build combining **Node 22** (Vue 3 frontend compilation) and **.NET 8 SDK** (API release publish), serving compiled static assets directly via ASP.NET Core `UseStaticFiles()` and `MapFallbackToFile("index.html")`. Public image repository: [`patelharsh19/cloudpulse:latest`](https://hub.docker.com/r/patelharsh19/cloudpulse). |
| **🐘 Serverless Database Pivot** | Shifted to **Neon Serverless PostgreSQL** to bypass Azure Student subscription regional Flexible Server quota restrictions. Applied remote **Entity Framework Core migrations** with schema auto-generation and dynamic demo data seeding on first container boot. |
| **☁️ Azure Cloud Provisioning** | Deployed on **Azure Web App for Containers** (Linux, Basic B1 tier) hosted in the `indiasouthcentral` region, configured for port `8080` with zero-downtime container replacement. |
| **🔐 Zero-Trust Secret Management** | Sensitive configuration (Neon ADO.NET connection strings, JWT HMAC-SHA256 signing keys, Google OAuth Client IDs) injected at runtime through **Azure App Settings**, guaranteeing zero plaintext credentials in source control. |
| **🛡️ Strict CORS Lockdown** | Hardened CORS policy using `AddDefaultPolicy` with an explicit VIP origin whitelist permitting only the live Azure production domain and authenticated local development ports (`5173`, `8080`). |

---

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
graph TD
    subgraph UI_TIER ["🖥️ Frontend Presentation Tier (Vue 3 + TypeScript)"]
        VIEWS["📊 Interactive Views<br/><i>Dashboard • AssetDetail • Billing • Auth</i>"]
        STORES["🗃️ Pinia State Management<br/><i>auth.ts • assets.ts</i>"]
        HTTP_CLIENT["🔐 Axios HTTP Client<br/><i>JWT Interceptor & Error Handler</i>"]
        CHECKOUT_SYS["💳 Enterprise Checkout Engine<br/><i>Live Card Detection • UPI • Validation</i>"]
        
        VIEWS --> STORES
        VIEWS --> CHECKOUT_SYS
        STORES --> HTTP_CLIENT
        CHECKOUT_SYS --> HTTP_CLIENT
    end

    subgraph API_TIER ["⚡ Backend Service Tier (ASP.NET Core 8 Clean Architecture)"]
        CONTROLLERS["📡 REST API Controllers<br/><i>AuthController • AssetController • MetricsController • PaymentController</i>"]
        SVC_LAYER["⚙️ Core Services<br/><i>TokenService • OtpService • DbSeeder</i>"]
        BG_SVC["🛰️ HealthMonitoringWorker<br/><i>IHostedService (30s Concurrent Pings)</i>"]
        ORM["🗄️ Entity Framework Core 8<br/><i>AppDbContext & LINQ Mapping</i>"]
        
        CONTROLLERS --> SVC_LAYER
        CONTROLLERS --> ORM
        SVC_LAYER --> ORM
        BG_SVC --> ORM
    end

    subgraph DATA_TIER ["🐘 Data Persistence Layer"]
        POSTGRES_DB[("🗄️ Neon Serverless PostgreSQL 16<br/><i>Users • CloudAssets • HealthLogs • Invoices</i>")]
    end

    subgraph TARGET_TIER ["🌐 Monitored Infrastructure"]
        NODE_TARGETS["🎯 External APIs, Microservices & Databases<br/><i>Out-of-band HTTP/S Health Sweeps</i>"]
    end

    HTTP_CLIENT -->|REST API Calls /api/v1| CONTROLLERS
    ORM <-->|Encrypted Npgsql TLS Driver| POSTGRES_DB
    BG_SVC -->|Async GET (5s CTS Timeout)| NODE_TARGETS

    classDef frontStyle fill:#0f172a,stroke:#38bdf8,stroke-width:2px,color:#f8fafc;
    classDef backStyle fill:#1e1b4b,stroke:#a855f7,stroke-width:2px,color:#f8fafc;
    classDef dataStyle fill:#064e3b,stroke:#34d399,stroke-width:2px,color:#f8fafc;
    classDef extStyle fill:#4c0519,stroke:#fb7185,stroke-width:2px,color:#f8fafc;

    class VIEWS,STORES,HTTP_CLIENT,CHECKOUT_SYS frontStyle;
    class CONTROLLERS,SVC_LAYER,BG_SVC,ORM backStyle;
    class POSTGRES_DB dataStyle;
    class NODE_TARGETS extStyle;
```

---

## 🛠️ Tech Stack

### **Cloud & DevOps Infrastructure**
| Component | Technology | Description |
| :--- | :--- | :--- |
| **Cloud Host** | Microsoft Azure App Service | Linux Basic B1 container app in India South Central |
| **Container Engine** | Docker & Docker Hub | Multi-stage image build (`patelharsh19/cloudpulse:latest`) |
| **Cloud Database** | Neon Serverless PostgreSQL | Auto-scaling managed Postgres instance with SSL/TLS |
| **Config & Secrets** | Azure App Settings | Environment variables for connection strings and JWT secrets |

### **Backend (`CloudPulse.Api`)**
| Component | Technology | Description |
| :--- | :--- | :--- |
| **Runtime** | .NET 8.0 SDK (C# 12) | High-performance asynchronous runtime |
| **Framework** | ASP.NET Core Web API | RESTful API endpoints with structured JSON outputs |
| **ORM** | Entity Framework Core 8 | Fluent API mapping, composite indexes & cascade rules |
| **Database Driver** | Npgsql.EntityFrameworkCore | High-throughput PostgreSQL data provider |
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

## 🐳 Docker & Container Deployment Guide

The entire full-stack solution is built into a single lightweight production container utilizing multi-stage builds:

### **1. Build Local Image**
```bash
docker build -t cloudpulse:latest .
```

### **2. Run Container Locally**
```bash
docker run -d -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=Host=ep-xyz.indiasouthcentral.azure.neon.tech;Database=neondb;Username=neondb_owner;Password=secret;SSL Mode=Require;Trust Server Certificate=true" \
  -e "JwtSettings__Secret=YOUR_LONG_HMAC_SHA256_SECRET_KEY_HERE_12345" \
  --name cloudpulse-app \
  cloudpulse:latest
```

### **3. Tag and Push to Docker Hub**
```bash
# Tag for Docker Hub
docker tag cloudpulse:latest patelharsh19/cloudpulse:latest

# Push to Docker Hub
docker push patelharsh19/cloudpulse:latest
```

---

## 🚀 Getting Started (Local Development)

### **Prerequisites**
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18+ or v20+)](https://nodejs.org/) & `npm`
- [PostgreSQL](https://www.postgresql.org/) or [Neon Postgres](https://neon.tech) account

---

### **1. Backend Setup (`CloudPulse.Api`)**

```bash
# Navigate to backend directory
cd CloudPulse.Api

# Restore dependencies
dotnet restore

# Run the API server
dotnet run --urls "http://localhost:5000"
```

> **Note:** On startup, the backend automatically provisions database tables (`Users`, `CloudAssets`, `AssetHealthLogs`, `PaymentRecords`) and populates rich demo telemetry via `DbSeeder.SeedAsync()`.

* **API Base URL:** `http://localhost:5000`
* **Swagger API Explorer:** `http://localhost:5000/swagger`

---

### **2. Frontend Setup (`cloudpulse-ui`)**

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
├── Dockerfile                     # Multi-stage production container build (Node 22 + .NET 8)
├── .dockerignore                  # Docker build context optimizations
├── .gitignore                     # Global gitignore configuration
└── Readme.md                      # Comprehensive project documentation & deployment guide
```

---

## 🔒 Security & Production Hardening

- **🔒 Strict VIP CORS Lockdown**: The backend API completely locks down cross-origin resource sharing to the live Azure production domain (`https://cloudpulse-window-huc9hafmf5dpd6aw.indiasouthcentral-01.azurewebsites.net`) and local development endpoints (`http://localhost:5173`, `http://localhost:8080`).
- **🛡️ Managed Cloud Secrets**: Zero plain-text secrets in the repository; production database connection strings and JWT signing tokens are managed through Azure App Service App Settings.
- **🔑 Zero-Plaintext Passwords**: Salted password hashing with BCrypt (`BCrypt.Net.BCrypt.HashPassword`).
- **🎫 Cryptographic JWT Tokens**: Signed with HMAC-SHA256, strictly validated for expiration, audience, and issuer with zero clock skew.
- **⚡ Timing-Attack Mitigation**: Constant-time byte array comparison (`FixedTimeEquals`) used during payment signature verification.
- **🛰️ Resilient Background Polling**: Isolated try/catch scopes with 5-second `CancellationTokenSource` timeouts to prevent hanging network sockets from blocking worker threads.
- **📦 Scoped Database Operations**: Ephemeral service scopes in background services preventing database context concurrency conflicts on Neon Serverless PostgreSQL.

---

## 📄 License

This project is licensed under the **MIT License**. Feel free to use, modify, and distribute it for personal and commercial projects.
