# Wholesale Retail Store

A comprehensive ASP.NET Core MVC application that demonstrates wholesale and retail pricing logic with automatic discount calculations based on customer types.

## 🏗️ Architecture Overview

This application implements a **3-tier architecture** following clean architecture principles:

### **Presentation Layer**
- ASP.NET Core MVC with Razor views
- Bootstrap 5 for responsive UI
- RESTful API endpoints with Swagger documentation

### **Business Logic Layer**
- **PricingService**: Handles automatic price calculations based on customer type
- **Controllers**: Process requests and orchestrate business operations
- **DTOs**: Data transfer objects for API communication

### **Data Access Layer**
- Entity Framework Core with SQL Server
- Repository pattern implemented through DbContext
- Database designed in **3rd Normal Form** (3NF)

## 📊 Database Schema

The application uses a normalized database design with the following entities:

### **Products**
```sql
- Id (PK)
- Name (Required, Max 100 chars)
- Description (Max 500 chars)
- Stock (Required, Default 0)
- BasePrice (Required, Decimal 18,2)
```

### **Customers**
```sql
- Id (PK)
- Name (Required, Max 100 chars)
- Email (Required, Unique, Max 255 chars)
- CustomerType (Required: Retail/Wholesale)
```

### **Orders**
```sql
- Id (PK)
- CustomerId (FK to Customers)
- CreatedAt (Required, Default UTC)
- TotalAmount (Required, Decimal 18,2)
- Status (Required: Pending/Confirmed/Shipped/Cancelled)
```

### **OrderItems**
```sql
- Id (PK)
- OrderId (FK to Orders)
- ProductId (FK to Products)
- Quantity (Required, Min 1)
- UnitPrice (Required, Decimal 18,2)
```

### **PricingRules**
```sql
- Id (PK)
- CustomerType (Required)
- DiscountPercentage (Required, 0-100%)
- MinimumOrderAmount (Optional, Decimal 18,2)
- IsActive (Required, Default true)
- Description (Max 200 chars)
```

## 💰 Pricing Logic

### **Retail Customers**
- Pay **full base price**
- No discounts applied
- Example: Product $99.99 → Customer pays $99.99

### **Wholesale Customers**
- Receive **automatic discounts** based on pricing rules
- **Standard Discount**: 10% off all products
- **Volume Discount**: 15% off orders over $500
- Example: Product $99.99 → Customer pays $89.99 (10% discount)

### **Price Calculation Flow**
1. Identify customer type from order
2. Find applicable pricing rules (active, matching customer type)
3. Apply best discount (highest percentage)
4. Check minimum order amount for volume discounts
5. Calculate final price: `BasePrice × (1 - DiscountPercentage/100)`

## 🚀 Quick Start

### **Prerequisites**
- .NET 8.0 SDK
- SQL Server Express or LocalDB (included with Visual Studio)
- Web browser

### **Setup Instructions**

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd WholesaleRetailStore
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Open your browser**
   - Main application: `https://localhost:7052` or `http://localhost:5076`
   - Swagger API docs: `https://localhost:7052/swagger`

### **Sample Data Included**

The application comes pre-loaded with:
- **6 Products**: Wireless Headphones, Smart Watch, Laptop Stand, USB-C Cable, Bluetooth Speaker, Webcam Cover
- **4 Customers**: 2 Retail, 2 Wholesale
- **2 Pricing Rules**: 10% wholesale discount, 15% volume discount for orders over $500

## 🛠️ API Usage Examples

### **Base URL**
```
https://localhost:7052/api
```

### **Customers API**

#### **Get All Customers**
```http
GET /api/customers
```

#### **Create New Customer**
```http
POST /api/customers
Content-Type: application/json

{
  "name": "New Customer Corp",
  "email": "contact@newcustomer.com",
  "customerType": "Wholesale"
}
```

#### **Get Customer by ID**
```http
GET /api/customers/1
```

### **Products API**

#### **Get All Products**
```http
GET /api/products
```

### **Pricing API**

#### **Generate Quote**
```http
POST /api/pricing/quote
Content-Type: application/json

{
  "customerId": 3,
  "items": [
    {
      "productId": 1,
      "quantity": 2
    },
    {
      "productId": 2,
      "quantity": 1
    }
  ]
}
```

#### **Get Product Prices for Customer**
```http
GET /api/pricing/products/3
```

#### **Compare Retail vs Wholesale Pricing**
```http
POST /api/pricing/compare
Content-Type: application/json

{
  "productIds": [1, 2, 3]
}
```

### **Orders API**

#### **Get All Orders**
```http
GET /api/orders
```

#### **Create Order**
```http
POST /api/orders
Content-Type: application/json

{
  "customerId": 3,
  "items": [
    {
      "productId": 1,
      "quantity": 1
    }
  ]
}
```

## 🧪 Testing the Application

### **Web Interface Testing**

