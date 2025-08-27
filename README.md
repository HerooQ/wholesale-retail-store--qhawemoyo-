# Wholesale Retail Store

A comprehensive ASP.NET Core application with React frontend that demonstrates wholesale and retail pricing logic with automatic discount calculations based on customer types. Features AI-enhanced intelligent search capabilities and modern single-page application architecture.

## Architecture Overview

This application implements a **3-tier architecture** following clean architecture principles:

### **Presentation Layer**
- **React 19** frontend with TypeScript for modern SPA experience
- **ASP.NET Core MVC** backend serving RESTful API endpoints
- **Tailwind CSS** for responsive and modern UI styling
- **Swagger documentation** for comprehensive API testing

### **Business Logic Layer**
- **PricingService**: Handles automatic price calculations based on customer type
- **IntelligentSearchService**: AI-powered search with synonym matching and fuzzy search
- **Controllers**: Process requests and orchestrate business operations
- **DTOs**: Data transfer objects for API communication

### **Data Access Layer**
- Entity Framework Core with SQL Server
- Repository pattern implemented through DbContext
- Database designed in **3rd Normal Form** (3NF)

## Database Schema

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

## Pricing Logic

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

## Quick Start

### **Prerequisites**
- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 20.19+** - [Download here](https://nodejs.org/) (for React frontend)
- **SQL Server Express or LocalDB** (included with Visual Studio)
- **Web browser** (Chrome, Firefox, Edge, Safari)

### **Complete Setup Instructions**

#### **Step 1: Clone and Setup Backend**

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd WholesaleRetailStore
   ```

2. **Restore .NET packages**
   ```bash
   dotnet restore
   ```

3. **Setup database with sample data**
   ```bash
   dotnet ef database update
   ```
   This creates the database and populates it with sample products, customers, and pricing rules.

#### **Step 2: Setup React Frontend**

1. **Navigate to frontend directory**
   ```bash
   cd frontend
   ```

2. **Install Node.js dependencies**
   ```bash
   npm install
   ```

3. **Build the React application**
   ```bash
   npm run build
   ```

4. **Return to root directory**
   ```bash
   cd ..
   ```

#### **Step 3: Start the Application**

1. **Start the ASP.NET Core server** (serves both API and React app)
   ```bash
   dotnet run
   ```

2. **Open your browser and navigate to:**
   - **Main Application**: `http://localhost:5076`
   - **API Documentation**: `http://localhost:5076/swagger`

### **Development Mode (Optional)**

For frontend development with hot-reload:

1. **Terminal 1 - Start ASP.NET Core API:**
   ```bash
   dotnet run
   ```

2. **Terminal 2 - Start React dev server:**
   ```bash
   cd frontend
   npm run dev
   ```

3. **Access:**
   - **React Dev Server**: `http://localhost:5173` (with hot-reload)
   - **API Server**: `http://localhost:5076/api`

### **Sample Data Included**

The application comes pre-loaded with:
- **6 Products**: Wireless Headphones, Smart Watch, Laptop Stand, USB-C Cable, Bluetooth Speaker, Webcam Cover
- **4 Customers**: 2 Retail (John Smith, Jane Doe), 2 Wholesale (ABC Electronics, Tech Wholesale Inc)
- **2 Pricing Rules**: 10% wholesale discount, 15% volume discount for orders over $500

### **AI-Enhanced Features**

The application includes intelligent search capabilities:

- **Smart Search**: AI-powered product search with synonym matching (e.g., "laptop" finds "Laptop Stand")
- **Typo Tolerance**: Handles spelling mistakes with fuzzy search algorithms
- **Auto-Complete**: Real-time search suggestions as you type
- **Semantic Analysis**: Finds related products using AI understanding
- **Stock-Aware Ranking**: Prioritizes in-stock products in search results

**To enable AI search**: Click the purple sparkles button in the search bar on the main application.

## API Usage Examples

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

##  Testing the Application

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

## Configuration

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

## Project Structure

```
WholesaleRetailStore/
├── Controllers/           # API Controllers for backend services
│   ├── CustomerController.cs
│   ├── IntelligentSearchController.cs  # AI-enhanced search endpoints
│   ├── OrderController.cs
│   ├── PricingController.cs
│   └── ProductsController.cs
├── Services/              # Business logic services
│   ├── PricingService.cs
│   └── IntelligentSearchService.cs     # AI search algorithms
├── Data/                  # Database context and configuration
│   └── AppDbContext.cs
├── Models/                # Entity models and DTOs
│   ├── Customer.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── PricingRule.cs
│   ├── Products.cs
│   └── DTOs/
├── frontend/              # React SPA Frontend
│   ├── src/
│   │   ├── components/    # React components
│   │   │   ├── CustomerSelector.tsx
│   │   │   ├── IntelligentSearchBar.tsx  # AI search component
│   │   │   ├── ProductCatalog.tsx
│   │   │   ├── ShoppingCart.tsx
│   │   │   └── OrderHistory.tsx
│   │   ├── services/      # API client services
│   │   │   └── api.ts
│   │   ├── types/         # TypeScript type definitions
│   │   └── App.tsx        # Main React application
│   ├── package.json       # Node.js dependencies
│   ├── tailwind.config.js # Tailwind CSS configuration
│   └── vite.config.ts     # Vite build configuration
├── Views/                 # Legacy Razor views (for API documentation)
├── wwwroot/              # Static assets served by ASP.NET Core
└── Program.cs            # Application startup and SPA integration
```

## Key Features Implemented

### **Backend Features**
- **5 Normalized Database Tables** with proper relationships (3rd Normal Form)
- **Automatic Pricing Logic** based on customer type with configurable rules
- **RESTful API Endpoints** with comprehensive CRUD functionality
- **AI-Enhanced Search Service** with synonym matching and fuzzy search
- **Stock Management** with automatic inventory reduction on orders
- **XML Documentation** on all classes and methods
- **Swagger Documentation** for comprehensive API testing
- **LocalDB Support** (free, no additional setup required)

### **Frontend Features**
- **Modern React 19** single-page application with TypeScript
- **Tailwind CSS** for responsive and professional styling
- **Intelligent Search Bar** with real-time suggestions and AI capabilities
- **Customer Management** with dropdown selector and new customer creation
- **Shopping Cart** with quantity management and pricing calculations
- **Order History** with detailed order tracking and status management
- **Responsive Design** that works on desktop, tablet, and mobile devices
- **Real-time Updates** with optimistic UI updates and error handling

### **AI-Enhanced Capabilities**
- **Smart Product Search** with synonym recognition and semantic matching
- **Fuzzy Search Algorithm** handles typos and partial matches (70%+ similarity)
- **Auto-Complete Suggestions** with debounced API calls for performance
- **Stock-Aware Ranking** prioritizes available products in search results
- **Related Terms Analysis** suggests semantically similar products
- **Category Classification** for intelligent product grouping

## Business Rules Enforced

1. **Stock Validation**: Orders cannot exceed available inventory
2. **Email Uniqueness**: Customer emails must be unique
3. **Price Calculation**: Automatic application of best available discount
4. **Data Integrity**: Foreign key constraints prevent orphaned records
5. **Order Status Flow**: Proper state management for orders

## Technology Stack

### **Backend Technologies**
- **ASP.NET Core 8.0** - Web framework and API server
- **Entity Framework Core 9.0** - Object-relational mapping (ORM)
- **SQL Server LocalDB** - Database engine (development)
- **Swagger/OpenAPI** - API documentation and testing
- **C# 12** - Programming language with modern features

### **Frontend Technologies**
- **React 19** - Modern JavaScript library for building user interfaces
- **TypeScript 5** - Type-safe JavaScript with enhanced developer experience
- **Vite** - Fast build tool and development server
- **Tailwind CSS v4** - Utility-first CSS framework for styling
- **Axios** - HTTP client for API communication
- **React Router DOM** - Client-side routing for single-page application

### **AI & Search Technologies**
- **Custom AI Algorithms** - Levenshtein distance for fuzzy matching
- **Synonym Dictionary** - Semantic search capabilities
- **Debounced Search** - Performance-optimized real-time suggestions
- **String Similarity** - Advanced text matching algorithms

### **Development Tools**
- **Node.js 20.19+** - JavaScript runtime for frontend tooling
- **npm** - Package manager for frontend dependencies
- **PostCSS** - CSS processing and optimization
- **ESLint** - Code quality and style enforcement

## Deployment

### **Production Deployment**

1. **Build the React frontend**
   ```bash
   cd frontend
   npm install
   npm run build
   cd ..
   ```

2. **Update connection string** in `appsettings.json` for production database

3. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

4. **Publish the application**
   ```bash
   dotnet publish -c Release
   ```

5. **Deploy to your preferred platform** (IIS, Azure App Service, Docker, etc.)

### **Docker Support**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY published/ .
EXPOSE 80
ENTRYPOINT ["dotnet", "WholesaleRetailStore.dll"]
```

## Performance Considerations

### **Backend Optimizations**
- **Database Indexing**: Optimized queries with proper indexing on foreign keys
- **Efficient Queries**: Uses EF Core Include/ThenInclude for eager loading
- **Connection Pooling**: Default SQL Server connection pooling enabled
- **API Response Caching**: Static data cached for improved performance

### **Frontend Optimizations**
- **Code Splitting**: Vite automatically splits code for optimal loading
- **Tree Shaking**: Unused code eliminated in production builds
- **CSS Optimization**: Tailwind CSS purges unused styles automatically
- **Debounced Search**: AI search requests debounced to 300ms for performance
- **Optimistic Updates**: UI updates immediately with server reconciliation
- **Bundle Optimization**: Vite optimizes JavaScript and CSS bundles

### **AI Search Performance**
- **In-Memory Algorithms**: Fuzzy search and synonym matching run in-memory
- **Efficient String Matching**: Levenshtein distance optimized for 70%+ similarity
- **Smart Caching**: Search suggestions cached for repeated queries
- **Fallback Mechanisms**: Graceful degradation when AI features are unavailable

##  Troubleshooting

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

### **Frontend Build Issues**
```bash
# Clear npm cache
npm cache clean --force

# Delete node_modules and reinstall
cd frontend
rm -rf node_modules package-lock.json
npm install

# Rebuild frontend
npm run build
```

### **React Development Server Issues**
```bash
# If Vite dev server fails to start
cd frontend
npm run dev -- --port 5174  # Try different port

# Clear Vite cache
npx vite --force
```

### **AI Search Not Working**
1. Check browser console for API errors
2. Verify ASP.NET Core server is running on port 5076
3. Enable AI search by clicking the purple sparkles button
4. Check Swagger documentation at `/swagger` for AI endpoints

##  Support

For questions or issues:
1. Check the Swagger documentation at `/swagger`
2. Review the code comments for implementation details
3. Test with the provided sample data
4. Examine the browser developer tools for client-side issues

---

**Built with**: ASP.NET Core 8.0, Entity Framework Core, Bootstrap 5, SQL Server
**Architecture**: MVC, Clean Architecture, 3NF Database Design
