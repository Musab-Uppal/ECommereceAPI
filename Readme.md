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
