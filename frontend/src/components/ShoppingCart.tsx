import { useState } from 'react';
import { Trash2, Plus, Minus, ShoppingBag, CreditCard } from 'lucide-react';
import type { CartState, CartItem } from '../types';
import { CustomerType } from '../types';
import { ApiService, PricingUtils } from '../services/api';

interface ShoppingCartProps {
  cart: CartState;
  onUpdateCart: (productId: number, quantity: number) => void;
  onRemoveFromCart: (productId: number) => void;
  onClearCart: () => void;
}

const ShoppingCart: React.FC<ShoppingCartProps> = ({
  cart,
  onUpdateCart,
  onRemoveFromCart,
  onClearCart,
}) => {
  const [creatingOrder, setCreatingOrder] = useState(false);
  const [orderCreated, setOrderCreated] = useState(false);
  const [orderId, setOrderId] = useState<number | null>(null);

  const handleQuantityChange = (productId: number, newQuantity: number) => {
    if (newQuantity === 0) {
      onRemoveFromCart(productId);
    } else {
      onUpdateCart(productId, newQuantity);
    }
  };

  const handleCreateOrder = async () => {
    if (!cart.customerId || cart.items.length === 0) return;

    setCreatingOrder(true);
    try {
      const orderRequest = {
        customerId: cart.customerId,
        items: cart.items.map(item => ({
          productId: item.product.id,
          quantity: item.quantity,
        })),
      };

      const order = await ApiService.createOrder(orderRequest);
      setOrderId(order.id);
      setOrderCreated(true);
      onClearCart();
    } catch (error) {
      console.error('Failed to create order:', error);
      alert('Failed to create order. Please try again.');
    } finally {
      setCreatingOrder(false);
    }
  };

  const calculateItemTotal = (item: CartItem): number => {
    return item.discountedPrice * item.quantity;
  };

  const getTotalItems = (): number => {
    return cart.items.reduce((total, item) => total + item.quantity, 0);
  };

  if (cart.items.length === 0 && !orderCreated) {
    return (
      <div className="text-center py-12">
        <ShoppingBag className="h-16 w-16 text-gray-400 mx-auto mb-4" />
        <h3 className="text-xl font-medium text-gray-900 mb-2">Your cart is empty</h3>
        <p className="text-gray-600 mb-6">
          Add some products to get started with your order.
        </p>
        <button
          onClick={() => window.location.reload()} // This would ideally be handled by the parent component
          className="bg-blue-600 text-white px-6 py-3 rounded-md hover:bg-blue-700 transition-colors"
        >
          Browse Products
        </button>
      </div>
    );
  }

  if (orderCreated && orderId) {
    return (
      <div className="text-center py-12">
        <div className="bg-green-50 border border-green-200 rounded-lg p-8 max-w-md mx-auto">
          <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <CreditCard className="h-8 w-8 text-green-600" />
          </div>
          <h3 className="text-xl font-semibold text-green-900 mb-2">
            Order Created Successfully!
          </h3>
          <p className="text-green-700 mb-4">
            Your order #{orderId} has been placed and confirmed.
          </p>
          <button
            onClick={() => {
              setOrderCreated(false);
              setOrderId(null);
            }}
            className="bg-green-600 text-white px-6 py-3 rounded-md hover:bg-green-700 transition-colors"
          >
            Continue Shopping
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold text-gray-900">Shopping Cart</h2>
          <p className="text-gray-600 mt-1">
            {getTotalItems()} item{getTotalItems() !== 1 ? 's' : ''} in your cart
          </p>
        </div>

        {cart.items.length > 0 && (
          <button
            onClick={onClearCart}
            className="flex items-center space-x-2 px-4 py-2 text-red-600 hover:text-red-700 hover:bg-red-50 rounded-md transition-colors"
          >
            <Trash2 className="h-4 w-4" />
            <span>Clear Cart</span>
          </button>
        )}
      </div>

      {/* Customer Info */}
      {cart.customerId && cart.customerType && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <div className="flex items-center space-x-3">
            <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
              <span className="text-blue-600 font-semibold text-sm">
                {cart.customerType !== null && Number(cart.customerType) === 0 ? 'R' : 'W'}
              </span>
            </div>
            <div>
              <p className="font-medium text-blue-900">
                Order for {PricingUtils.getCustomerTypeLabel(cart.customerType)} Customer
              </p>
              <p className="text-sm text-blue-700">
                {cart.customerType !== null && Number(cart.customerType) === 0
                  ? 'Full retail pricing applies'
                  : 'Wholesale discounts applied'
                }
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Cart Items */}
      <div className="space-y-4">
        {cart.items.map((item) => (
          <div key={item.product.id} className="bg-white border rounded-lg p-6">
            <div className="flex flex-col lg:flex-row lg:items-center gap-4">
              {/* Product Info */}
              <div className="flex-1">
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  {item.product.name}
                </h3>
                <p className="text-gray-600 text-sm mb-3">
                  {item.product.description}
                </p>

                {/* Stock Status */}
                <div className="flex items-center space-x-4 text-sm">
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                    PricingUtils.getStockStatus(item.product.stock).color
                  }`}>
                    {PricingUtils.getStockStatus(item.product.stock).label}
                  </span>
                </div>
              </div>

              {/* Quantity Controls */}
              <div className="flex items-center space-x-3">
                <div className="flex items-center border border-gray-300 rounded-md">
                  <button
                    onClick={() => handleQuantityChange(item.product.id, item.quantity - 1)}
                    className="px-3 py-2 text-gray-600 hover:text-gray-800 hover:bg-gray-50"
                  >
                    <Minus className="h-4 w-4" />
                  </button>

                  <input
                    type="number"
                    min="1"
                    max={item.product.stock}
                    value={item.quantity}
                    onChange={(e) => handleQuantityChange(item.product.id, parseInt(e.target.value) || 1)}
                    className="w-16 px-2 py-2 text-center border-0 focus:ring-0"
                  />

                  <button
                    onClick={() => handleQuantityChange(item.product.id, item.quantity + 1)}
                    className="px-3 py-2 text-gray-600 hover:text-gray-800 hover:bg-gray-50"
                    disabled={item.quantity >= item.product.stock}
                  >
                    <Plus className="h-4 w-4" />
                  </button>
                </div>

                <button
                  onClick={() => onRemoveFromCart(item.product.id)}
                  className="p-2 text-red-600 hover:text-red-700 hover:bg-red-50 rounded-md"
                >
                  <Trash2 className="h-4 w-4" />
                </button>
              </div>

              {/* Pricing */}
              <div className="text-right min-w-0 lg:min-w-[200px]">
                <div className="space-y-1">
                  {item.discountAmount > 0 ? (
                    <div>
                      <div className="flex items-center justify-end space-x-2">
                        <span className="text-lg font-bold text-green-600">
                          {PricingUtils.formatCurrency(item.discountedPrice)}
                        </span>
                        <span className="text-sm text-gray-500 line-through">
                          {PricingUtils.formatCurrency(item.product.basePrice)}
                        </span>
                      </div>
                      <p className="text-xs text-green-600">
                        Save {PricingUtils.formatCurrency(item.discountAmount)} per item
                      </p>
                    </div>
                  ) : (
                    <span className="text-lg font-bold text-gray-900">
                      {PricingUtils.formatCurrency(item.product.basePrice)}
                    </span>
                  )}

                  <p className="text-sm text-gray-600">
                    Subtotal: {PricingUtils.formatCurrency(calculateItemTotal(item))}
                  </p>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Order Summary */}
      {cart.items.length > 0 && (
        <div className="bg-white border rounded-lg p-6">
          <h3 className="text-xl font-semibold text-gray-900 mb-4">Order Summary</h3>

          <div className="space-y-3">
            <div className="flex justify-between text-sm">
              <span>Subtotal ({getTotalItems()} items)</span>
              <span>{PricingUtils.formatCurrency(cart.subtotal)}</span>
            </div>

            {cart.totalDiscount > 0 && (
              <div className="flex justify-between text-sm text-green-600">
                <span>Total Savings</span>
                <span>-{PricingUtils.formatCurrency(cart.totalDiscount)}</span>
              </div>
            )}

            <hr />

            <div className="flex justify-between text-lg font-semibold">
              <span>Total</span>
              <span>{PricingUtils.formatCurrency(cart.total)}</span>
            </div>

            {cart.customerType === CustomerType.Wholesale && cart.totalDiscount > 0 && (
              <div className="bg-green-50 border border-green-200 rounded-md p-3">
                <p className="text-sm text-green-800">
                  <strong>Wholesale Discount Applied:</strong> You save{' '}
                  {PricingUtils.formatCurrency(cart.totalDiscount)} on this order!
                </p>
              </div>
            )}
          </div>

          <div className="mt-6">
            <button
              onClick={handleCreateOrder}
              disabled={creatingOrder || !cart.customerId}
              className="w-full bg-blue-600 text-white py-3 px-6 rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center space-x-2 transition-colors"
            >
              {creatingOrder ? (
                <>
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                  <span>Creating Order...</span>
                </>
              ) : (
                <>
                  <CreditCard className="h-5 w-5" />
                  <span>Place Order</span>
                </>
              )}
            </button>

            {!cart.customerId && (
              <p className="text-red-600 text-sm mt-2 text-center">
                Please select a customer before placing an order.
              </p>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default ShoppingCart;
