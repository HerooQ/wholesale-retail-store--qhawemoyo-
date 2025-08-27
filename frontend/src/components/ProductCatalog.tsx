import { useState, useEffect } from 'react';
import { Search, ChevronDown, ShoppingCart as ShoppingCartIcon, Brain } from 'lucide-react';
import type { Product, CartItem } from '../types';
import { CustomerType } from '../types';
import { ApiService, PricingUtils } from '../services/api';
import { IntelligentSearchBar } from './IntelligentSearchBar';

interface ProductCatalogProps {
  customerId: number | null;
  customerType: CustomerType | null;
  cartItems: CartItem[];
  onAddToCart: (productId: number, quantity: number) => void;
  onUpdateCart: (productId: number, quantity: number) => void;
  onRemoveFromCart: (productId: number) => void;
}

const ProductCatalog: React.FC<ProductCatalogProps> = ({
  customerId,
  customerType,
  cartItems,
  onAddToCart,
  onUpdateCart,
  onRemoveFromCart,
}) => {
  const [products, setProducts] = useState<Product[]>([]);
  const [filteredProducts, setFilteredProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [inStockOnly, setInStockOnly] = useState(false);
  const [sortBy, setSortBy] = useState<'name' | 'price-low' | 'price-high' | 'stock'>('name');

  const [aiSearchResults, setAISearchResults] = useState<Product[]>([]);
  const [searchMode, setSearchMode] = useState<'standard' | 'ai'>('standard');

  useEffect(() => {
    loadProducts();
  }, []);

  useEffect(() => {
    if (searchMode === 'standard') {
      filterAndSortProducts();
    }
  }, [products, searchTerm, inStockOnly, sortBy, searchMode]);

  useEffect(() => {
    if (searchMode === 'ai') {
      setFilteredProducts(aiSearchResults);
    }
  }, [aiSearchResults, searchMode]);

  const loadProducts = async () => {
    try {
      const productData = await ApiService.getProducts();
      setProducts(productData);
    } catch (error) {
      console.error('Failed to load products:', error);
    } finally {
      setLoading(false);
    }
  };

  const filterAndSortProducts = () => {
    let filtered = products.filter(product => {
      const matchesSearch = product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                           product.description.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesStock = !inStockOnly || product.stock > 0;
      return matchesSearch && matchesStock;
    });

    // Sort products
    filtered.sort((a, b) => {
      switch (sortBy) {
        case 'name':
          return a.name.localeCompare(b.name);
        case 'price-low':
          return a.basePrice - b.basePrice;
        case 'price-high':
          return b.basePrice - a.basePrice;
        case 'stock':
          return b.stock - a.stock;
        default:
          return 0;
      }
    });

    setFilteredProducts(filtered);
  };

  // AI-Enhanced Search Function with Fallback
  const handleAISearch = async (query: string, useAI: boolean = true) => {
    setSearchTerm(query);
    
    if (!query.trim()) {
      setSearchMode('standard');
      setAISearchResults([]);
      return;
    }

    if (useAI) {
      try {
        setSearchMode('ai');
        
        // Try AI search first
        const result = await ApiService.intelligentSearch(query, 20);
        
        // Ensure we have valid results
        if (result && result.Products && Array.isArray(result.Products)) {
          setAISearchResults(result.Products);
          console.log(`AI Search: "${query}" returned ${result.ResultCount || result.Products.length} results`);
        } else {
          console.warn('AI search returned invalid data, falling back to standard search');
          throw new Error('Invalid AI search response');
        }
      } catch (error) {
        console.error('AI search failed, using standard search:', error);
        
        // Fallback to standard search
        setSearchMode('standard');
        setAISearchResults([]);
        
        // The standard search will be triggered by the useEffect that watches searchTerm
      }
    } else {
      setSearchMode('standard');
      setAISearchResults([]);
    }
  };

  const handleSearchSuggestionSelect = (suggestion: string) => {
    handleAISearch(suggestion, true);
  };

  const getCartQuantity = (productId: number): number => {
    const cartItem = cartItems.find(item => item.product.id === productId);
    return cartItem ? cartItem.quantity : 0;
  };

  const handleQuantityChange = (productId: number, newQuantity: number) => {
    const currentQuantity = getCartQuantity(productId);

    if (newQuantity === 0) {
      onRemoveFromCart(productId);
    } else if (currentQuantity === 0) {
      onAddToCart(productId, newQuantity);
    } else {
      onUpdateCart(productId, newQuantity);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-2 text-gray-600">Loading products...</span>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* AI-Enhanced Search and Filters */}
      <div className="space-y-4">
        {/* Intelligent Search Bar */}
        <div className="flex-1 max-w-2xl">
          <IntelligentSearchBar
            onSearch={handleAISearch}
            onSuggestionSelect={handleSearchSuggestionSelect}
            placeholder="Search products with AI intelligence..."
            className="w-full"
          />
        </div>
        
        {/* Search Mode Indicator */}
        {searchTerm && (
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-3">
              {searchMode === 'ai' ? (
                <div className="flex items-center space-x-2 text-sm">
                  <div className="flex items-center space-x-1 px-3 py-1 bg-gradient-to-r from-purple-100 to-blue-100 text-purple-700 rounded-full">
                    <Brain className="w-4 h-4" />
                    <span>AI Search Active</span>
                  </div>
                  <span className="text-gray-500">•</span>
                  <span className="text-gray-600">Showing {filteredProducts.length} intelligent results for "{searchTerm}"</span>
                </div>
              ) : (
                <div className="flex items-center space-x-2 text-sm text-gray-600">
                  <Search className="w-4 h-4" />
                  <span>Standard search: {filteredProducts.length} results for "{searchTerm}"</span>
                </div>
              )}
            </div>
            
            {/* Clear Search */}
            <button
              onClick={() => handleAISearch('', false)}
              className="text-sm text-gray-500 hover:text-gray-700 px-3 py-1 rounded-md hover:bg-gray-100"
            >
              Clear search
            </button>
          </div>
        )}
      
        {/* Filter Controls */}
        <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
          <div className="text-sm text-gray-500">
            {!searchTerm && `Showing all ${filteredProducts.length} products`}
          </div>

          {/* Sort and Filter Controls */}
          <div className="flex items-center space-x-4">
          {/* Sort Dropdown */}
          <div className="relative">
            <select
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value as any)}
              className="appearance-none px-4 py-3 pr-10 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white"
            >
              <option value="name">Sort by Name</option>
              <option value="price-low">Price: Low to High</option>
              <option value="price-high">Price: High to Low</option>
              <option value="stock">Stock Level</option>
            </select>
            <ChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400 pointer-events-none" />
          </div>

          {/* In Stock Filter */}
          <label className="flex items-center space-x-2 whitespace-nowrap">
            <input
              type="checkbox"
              checked={inStockOnly}
              onChange={(e) => setInStockOnly(e.target.checked)}
              className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
            <span className="text-sm text-gray-700">In stock only</span>
          </label>
y          </div>
        </div>
      </div>

      {/* Products Grid */}
      {filteredProducts.length > 0 ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredProducts.map((product) => {
            // Use proper pricing logic - retail customers pay full price, wholesale get discounts
            const discountedPrice = customerType === 1 ? product.basePrice * 0.9 : product.basePrice;
            const hasDiscount = customerType === 1 && discountedPrice < product.basePrice;

            return (
              <div key={product.id} className="bg-white rounded-lg border border-gray-200 overflow-hidden hover:shadow-md transition-shadow">
                <div className="p-6">
                  {/* Product Header */}
                  <div className="flex items-start justify-between mb-3">
                    <h3 className="text-lg font-semibold text-gray-900">{product.name}</h3>
                    {/* Stock Badge */}
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                      product.stock > 50 ? 'bg-blue-100 text-blue-800' :
                      product.stock > 0 ? 'bg-blue-100 text-blue-800' :
                      'bg-gray-100 text-gray-800'
                    }`}>
                      {product.stock > 50 ? `In Stock (${product.stock})` :
                       product.stock > 0 ? `In Stock (${product.stock})` :
                       'Out of Stock'}
                    </span>
                  </div>

                  {/* Description */}
                  <p className="text-sm text-gray-600 mb-4 line-clamp-2">{product.description}</p>

                  {/* Price */}
                  <div className="mb-4">
                    {hasDiscount && customerId ? (
                      <div className="space-y-1">
                        <div className="text-2xl font-bold text-gray-900">
                          {PricingUtils.formatCurrency(discountedPrice)}
                        </div>
                        <div className="flex items-center space-x-2">
                          <span className="text-sm text-gray-500 line-through">
                            {PricingUtils.formatCurrency(product.basePrice)}
                          </span>
                          <span className="text-xs font-medium px-2 py-1 bg-green-100 text-green-800 rounded">
                            -{((product.basePrice - discountedPrice) / product.basePrice * 100).toFixed(0)}% off
                          </span>
                        </div>
                      </div>
                    ) : (
                      <div className="text-2xl font-bold text-gray-900">
                        {PricingUtils.formatCurrency(product.basePrice)}
                      </div>
                    )}
                  </div>

                  {/* Add to Cart Button */}
                  {customerId ? (
                    <button
                      onClick={() => handleQuantityChange(product.id, 1)}
                      disabled={product.stock === 0}
                      className="w-full flex items-center justify-center space-x-2 px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                      <ShoppingCartIcon className="h-4 w-4" />
                      <span>Add to Cart</span>
                    </button>
                  ) : (
                    <button
                      disabled
                      className="w-full flex items-center justify-center space-x-2 px-4 py-3 bg-gray-200 text-gray-500 rounded-lg cursor-not-allowed"
                    >
                      <ShoppingCartIcon className="h-4 w-4" />
                      <span>Select Customer</span>
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      ) : (
        <div className="text-center py-12">
          <div className="text-gray-500">
            <h3 className="text-lg font-medium text-gray-900 mb-2">No products found</h3>
            <p>Try adjusting your search or filter criteria.</p>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProductCatalog;