// API Response Types
export interface Product {
  id: number;
  name: string;
  description: string;
  stock: number;
  basePrice: number;
}

export interface Customer {
  id: number;
  name: string;
  email: string;
  customerType: CustomerType;
}

export interface Order {
  id: number;
  customerId: number;
  customerName: string;
  customerType: CustomerType;
  createdAt: string;
  totalAmount: number;
  status: OrderStatus;
  items: OrderItem[];
}

export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface QuoteRequest {
  customerId: number;
  items: QuoteItem[];
}

export interface QuoteItem {
  productId: number;
  quantity: number;
}

export interface Quote {
  customerId: number;
  customerName: string;
  customerType: CustomerType;
  generatedAt: string;
  items: QuoteItemDetail[];
  subtotal: number;
  additionalDiscount: number;
  total: number;
}

export interface QuoteItemDetail {
  productId: number;
  productName: string;
  quantity: number;
  basePrice: number;
  discountedPrice: number;
  discountAmount: number;
  lineTotal: number;
}

// Enums (const assertions for erasableSyntaxOnly compatibility)
export const CustomerType = {
  Retail: 0,
  Wholesale: 1
} as const;

export type CustomerType = 0 | 1;

export const OrderStatus = {
  Pending: 0,
  Confirmed: 1,
  Shipped: 2,
  Cancelled: 3
} as const;

export type OrderStatus = typeof OrderStatus[keyof typeof OrderStatus];

// UI State Types
export interface CartItem {
  product: Product;
  quantity: number;
  discountedPrice: number;
  discountAmount: number;
}

export interface CartState {
  items: CartItem[];
  customerId: number | null;
  customerType: CustomerType | null;
  subtotal: number;
  totalDiscount: number;
  total: number;
}

export interface FilterState {
  searchTerm: string;
  priceRange: [number, number];
  inStockOnly: boolean;
  sortBy: 'name' | 'price-low' | 'price-high' | 'stock';
}

// API Response Types
export interface ApiResponse<T> {
  data?: T;
  error?: string;
  message?: string;
}

export interface ProductsResponse {
  products: Product[];
}

export interface CustomersResponse {
  customers: Customer[];
}

export interface PricingResponse {
  customerId: number;
  customerName: string;
  customerType: CustomerType;
  products: ProductPricing[];
}

export interface ProductPricing {
  id: number;
  name: string;
  description: string;
  stock: number;
  basePrice: number;
  calculatedPrice: number;
  customerType: CustomerType;
}
