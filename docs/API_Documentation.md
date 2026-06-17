# ECommereceAPI — API & Frontend Documentation

- High-level overview
- Backend: architecture, controllers, DTOs, endpoints, error handling, security
- Frontend: architecture, services, state management, auth flow, UI conventions
- Running locally (dev) and tests
- Deployment and production notes
- Troubleshooting and common issues
- Conventions and design decisions

---

## High-level overview

This repository contains two cooperating applications:

- Backend: ASP.NET Core Web API (C#) exposing REST endpoints at `/api/*`.
- Frontend: React (Vite) single-page application consuming the API.

High-level design goals

- Clear separation of concerns (Controllers → Service layer → DbContext)
- Defensive front-end mapping of backend shapes to prevent runtime exceptions
- Well-defined REST endpoints with standard HTTP semantics and pagination
- Token-based authentication (JWT) persisted for SPA usage

---

## Backend

### Tech stack

- .NET (ASP.NET Core Web API)
- EF Core (migrations and DbContext)
- Structured Service interfaces (e.g. `IUserService`)

### Important folders

- `Controllers/` — API controllers (e.g., `UserController`, `OrderController`, `ProductController`).
- `Services/Implementation` & `Services/Interfaces` — business logic and contracts.
- `Data/` — `ApplicationDbContext`, seed data and EF migrations.
- `Models/` — domain entities.
- `Migrations/` — EF schema migrations.

### Key endpoints (summary)

All endpoints are relative to the API base (`/api`). This section lists the most relevant endpoints and expected shapes.

- `POST /api/user/register` — register new user
  - Body: `{ email, password, firstName, lastName, phone, address }`
  - Success: 201 Created, returns `AuthResponse` (token + user payload)

- `POST /api/user/login` — authenticate user
  - Body: `{ email, password }`
  - Success: 200 OK, returns `AuthResponse` (token + user payload)

- `GET /api/user/profile?userId={id}` — fetch user profile
  - Query: `userId` (required)
  - Success: 200 OK, returns `UserProfileDto` (`userId`, `email`, `firstName`, `lastName`, `phone`, `address`, `role`, `createdAt`, `updatedAt`)

- `PUT /api/user/profile?userId={id}` — update user profile
  - Query: `userId` (required)
  - Body: `{ firstName, lastName, phone, address }` (fields optional but validated)
  - Success: 200 OK, returns updated `UserProfileDto`

- `PUT /api/user/change-password?userId={id}` — change password

- `GET /api/order/my-orders?userId={id}&pageNumber=1&pageSize=10` — user's orders
  - Returns a paginated response with order DTOs

- `POST /api/order/create` — create order

- `GET /api/product` & `GET /api/product/{id}` — product listing and single product

For the exact public surface, see the controllers under `src/Ecommerce/Controllers`.

### Request/response contracts

The backend defines DTOs in `Services/Interfaces` (e.g., `UserProfileDto`, `UpdateUserProfileDto`). When the frontend receives a response, services should normalize the DTOs into the frontend `types` to avoid runtime errors caused by naming differences (e.g., `userId` vs `id`, `firstName` + `lastName` vs `name`).

### Error handling

- 200 OK — successful response
- 201 Created — resource created
- 400 Bad Request — validation or missing parameters
- 401 Unauthorized — invalid/expired token
- 403 Forbidden — insufficient permissions
- 404 Not Found — resource not found
- 500 Internal Server Error — unexpected server error

Controllers typically return objects with a `message` field describing the error for client display.

### Security

- Use HTTPS in production and enable transport security.
- JWT tokens are used for authentication; they should be stored securely (consider using cookies in stricter security models).
- Protect admin endpoints with role-based authorization (`[Authorize(Roles = "Admin")]`).

---

## Frontend

### Tech stack

- React + TypeScript (Vite)
- Zustand (state)
- Axios (HTTP client)
- Tailwind-like utilities + custom CSS tokens

### Code layout (notable files)

- `src/pages/` — page-level views (Home, Shop, Orders, Profile, Cart, Register, Login)
- `src/services/` — API clients and business-facing HTTP helpers (`api.ts`, `authService.ts`, `orderService.ts`, `productService.ts`)
- `src/store/` — Zustand stores (e.g. `authStore.ts`)
- `src/types/` — TypeScript types and interfaces
- `src/index.css` — global tokens, utilities and animations

### HTTP client (`src/services/api.ts`)

- Single Axios instance with:
  - Base URL: `VITE_API_URL` (if present) or runtime-probed `https://localhost:5211` / `http://localhost:5211`.
  - Timeout: 10s
  - Request interceptor: attaches `Authorization: Bearer {token}` from `localStorage.token`.
  - Response interceptor: on `401` triggers `useAuthStore.getState().logout()` and redirects to `/login`.
  - `handleApiError(error)` normalizes Axios errors into friendly messages.

### Auth service (`src/services/authService.ts`)

Key methods:

- `login(credentials)` — `POST /user/login` — normalizes backend user shape to frontend `UserAuthDto` and returns `AuthResponse`.
- `register(data)` — `POST /user/register`.
- `logout()` — `POST /user/logout`.
- `refreshToken()` — `POST /user/refresh`.
- `getUser()` — reads `localStorage.user`.
- `updateProfile(userId, data)` — `PUT /user/profile?userId={id}` — added to persist profile changes server-side and obtain canonical user data.

Notes about normalization: backend DTO naming may vary; the service layer maps server fields to the frontend `types` as early as possible.

### State management (`src/store/authStore.ts`)

- `user` (User | null), `token` (string | null), `isAuthenticated` (boolean), and helpers.
- `setAuth(userAuthDto, token)` — builds a `User` object, persists `token` and `user` to `localStorage`, and updates the store.
- `initializeAuth()` — loads `token` + `user` from `localStorage` at app startup.
- `updateUser(partial)` — merges server canonical fields into the current `user` and persists to `localStorage` (added to ensure server is authoritative).

### Common frontend pitfalls & best practices

- Do all DTO mapping in `*Service` files so pages consume stable frontend `types`.
- Persist and read the authoritative user object from `localStorage` only after receiving confirmation from the server (don't rely on simulated local-only updates).
- Use `handleApiError` for consistent user-facing error messages.

### Styling & animation

- Global tokens and utilities live in `src/index.css`. Use `bg-card-bg`, `border-card-border`, and `.card-dark-shadow` for consistent look & feel.
- Animations: `.page-fade` and `.page-fade-up` with inline `animationDelay` on repeated elements to create staggered in-view entrance effects. Respect `prefers-reduced-motion`.

---

## Running locally (development)

### Backend

```bash
cd src/Ecommerce
dotnet run
```

Default launch URL (local dev) is typically `https://localhost:5211`. Confirm the exact URL when running.

### Frontend

```bash
cd src/frontend
npm install      # first-time only
npm run dev
```

Environment:

- `VITE_API_URL` — optional. If set, the frontend uses this base URL. If omitted, the frontend probes common localhost HTTP(s) addresses.

---

## Tests

### Backend unit tests

```bash
cd src/ECommerceTest
dotnet test
```

### Frontend tests

- If present in `package.json`, run `npm test` from `src/frontend`.

---

## Deployment & production notes

- Build frontend for production and host behind CDN or static hosting.

```bash
cd src/frontend
npm run build
```

- Deploy backend to any HTTPS-capable host (Azure, AWS, GCP). Ensure the environment variables and connection strings are configured for the target environment.
- Configure CORS to allow production frontend origin only.
- Use proper logging, monitoring (Application Insights, Prometheus), and alerting.

---

## Troubleshooting & common issues

- Profile updates not persisting across sessions
  - Symptom: edits show immediately, but after logout/login the old profile is fetched.
  - Root cause: client-side-only update (localStorage manipulation) without calling `PUT /api/user/profile` (server remained unchanged).
  - Fix: call `authService.updateProfile(userId, updateDto)`, then persist the returned `UserProfileDto` via `useAuthStore.updateUser(...)` (this repository now implements this flow).

- 401 Unauthorized
  - Reason: token expired or missing. The `api` response interceptor automatically logs out and redirects to `/login` on `401`.

- Network / probe issues in dev
  - If the frontend cannot detect or probe your backend, explicitly set `VITE_API_URL` to the backend base URL.

---

## Conventions & design decisions

- Centralize network shape mapping in services to reduce brittle UI code.
- Keep global state minimal and immutable-like (replace whole `user` object on updates).
- Persist token+user to `localStorage` for SPA simplicity; consider cookie-based storage for stricter security.
- Prefer explicit API endpoints over implicit client-side assumptions.

---

## Next steps and recommendations

- Publish OpenAPI (Swagger) for the backend so the frontend can rely on a machine-readable contract.
- Add integration tests that exercise critical flows (register/login/profile update, order lifecycle).
- Harden auth flows (refresh token rotation, secure cookie usage) before production.

If you want, I can also:

- Add cURL / Postman examples for the most important endpoints,
- Add a Swagger/OpenAPI generator endpoint to the backend,
- Run the app locally and demonstrate a profile update trace.