1. **Browse Products**: Navigate to Products → View catalog
2. **Create Customers**: Add both Retail and Wholesale customers
3. **Generate Quotes**: Use Pricing → Generate Quote to see automatic discounts
4. **Place Orders**: Convert quotes to orders (stock automatically reduced)

### **API Testing with Swagger**
1. Open `/swagger` in your browser
2. Test customer creation with different types
3. Generate quotes to see pricing calculations
4. Create orders and observe stock changes

### **Postman Collection**
```json
{
  "info": {
    "name": "Wholesale Retail Store API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Create Retail Customer",
      "request": {
        "method": "POST",
        "url": "https://localhost:7052/api/customers",
        "body": {
          "mode": "raw",
          "raw": "{\"name\":\"Test Retail\",\"email\":\"retail@test.com\",\"customerType\":\"Retail\"}"
        }
      }
    },
    {
      "name": "Create Wholesale Customer",
      "request": {
        "method": "POST",
        "url": "https://localhost:7052/api/customers",
        "body": {
          "mode": "raw",
          "raw": "{\"name\":\"Test Wholesale\",\"email\":\"wholesale@test.com\",\"customerType\":\"Wholesale\"}"
        }
      }
    },
    {
      "name": "Generate Quote",
      "request": {
        "method": "POST",
        "url": "https://localhost:7052/api/pricing/quote",
        "body": {
          "mode": "raw",
          "raw": "{\"customerId\":2,\"items\":[{\"productId\":1,\"quantity\":1}]}"
        }
      }
    }
  ]
}
```

## 🔧 Configuration

### **Database Connection**
Located in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=WholesaleRetailStoreDb;Trusted_Connection=True;"
  }
}
```

### **Development Settings**
Located in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 📁 Project Structure

```
WholesaleRetailStore/
├── Controllers/           # MVC Controllers + API endpoints
│   ├── CustomerController.cs
│   ├── OrderController.cs
│   ├── PricingController.cs
│   └── ProductsController.cs
├── Data/                  # Database context and configuration
│   └── AppDbContext.cs
├── Models/                # Entity models and DTOs
│   ├── Customer.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── PricingRule.cs
│   ├── Product.cs
│   └── DTOs/             # Data Transfer Objects
├── Services/             # Business logic services
│   └── PricingService.cs
├── Views/                # Razor views
│   ├── Customers/
│   ├── Orders/
│   ├── Pricing/
│   └── Products/
├── wwwroot/             # Static assets (CSS, JS, Bootstrap)
└── Program.cs           # Application startup
```

## 🎯 Key Features Implemented

✅ **5 Normalized Database Tables** with proper relationships
✅ **Automatic Pricing Logic** based on customer type
✅ **RESTful API Endpoints** with comprehensive functionality
✅ **Sample Data Pre-loaded** for immediate testing
✅ **XML Documentation** on all classes and methods
✅ **Bootstrap UI** with responsive design
✅ **Stock Management** with automatic reduction on orders
✅ **Order Workflow** from quote to confirmed order
✅ **Swagger Documentation** for API testing
✅ **LocalDB Support** (free, no additional setup required)

## 🔒 Business Rules Enforced

1. **Stock Validation**: Orders cannot exceed available inventory
2. **Email Uniqueness**: Customer emails must be unique
3. **Price Calculation**: Automatic application of best available discount
4. **Data Integrity**: Foreign key constraints prevent orphaned records
5. **Order Status Flow**: Proper state management for orders

## 🚀 Deployment

### **Production Deployment**
1. Update connection string in `appsettings.json`
2. Run migrations: `dotnet ef database update`
3. Publish: `dotnet publish -c Release`
4. Deploy to IIS, Azure, or Docker

### **Docker Support**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY published/ .
EXPOSE 80
ENTRYPOINT ["dotnet", "WholesaleRetailStore.dll"]
```

## 📈 Performance Considerations

- **Database Indexing**: Optimized queries with proper indexing
- **Efficient Queries**: Uses EF Core Include/ThenInclude for eager loading
- **Pagination Ready**: API endpoints prepared for pagination
- **Connection Pooling**: Default SQL Server connection pooling enabled

## 🐛 Troubleshooting

### **Database Issues**
```bash
# Reset database
dotnet ef database drop
dotnet ef database update
```

### **Port Conflicts**
Update `launchSettings.json` if ports 5076/7052 are in use:
```json
{
  "applicationUrl": "http://localhost:5077;https://localhost:7053"
}
```

### **Migration Errors**
```bash
# Revert last migration
dotnet ef migrations remove
# Recreate migration
dotnet ef migrations add NewMigration
```

## 📞 Support

For questions or issues:
1. Check the Swagger documentation at `/swagger`
2. Review the code comments for implementation details
3. Test with the provided sample data
4. Examine the browser developer tools for client-side issues

---

**Built with**: ASP.NET Core 8.0, Entity Framework Core, Bootstrap 5, SQL Server
**Architecture**: MVC, Clean Architecture, 3NF Database Design
