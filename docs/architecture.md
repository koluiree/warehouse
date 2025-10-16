# Warehouse Management System Architecture

## Overview

This document describes the architecture of a C# ASP.NET MVC application for warehouse management, based on the provided class and use case diagrams.

## Technology Stack

-   **Framework**: ASP.NET Core MVC (.NET 6+)
-   **Database**: Entity Framework Core with SQL Server
-   **Authentication**: ASP.NET Core Identity
-   **Architecture Pattern**: MVC with Repository Pattern and Dependency Injection
-   **Frontend**: Razor Views with Bootstrap/CSS

## Project Structure

```
WarehouseManagement/
├── Controllers/           # MVC Controllers
│   ├── AccountController.cs
│   ├── ItemController.cs
│   ├── RequestController.cs
│   ├── WarehouseController.cs
│   ├── AdminController.cs
│   └── DirectorController.cs
├── Models/               # Data Models and ViewModels
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Item.cs
│   │   ├── ItemRequest.cs
│   │   ├── Warehouse.cs
│   │   ├── Section.cs
│   │   ├── Company.cs
│   │   └── Category.cs
│   ├── ViewModels/
│   │   ├── LoginViewModel.cs
│   │   ├── ItemViewModel.cs
│   │   ├── RequestViewModel.cs
│   │   └── UserManagementViewModel.cs
│   └── Enums/
│       └── RequestStatus.cs
├── Views/                # Razor Views
│   ├── Account/
│   ├── Item/
│   ├── Request/
│   ├── Warehouse/
│   ├── Admin/
│   └── Director/
├── Services/             # Business Logic Layer
│   ├── Interfaces/
│   │   ├── IUserService.cs
│   │   ├── IItemService.cs
│   │   ├── IRequestService.cs
│   │   ├── IWarehouseService.cs
│   │   ├── IAdminService.cs
│   │   └── IDirectorService.cs
│   └── Implementations/
│       ├── UserService.cs
│       ├── ItemService.cs
│       ├── RequestService.cs
│       ├── WarehouseService.cs
│       ├── AdminService.cs
│       └── DirectorService.cs
├── Data/                 # Data Access Layer
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── IUserRepository.cs
│   │   ├── IItemRepository.cs
│   │   └── ...
│   ├── Repositories/
│   │   ├── Repository.cs
│   │   ├── UserRepository.cs
│   │   ├── ItemRepository.cs
│   │   └── ...
│   └── ApplicationDbContext.cs
├── wwwroot/             # Static files
├── appsettings.json     # Configuration
├── Program.cs           # Application entry point
└── Startup.cs           # Services configuration
```

## Architecture Layers

### 1. Presentation Layer (Controllers & Views)

-   **Controllers**: Handle HTTP requests, coordinate between services and views
-   **Views**: Razor templates for rendering UI
-   **ViewModels**: Data transfer objects for views

### 2. Business Logic Layer (Services)

-   **Services**: Contain business rules and logic
-   **Interfaces**: Define contracts for services
-   **Dependency Injection**: Services are injected into controllers

### 3. Data Access Layer (Repositories)

-   **Repositories**: Abstract data access operations
-   **Entity Framework**: ORM for database operations
-   **DbContext**: Manages database connections and entity sets

### 4. Domain Layer (Models)

-   **Entities**: Database models
-   **Enums**: Enumeration types
-   **DTOs**: Data transfer objects

## Key Components Mapping

### User Roles and Controllers

-   **UnauthorizedUser**: AccountController (Login)
-   **AuthorizedUser**: Inherits base functionality
-   **WarehouseManager**: WarehouseController + ItemController
-   **Admin**: AdminController
-   **Director**: DirectorController (inherits from Admin and WarehouseManager)

### Interfaces from Class Diagram

-   **Admin**: IAdminService (RegisterUser, DeleteUser, EditUser)
-   **IWarehouseManager**: IWarehouseService (ProcessRequest, AddItem, DeleteItem, TransferItem, ReplenishItem, WriteOffItem)
-   **IDirector**: IDirectorService (ViewCompanyInfo, ViewWorkersList)

### Entities

-   **Company**: Root entity containing users and warehouses
-   **User**: Base user entity with roles
-   **Item**: Warehouse items with SN, quantity, etc.
-   **ItemRequest**: Request entity with status tracking
-   **Warehouse**: Contains sections
-   **Section**: Contains items
-   **Category**: Item categorization

## Database Design

### Entity Relationships

-   Company 1:N Users
-   Company 1:N Warehouses
-   Warehouse 1:N Sections
-   Section 1:N Items
-   User 1:N ItemRequests
-   Item 1:N ItemRequests
-   Item N:1 Category

### Key Tables

-   Companies
-   Users (with ASP.NET Identity tables)
-   Items
-   ItemRequests
-   Warehouses
-   Sections
-   Categories

## Security & Authorization

-   **Authentication**: ASP.NET Core Identity
-   **Authorization**: Role-based access control
-   **Roles**: UnauthorizedUser, AuthorizedUser, WarehouseManager, Admin, Director
-   **Policies**: Custom authorization policies for specific actions

## Dependency Injection

-   Services registered in Program.cs
-   Scoped lifetime for most services
-   Singleton for shared resources like DbContext

## Configuration

-   **appsettings.json**: Database connection, logging, etc.
-   **Environment-specific configs**: Development, Production settings

This architecture follows SOLID principles, separates concerns, and provides a scalable foundation for the warehouse management system.
