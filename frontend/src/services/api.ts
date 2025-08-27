import axios from 'axios';
import type {
  Product,
  Customer,
  Order,
  Quote,
  QuoteRequest
} from '../types';
import { CustomerType } from '../types';

// Create axios instance with base configuration
const api = axios.create({
  baseURL: '/api', // Use relative URL to work with both dev and production
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error);
    return Promise.reject(error);
  }
);

// API Service class
export class ApiService {
  // Products
  static async getProducts(): Promise<Product[]> {
    const response = await api.get('/products');
    return response.data;
  }

  static async getProduct(id: number): Promise<Product> {
    const response = await api.get(`/products/${id}`);
    return response.data;
  }

  // Customers
  static async getCustomers(): Promise<Customer[]> {
    const response = await api.get('/customers');
    return response.data;
  }

  static async getCustomer(id: number): Promise<Customer> {
    const response = await api.get(`/customers/${id}`);
    return response.data;
  }

  static async createCustomer(customer: Omit<Customer, 'id'>): Promise<Customer> {
    const response = await api.post('/customers', customer);
    return response.data;
  }

  // Pricing
  static async generateQuote(request: QuoteRequest): Promise<Quote> {
    const response = await api.post('/pricing/quote', request);
    return response.data;
  }

  static async getProductPrices(customerId: number): Promise<any> {
    const response = await api.get(`/pricing/products/${customerId}`);
    return response.data;
  }

  // Orders
  static async getOrders(): Promise<Order[]> {
    const response = await api.get('/orders');
    return response.data;
  }

  static async getOrder(id: number): Promise<Order> {
    const response = await api.get(`/orders/${id}`);
    return response.data;
  }

  static async createOrder(request: QuoteRequest): Promise<Order> {
    const response = await api.post('/orders', request);
    return response.data;
  }

  static async updateOrderStatus(id: number, status: string): Promise<void> {
    await api.put(`/orders/${id}/status`, { status });
  }

  // AI-Enhanced Intelligent Search Methods
  
  // Intelligent product search with AI-powered ranking
  static async intelligentSearch(query: string, maxResults: number = 20): Promise<{
    Query: string;
    ResultCount: number;
    Products: Product[];
    SearchType: string;
  }> {
    try {
      const response = await api.get('/intelligentsearch/search', {
        params: { query, maxResults }
      });
      console.log('AI Search API Response:', response.data);
      return response.data;
    } catch (error) {
      console.error('AI Search API Error:', error);
      throw error;
    }
  }

  // Get intelligent search suggestions
  static async getSearchSuggestions(partial: string, maxSuggestions: number = 5): Promise<{
    PartialQuery: string;
    Suggestions: string[];
    SuggestionCount: number;
  }> {
    try {
      const response = await api.get('/intelligentsearch/suggestions', {
        params: { partial, maxSuggestions }
      });
      console.log('AI Suggestions API Response:', response.data);
      return response.data;
    } catch (error) {
      console.error('AI Suggestions API Error:', error);
      throw error;
    }
  }

  // Get AI-powered categories
  static async getIntelligentCategories(): Promise<{
    Categories: string[];
    CategoryCount: number;
  }> {
    const response = await api.get('/intelligentsearch/categories');
    return response.data;
  }

  // Get related search terms using AI analysis
  static async getRelatedSearchTerms(query: string): Promise<{
    OriginalQuery: string;
    RelatedTerms: string[];
    RelatedCount: number;
  }> {
    const response = await api.get('/intelligentsearch/related', {
      params: { query }
    });
    return response.data;
  }

  // Comprehensive intelligent search with all AI features
  static async comprehensiveSearch(query: string): Promise<{
    Query: string;
    SearchResults: { Products: Product[]; Count: number };
    Suggestions: string[];
    RelatedTerms: string[];
    Categories: string[];
    Features: string[];
  }> {
    const response = await api.get('/intelligentsearch/comprehensive', {
      params: { query }
    });
    return response.data;
  }
}

// Utility functions for price calculations
export class PricingUtils {
  static formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-ZA', {
      style: 'currency',
      currency: 'ZAR',
    }).format(amount);
  }

  static calculateDiscount(basePrice: number, discountedPrice: number): {
    amount: number;
    percentage: number;
  } {
    const amount = basePrice - discountedPrice;
    const percentage = basePrice > 0 ? (amount / basePrice) * 100 : 0;
    return { amount, percentage };
  }

  static getCustomerTypeLabel(type: CustomerType): string {
    return Number(type) === 0 ? 'Retail' : 'Wholesale';
  }

  static getCustomerTypeColor(type: CustomerType): string {
    return Number(type) === 0 ? 'bg-blue-100 text-blue-800' : 'bg-green-100 text-green-800';
  }

  static getStockStatus(stock: number): {
    label: string;
    color: string;
  } {
    if (stock === 0) {
      return { label: 'Out of Stock', color: 'bg-red-100 text-red-800' };
    } else if (stock < 10) {
      return { label: `Low Stock (${stock})`, color: 'bg-yellow-100 text-yellow-800' };
    } else {
      return { label: `In Stock (${stock})`, color: 'bg-green-100 text-green-800' };
    }
  }
}

// Export default instance for convenience
export default api;
