<div align="center">

# ⚡ CloudPulse

### **Production-Grade Cloud Asset Tracking, Health Monitoring & SaaS Telemetry Dashboard**

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
  <a href="#-api-reference">API Reference</a> •
  <a href="#-database-schema">Database Schema</a> •
  <a href="#-license">License</a>
</p>

---

</div>

## 🌐 Overview

**CloudPulse** is an enterprise-ready cloud infrastructure telemetry platform engineered to monitor HTTP/S endpoints, APIs, workers, databases, and microservices in real time. 

Built with **ASP.NET Core 8 Clean Architecture** on the backend and **Vue 3 (Composition API) + Shadcn / Tailwind CSS** on the frontend, CloudPulse delivers sub-second incident detection, historical latency timelines, multi-tier subscription quotas, and multi-factor authentication (Email/Password, Google OAuth 2.0, and Phone OTP).

---

## ✨ Key Features

- 🛰️ **Autonomous Telemetry Background Worker**: Non-blocking `IHostedService` executing periodic asynchronous ping sweeps every 30s with auto status degradation (`Healthy`, `Degraded`, `Down`).
- 📊 **Real-time Latency Timeline & Graphs**: Interactive SVG time-series visualization rendering response times, HTTP status codes, and uptime SLAs.
- 🚨 **Incident Detection Banner**: Instant pulsing incident detection bar triggering whenever monitored nodes drop into critical/down states.
- 🔐 **Multi-Method Enterprise Auth**:
  - JWT Bearer Authentication with HMAC-SHA256 and custom claim validation.
  - Secure BCrypt salted password hashing.
  - Google OAuth 2.0 One-Tap and JWT validation with auto profile provisioning.
  - Phone Number Authentication with cryptographically safe 5-minute TTL OTP verification.
- 💳 **SaaS Monetization & Plan Tiers**:
  - **Free Tier**: 3 Cloud Assets max, 60s sweep interval, basic metrics.
  - **Pro Tier**: Unlimited Cloud Assets, 10s ping sweeps, real-time alerts, Razorpay order verification with HMAC-SHA256 signature checking.
- 🌓 **Dual-Theme Design System**: Tailored HSL Dark Canvas (`#0B0C10` with `#9333EA` purple accent) and Light Mauve theme (`#F2EBF4` with `#2563EB` blue accent) with glassmorphism navigation.

---

## 🏛️ System Architecture

```mermaid
flowchart TB
    subgraph Frontend ["🖥️ cloudpulse-ui (Vue 3 + Vite + Pinia)"]
        UI_VIEWS["Vue Views\n(Dashboard, Detail, Billing, Auth)"]
        PINIA["Pinia Stores\n(auth.ts, assets.ts)"]
        AXIOS["Central Axios Client\n(JWT Interceptor + Error Handler)"]
        UI_VIEWS --> PINIA
        PINIA --> AXIOS
    end

    subgraph Backend ["⚡ CloudPulse.Api (.NET 8 Web API)"]
        CONTROLLERS["API Controllers\n(Auth, Asset, Metrics, Payment)"]
        SERVICES["Services Layer\n(TokenService, OtpService)"]
        WORKER["HealthMonitoringWorker\n(Periodic Background Runner)"]
        EF_CORE["Entity Framework Core 8\n(AppDbContext)"]
        
        CONTROLLERS --> SERVICES
        CONTROLLERS --> EF_CORE
        WORKER --> EF_CORE
    end

    subgraph Storage ["🐘 Data Persistence"]
        POSTGRES[("PostgreSQL\n(cloudpulse_db)")]
        EF_CORE --> POSTGRES
    end

    subgraph MonitoredNodes ["🌐 Monitored Infrastructure"]
        TARGET_APIS["Production APIs"]
        TARGET_SERVICES["Microservices & VMs"]
        WORKER -.->|HTTP GET (5s Timeout)| TARGET_APIS
        WORKER -.->|HTTP GET (5s Timeout)| TARGET_SERVICES
    end

    AXIOS -->|REST API Requests /api/v1| CONTROLLERS
```

---

## 🛠️ Tech Stack

