# ECommerce API

Production-ready e-commerce API built with .NET 8 and C#.

## Live Demo
**API:** https://your-app.azurewebsites.net  
**Swagger Docs:** https://your-app.azurewebsites.net (Swagger UI at root)

## Quick Start

### Register & Test
1. Visit live API URL above
2. POST `/api/user/register` with:
```json
{
  "email": "testuser@example.com",
  "password": "TestPass123",
  "firstName": "Test",
  "lastName": "User",
  "phone": "03001234567",
  "address": "Lahore, Pakistan"
}
```
3. Copy JWT token from response
4. Click "Authorize" button in Swagger
5. Test all endpoints

### Or Use Admin Account
- Email: `admin@ecommerce.pk`
- Password: `admin123`

## Features

### Authentication & Security
- User registration & login with JWT tokens
- BCrypt password hashing (workFactor: 12)
- Role-based authorization (Admin/Customer)
- Protected endpoints

### Product Management
- CRUD operations for products
- Category filtering & pagination
- Stock tracking

### Order Management
- Create orders with multiple items
- Automatic stock deduction
- Order status tracking: Pending → Shipped → Delivered
- Order cancellation with stock restoration
- Revenue statistics (admin)

### User Management
- Profile management
- Password change
- Admin user management

### Product Reviews
- Leave/edit/delete reviews
- Rating system (1-5 stars)
- View reviews by product

## Technology Stack

**Backend:** ASP.NET Core 8 (.NET 8)  
**Language:** C#  
**Database:** SQL Server  
**ORM:** Entity Framework Core (Code-First)  
**Authentication:** JWT Bearer tokens  
**Password Security:** BCrypt.Net  
**API Docs:** Swagger/OpenAPI  
**Deployment:** Azure App Service  
**CI/CD:** GitHub Actions

## Architecture

### Repository Pattern
- Separates data access from business logic
- Easy to test (mockable repositories)
- Easy to switch databases

### Service Layer
- Contains all business logic
- Validation & error handling
- DTOs for API contracts

### Folder Structure
ECommerce/
├── Controllers/                    # HTTP endpoints
├── Models/                         # Database entities
├── Data/                          # DbContext & migrations
├── Repositories/
│   ├── Interfaces/                # Repository contracts
│   └── Implementation/            # Repository implementations
├── Services/
│   ├── Interfaces/                # Service contracts
│   └── Implementation/            # Service implementations
└── Settings/                      # Configuration

## API Endpoints

### Authentication (Public)

POST   /api/user/register           Create new account
POST   /api/user/login              Login & get JWT token

### Products (Public)

GET    /api/product                 List all products (paginated)
GET    /api/product/{id}            Get product details
GET    /api/product/category/{id}   Get products by category
POST   /api/product                 Create product (admin)
PUT    /api/product/{id}            Update product (admin)
DELETE /api/product/{id}            Delete product (admin)

### Orders (Authenticated)

POST   /api/order                   Create order
GET    /api/order/my-orders         Get your orders
GET    /api/order/{id}              Get order details
DELETE /api/order/{id}              Cancel order
GET    /api/order                   List all orders (admin)
PUT    /api/order/{id}/status       Update order status (admin)
GET    /api/order/stats/revenue     Revenue stats (admin)

### User Profile

GET    /api/user/profile            Get own profile
PUT    /api/user/profile            Update profile
PUT    /api/user/change-password    Change password
GET    /api/user                    List users (admin)
GET    /api/user/{id}               Get user (admin)
PUT    /api/user/{id}               Update user (admin)
DELETE /api/user/{id}               Delete user (admin)
PUT    /api/user/{id}/role          Change user role (admin)

### Reviews

GET    /api/review/{productId}      Get product reviews
POST   /api/review                  Create review
DELETE /api/review                  Delete review

## How to Use (For Recruiters/Testers)

1. **Access the API:** [Live URL](https://your-app.azurewebsites.net)
2. **See Swagger Docs:** Swagger UI loads at root URL
3. **Register a user:** POST /api/user/register
4. **Get JWT token:** From register or login response
5. **Authorize in Swagger:** Click green "Authorize" button, paste token
6. **Test endpoints:** All endpoints now available with authentication

## Key Learning Outcomes

Through building this API, I learned:
- ✅ ASP.NET Core & C# fundamentals
- ✅ Entity Framework Core (Code-First approach)
- ✅ Repository Pattern & Service Layer architecture
- ✅ JWT authentication & BCrypt password hashing
- ✅ RESTful API design principles
- ✅ Async/await & dependency injection
- ✅ Error handling & input validation
- ✅ Git & GitHub workflow
- ✅ Azure deployment & CI/CD
- ✅ API documentation with Swagger

## Setup Instructions (For Local Development)

### Prerequisites
- .NET 8 SDK
- SQL Server (or SQL Server Express)
- Visual Studio 2022 / VS Code

### Steps
1. Clone repository:
```bash
git clone https://github.com/YourUsername/ECommerceAPI.git
cd ECommerceAPI
```

2. Update connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=EcommerceApiDb;Trusted_Connection=True;"
}
```

3. Run migrations:
```bash
dotnet ef database update
```

4. Start API:
```bash
dotnet run
```

5. Open `https://localhost:7000` → Swagger UI opens

## Test Data

### Pre-seeded Data:
- Admin account: `admin@ecommerce.pk` / `admin123`
- 3 customer accounts
- 5 product categories
- 10 sample products
- 3 sample orders

### Create Your Own:
- Register via `/api/user/register`
- Create products (admin only)
- Create orders
- Leave reviews

## Production Features Implemented

✅ Input validation on all endpoints  
✅ Proper error handling & HTTP status codes  
✅ Logging throughout application  
✅ Pagination for list endpoints  
✅ CORS properly configured  
✅ No sensitive data in responses  
✅ Password hashing (not plaintext)  
✅ Token-based security  
✅ Role-based authorization  
✅ Database relationships & constraints  
