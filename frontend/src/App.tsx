import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import { Store, Package, FileText, ShoppingCart as ShoppingCartIcon } from 'lucide-react';
import HomePage from './components/HomePage';
import ShoppingCart from './components/ShoppingCart';
import OrderHistory from './components/OrderHistory';
import type { CartState, Customer, CartItem } from './types';
import { ApiService } from './services/api';
import './App.css';

function App() {
  // Set document title
  document.title = "Wholesale Retail Store";
  const [cart, setCart] = useState<CartState>({
    items: [],
    customerId: null,
    customerType: null,
    subtotal: 0,
    totalDiscount: 0,
    total: 0,
  });

  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [orderCount, setOrderCount] = useState(0);

  useEffect(() => {
    loadCustomers();
    loadOrderCount();
  }, []);

  const loadOrderCount = async () => {
    try {
      const orders = await ApiService.getOrders();
      setOrderCount(orders.length);
    } catch (error) {
      console.error('Failed to load order count:', error);
    }
  };

  const loadCustomers = async () => {
    try {
      const customerData = await ApiService.getCustomers();
      setCustomers(customerData);
    } catch (error) {
      console.error('Failed to load customers:', error);
    } finally {
      setLoading(false);
    }
  };



  const handleCustomerSelect = (customer: Customer) => {
    setCart(prev => ({
      ...prev,
      customerId: customer.id,
      customerType: customer.customerType,
      // Recalculate prices when customer changes
      items: prev.items.map(item => {
        // This would need to recalculate prices based on new customer type
        // For now, we'll keep the same prices
        return item;
      })
    }));
  };

  const addToCart = async (productId: number, quantity: number) => {
    if (!cart.customerId || cart.customerType === null || cart.customerType === undefined) return;

    try {
      // Get product details and calculate pricing
      const products = await ApiService.getProducts();
      const product = products.find(p => p.id === productId);
      if (!product) return;

      const pricingResponse = await ApiService.getProductPrices(cart.customerId);
      const productPricing = pricingResponse.products.find((p: any) => p.id === productId);

      const discountedPrice = productPricing?.calculatedPrice || product.basePrice;
      const discountAmount = product.basePrice - discountedPrice;

      const newItem: CartItem = {
        product,
        quantity,
        discountedPrice,
        discountAmount,
      };

      setCart(prev => ({
        ...prev,
        items: [...prev.items.filter(item => item.product.id !== productId), newItem],
        subtotal: prev.subtotal + (discountedPrice * quantity),
        totalDiscount: prev.totalDiscount + (discountAmount * quantity),
        total: prev.total + (discountedPrice * quantity),
      }));
    } catch (error) {
      console.error('Failed to add item to cart:', error);
      alert('Failed to add item to cart. Please try again.');
    }
  };

  const updateCartItem = (productId: number, quantity: number) => {
    if (quantity === 0) {
      removeFromCart(productId);
      return;
    }

    setCart(prev => {
      const item = prev.items.find(item => item.product.id === productId);
      if (!item) return prev;

      const quantityDiff = quantity - item.quantity;
      const priceDiff = item.discountedPrice * quantityDiff;
      const discountDiff = item.discountAmount * quantityDiff;

      return {
        ...prev,
        items: prev.items.map(cartItem =>
          cartItem.product.id === productId
            ? { ...cartItem, quantity }
            : cartItem
        ),
        subtotal: prev.subtotal + priceDiff,
        totalDiscount: prev.totalDiscount + discountDiff,
        total: prev.total + priceDiff,
      };
    });
  };

  const removeFromCart = (productId: number) => {
    setCart(prev => {
      const item = prev.items.find(item => item.product.id === productId);
      if (!item) return prev;

      const itemTotal = item.discountedPrice * item.quantity;
      const itemDiscount = item.discountAmount * item.quantity;

      return {
        ...prev,
        items: prev.items.filter(item => item.product.id !== productId),
        subtotal: prev.subtotal - itemTotal,
        totalDiscount: prev.totalDiscount - itemDiscount,
        total: prev.total - itemTotal,
      };
    });
  };

  const clearCart = () => {
    setCart({
      items: [],
      customerId: null,
      customerType: null,
      subtotal: 0,
      totalDiscount: 0,
      total: 0,
    });
    // Refresh order count after clearing cart (usually after creating an order)
    loadOrderCount();
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <Router>
      <div className="min-h-screen bg-gray-50">
        {/* Header */}
        <header className="bg-white shadow-sm border-b">
          <div className="px-4 py-4">
            <div className="flex items-center justify-between">
              {/* Left: Store Brand */}
              <div className="flex items-center space-x-3">
                <div className="bg-blue-500 text-white p-2 rounded-lg">
                  <Store className="h-6 w-6" />
                </div>
                <div>
                  <h1 className="text-xl font-bold text-gray-900">Wholesale Retail Store</h1>
                  <p className="text-sm text-gray-600">Professional B2B Commerce Platform</p>
                </div>
              </div>
              
              {/* Right: Navigation */}
              <nav className="flex items-center space-x-6">
                <Link 
                  to="/" 
                  className="flex items-center space-x-2 px-3 py-2 text-gray-700 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                >
                  <Package className="h-4 w-4" />
                  <span>Products</span>
                </Link>
                <Link 
                  to="/cart" 
                  className="flex items-center space-x-2 px-3 py-2 text-gray-700 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                >
                  <ShoppingCartIcon className="h-4 w-4" />
                  <span>Cart</span>
                  {cart.items.length > 0 && (
                    <span className="bg-blue-500 text-white text-xs rounded-full px-2 py-1 min-w-[20px] text-center">
                      {cart.items.length}
                    </span>
                  )}
                </Link>
                <Link 
                  to="/orders" 
                  className="flex items-center space-x-2 px-3 py-2 text-gray-700 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                >
                  <FileText className="h-4 w-4" />
                  <span>Orders</span>
                  {orderCount > 0 && (
                    <span className="bg-blue-500 text-white text-xs rounded-full px-2 py-1 min-w-[20px] text-center">
                      {orderCount}
                    </span>
                  )}
                </Link>
              </nav>
            </div>
          </div>
        </header>

        {/* Main Content */}
        <main className="px-4 py-4">
          <Routes>
            <Route 
              path="/" 
              element={
                <HomePage 
                  customers={customers}
                  selectedCustomerId={cart.customerId}
                  onCustomerSelect={handleCustomerSelect}
                  onCustomerCreate={loadCustomers}
                  customerId={cart.customerId}
                  customerType={cart.customerType}
                  cartItems={cart.items}
                  onAddToCart={addToCart}
                  onUpdateCart={updateCartItem}
                  onRemoveFromCart={removeFromCart}
                />
              } 
            />
            <Route 
              path="/cart" 
              element={
                <ShoppingCart
                  cart={cart}
                  onUpdateCart={updateCartItem}
                  onRemoveFromCart={removeFromCart}
                  onClearCart={clearCart}
                />
              } 
            />
            <Route path="/orders" element={<OrderHistory onOrderCountChange={loadOrderCount} />} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default App;