### **Backend (`CloudPulse.Api`)**
| Component | Technology | Description |
| :--- | :--- | :--- |
| **Runtime** | .NET 8.0 SDK (C# 12) | High-performance asynchronous runtime |
| **Framework** | ASP.NET Core Web API | RESTful API endpoints |
| **ORM** | Entity Framework Core 8 | Fluent API mapping, composite indexes & cascade rules |
| **Database** | PostgreSQL 16 via Npgsql | Relational storage for users, assets, and health logs |
| **Authentication** | JWT Bearer & BCrypt.Net | Token issuance, claim management, secure hashing |
| **Background Runner** | `BackgroundService` | Asynchronous 30s concurrent HTTP health pollers |
| **Documentation** | Swagger / OpenAPI 3.0 | API explorer with Bearer authorization support |

### **Frontend (`cloudpulse-ui`)**
| Component | Technology | Description |
| :--- | :--- | :--- |
| **Framework** | Vue 3 (`<script setup lang="ts">`) | Modern Reactive Composition API |
| **Build Tool** | Vite 8 + TypeScript | Fast HMR and type safety |
| **State Management**| Pinia | Centralized stores for Auth, Assets, and Telemetry |
| **Styling** | Tailwind CSS 3.4 + Reka UI | Custom design system with glassmorphism & dark mode |
| **Icons** | Lucide Vue | Crisp modern vector iconography |
| **HTTP Client** | Axios | Request/response interceptors & global auth handling |

---

## 🚀 Getting Started

### **Prerequisites**
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18+ or v20+)](https://nodejs.org/) & `npm`
- [PostgreSQL](https://www.postgresql.org/) running locally or in Docker on port `5432`

---

### **1. Backend Setup (`CloudPulse.Api`)**

```bash
# Navigate to backend directory
cd CloudPulse.Api

# Restore dependencies
dotnet restore

# Verify appsettings.json database connection
# Host=localhost;Port=5432;Database=cloudpulse_db;Username=postgres;Password=postgres

# Run the API server
dotnet run --urls "http://localhost:5000"
```

> **Note:** The backend automatically provisions the `cloudpulse_db` database and its tables (`Users`, `CloudAssets`, `AssetHealthLogs`, `PaymentRecords`) on startup via `EnsureCreatedAsync()`.

* **API URL:** `http://localhost:5000`
* **Swagger Explorer:** `http://localhost:5000/swagger`

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

* **Frontend Dashboard:** `http://localhost:5173`

---

## 📡 API Reference

### 🔐 **Authentication (`/api/v1/auth`)**
| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Register new user with email, password & phone | ❌ No |
| `POST` | `/api/v1/auth/login` | Authenticate with email & password | ❌ No |
| `POST` | `/api/v1/auth/google` | Authenticate / provision user via Google ID Token | ❌ No |
| `POST` | `/api/v1/auth/phone/send-otp` | Dispatch 6-digit phone verification OTP | ❌ No |
| `POST` | `/api/v1/auth/phone/verify-otp`| Validate phone OTP and authenticate | ❌ No |

### ☁️ **Cloud Assets (`/api/v1/assets`)**
| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/assets?env={Production}&type={API}` | Retrieve caller's registered assets | 🔒 Bearer JWT |
| `POST` | `/api/v1/assets` | Register a new asset (enforces plan quotas) | 🔒 Bearer JWT |
| `GET` | `/api/v1/assets/{id}` | Fetch asset detail by ID | 🔒 Bearer JWT |
| `DELETE` | `/api/v1/assets/{id}` | Remove asset and cascade delete logs | 🔒 Bearer JWT |
| `POST` | `/api/v1/assets/{id}/ping` | Trigger an immediate out-of-band health check | 🔒 Bearer JWT |

### 📈 **Metrics & Telemetry (`/api/v1/metrics`)**
| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/metrics/dashboard` | Aggregated uptime %, total nodes, and status counts | 🔒 Bearer JWT |
| `GET` | `/api/v1/metrics/{id}/history`| Last 50 health checks for charting latency | 🔒 Bearer JWT |

### 💳 **Billing & Payments (`/api/v1/payments`)**
| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/payments/create-order` | Generate Razorpay order for tier upgrade | 🔒 Bearer JWT |
| `POST` | `/api/v1/payments/verify` | Verify HMAC-SHA256 signature and upgrade to Pro | 🔒 Bearer JWT |

---

## 🗄️ Database Schema

```
Users
├── Id (Guid, PK)
├── Email (string, Unique Index)
├── PhoneNumber (string, Nullable)
├── PasswordHash (string, Nullable)
├── GoogleSubjectId (string, Indexed)
├── Role ("Admin" | "Engineer")
├── SubscriptionTier ("Free" | "Pro")
└── CreatedAt (DateTime UTC)

CloudAssets
├── Id (Guid, PK)
├── UserId (Guid, FK -> Users.Id, Restrict)
├── Name (string, MaxLength 100)
├── TargetUrl (string)
├── ResourceType ("API" | "Database" | "VM" | "Worker")
├── Environment ("Production" | "Staging" | "Development")
├── CurrentStatus ("Healthy" | "Degraded" | "Down" | "Unknown")
├── LastLatencyMs (int)
├── LastCheckedAt (DateTime UTC, Nullable)
├── CheckIntervalSeconds (int)
└── IsActive (bool)

AssetHealthLogs
├── Id (bigint, PK AutoIncrement)
├── CloudAssetId (Guid, FK -> CloudAssets.Id, Cascade)
├── HttpStatusCode (int)
├── LatencyMs (int)
├── IsSuccessful (bool)
├── ErrorMessage (string, Nullable)
└── CheckedAt (DateTime UTC, Composite Indexed with CloudAssetId)

PaymentRecords
├── Id (Guid, PK)
├── UserId (Guid, FK -> Users.Id, Restrict)
├── RazorpayOrderId (string)
├── RazorpayPaymentId (string, Nullable)
├── RazorpaySignature (string, Nullable)
├── Amount (decimal)
├── Currency ("INR")
├── Status ("Created" | "Captured" | "Failed")
├── TargetTier ("Pro")
└── CreatedAt (DateTime UTC)
```

---

## 📂 Project Structure

```
Cloud-Pulse/
├── CloudPulse.Api/                 # ASP.NET Core 8 Backend
│   ├── Controllers/               # REST API Controllers (Auth, Asset, Metrics, Payment)
│   ├── Data/                      # AppDbContext & Fluent API configurations
│   ├── Dtos/                      # Type-safe Data Transfer Objects & validations
│   ├── Models/                    # Entity domain models (User, Asset, HealthLog, Payment)
│   ├── Services/                  # TokenService, OtpService, HealthMonitoringWorker
│   ├── appsettings.json           # Connection strings & JWT security settings
│   ├── Program.cs                 # Dependency injection, CORS, middleware pipeline
│   └── CloudPulse.Api.csproj      # .NET 8 Project Manifest & Nuget packages
│
├── cloudpulse-ui/                 # Vue 3 Frontend Single Page Application
│   ├── src/
│   │   ├── api/                   # Central Axios client with interceptors
│   │   ├── components/            # Shadcn UI primitives & custom layout components
│   │   │   ├── dashboard/         # AddAssetModal & dashboard widgets
│   │   │   ├── layout/            # Glassmorphism Navbar & theme switch
│   │   │   └── ui/                # Button, Card, Badge, Table, Dialog, Dropdown
│   │   ├── composables/           # Google Identity Services SDK composable
│   │   ├── router/                # Navigation guards and route definitions
│   │   ├── stores/                # Pinia state stores (auth, assets)
│   │   ├── views/                 # AuthView, DashboardView, AssetDetailView, BillingView
│   │   ├── style.css              # Dual-theme design tokens & animations
│   │   ├── App.vue                # Root application layout
│   │   └── main.ts                # Application bootstrapping
│   ├── vite.config.ts             # Vite configuration with API proxy
│   └── package.json               # Frontend dependencies & scripts
│
├── .gitignore                     # Comprehensive repository ignore rules
└── Readme.md                      # Project documentation
```

---

## 🔒 Security & Best Practices

- **Zero-Plaintext Passwords**: Industry-standard BCrypt hashing with cryptographic salt rounds.
- **Stateless Bearer Tokens**: HMAC-SHA256 signed JWTs with expiration boundaries and strict audience/issuer checks.
- **Timing-Safe HMAC Verification**: Constant-time byte comparison (`FixedTimeEquals`) to mitigate side-channel timing attacks during webhook/signature validation.
- **Resilient Background Polling**: Isolated try/catch scopes with 5-second `CancellationTokenSource` timeouts to prevent hung socket connections from exhausting the thread pool.
- **CORS Protection**: Origin-restricted access policies preventing unauthorized cross-origin tampering.

---

## 📄 License

This project is licensed under the **MIT License**. Feel free to use, modify, and distribute it for personal and commercial projects.
