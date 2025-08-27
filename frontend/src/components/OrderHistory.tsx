import { useState, useEffect } from 'react';
import { Package, Calendar, User } from 'lucide-react';
import type { Order } from '../types';
import { OrderStatus } from '../types';
import { ApiService, PricingUtils } from '../services/api';

interface OrderHistoryProps {
  onOrderCountChange?: () => void;
}

const OrderHistory: React.FC<OrderHistoryProps> = ({ onOrderCountChange }) => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);

  useEffect(() => {
    loadOrders();
  }, []);

  const loadOrders = async () => {
    try {
      const orderData = await ApiService.getOrders();
      setOrders(orderData);
      // Notify parent component about order count change
      if (onOrderCountChange) {
        onOrderCountChange();
      }
    } catch (error) {
      console.error('Failed to load orders:', error);
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status: OrderStatus): string => {
    switch (status) {
      case OrderStatus.Pending:
        return 'bg-yellow-100 text-yellow-800';
      case OrderStatus.Confirmed:
        return 'bg-blue-100 text-blue-800';
      case OrderStatus.Shipped:
        return 'bg-green-100 text-green-800';
      case OrderStatus.Cancelled:
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusLabel = (status: OrderStatus): string => {
    switch (status) {
      case OrderStatus.Pending:
        return 'Pending';
      case OrderStatus.Confirmed:
        return 'Confirmed';
      case OrderStatus.Shipped:
        return 'Shipped';
      case OrderStatus.Cancelled:
        return 'Cancelled';
      default:
        return 'Unknown';
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-2 text-gray-600">Loading orders...</span>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold text-gray-900">Order History</h2>
          <p className="text-gray-600 mt-1">
            {orders.length} order{orders.length !== 1 ? 's' : ''} found
          </p>
        </div>

        {orders.length > 0 && (
          <button
            onClick={loadOrders}
            className="flex items-center space-x-2 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
          >
            <Package className="h-4 w-4" />
            <span>Refresh</span>
          </button>
        )}
      </div>

      {orders.length === 0 ? (
        <div className="text-center py-12">
          <Package className="h-16 w-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-xl font-medium text-gray-900 mb-2">No orders found</h3>
          <p className="text-gray-600 mb-6">
            When you place orders, they will appear here.
          </p>
          <button
            onClick={() => window.location.reload()} // This would ideally navigate to products
            className="bg-blue-600 text-white px-6 py-3 rounded-md hover:bg-blue-700 transition-colors"
          >
            Browse Products
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Orders List */}
          <div className="space-y-4">
            <h3 className="text-lg font-semibold text-gray-900">Recent Orders</h3>
            {orders.map((order) => (
              <div
                key={order.id}
                className={`border rounded-lg p-4 cursor-pointer transition-colors ${
                  selectedOrder?.id === order.id
                    ? 'border-blue-500 bg-blue-50'
                    : 'border-gray-200 hover:border-gray-300'
                }`}
                onClick={() => setSelectedOrder(order)}
              >
                <div className="flex justify-between items-start mb-3">
                  <div>
                    <h4 className="font-semibold text-gray-900">
                      Order #{order.id}
                    </h4>
                    <div className="flex items-center space-x-4 text-sm text-gray-600 mt-1">
                      <div className="flex items-center space-x-1">
                        <User className="h-4 w-4" />
                        <span>{order.customerName}</span>
                      </div>
                      <div className="flex items-center space-x-1">
                        <Calendar className="h-4 w-4" />
                        <span>{new Date(order.createdAt).toLocaleDateString()}</span>
                      </div>
                    </div>
                  </div>
                  <div className="text-right">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(order.status)}`}>
                      {getStatusLabel(order.status)}
                    </span>
                  </div>
                </div>

                <div className="flex justify-between items-center">
                  <div className="text-sm text-gray-600">
                    {order.items.length} item{order.items.length !== 1 ? 's' : ''}
                  </div>
                  <div className="font-semibold text-gray-900">
                    {PricingUtils.formatCurrency(order.totalAmount)}
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Order Details */}
          <div>
            {selectedOrder ? (
              <div className="bg-white border rounded-lg p-6">
                <h3 className="text-xl font-semibold text-gray-900 mb-4">
                  Order #{selectedOrder.id} Details
                </h3>

                <div className="grid grid-cols-2 gap-4 mb-6">
                  <div>
                    <h4 className="font-medium text-gray-900 mb-2">Customer Information</h4>
                    <div className="space-y-1 text-sm">
                      <p><strong>Name:</strong> {selectedOrder.customerName}</p>
                      <p><strong>Type:</strong> {PricingUtils.getCustomerTypeLabel(selectedOrder.customerType)}</p>
                    </div>
                  </div>

                  <div>
                    <h4 className="font-medium text-gray-900 mb-2">Order Information</h4>
                    <div className="space-y-1 text-sm">
                      <p><strong>Date:</strong> {new Date(selectedOrder.createdAt).toLocaleString()}</p>
                      <p>
                        <strong>Status:</strong>
                        <span className={`ml-1 px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(selectedOrder.status)}`}>
                          {getStatusLabel(selectedOrder.status)}
                        </span>
                      </p>
                    </div>
                  </div>
                </div>

                <h4 className="font-medium text-gray-900 mb-3">Order Items</h4>
                <div className="space-y-3">
                  {selectedOrder.items.map((item) => (
                    <div key={item.productId} className="flex justify-between items-center py-2 border-b border-gray-100">
                      <div className="flex-1">
                        <h5 className="font-medium text-gray-900">{item.productName}</h5>
                        <p className="text-sm text-gray-600">
                          Quantity: {item.quantity} × {PricingUtils.formatCurrency(item.unitPrice)}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="font-semibold text-gray-900">
                          {PricingUtils.formatCurrency(item.totalPrice)}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>

                <div className="mt-6 pt-4 border-t border-gray-200">
                  <div className="flex justify-between items-center">
                    <span className="text-lg font-medium text-gray-900">Total Amount</span>
                    <span className="text-2xl font-bold text-blue-600">
                      {PricingUtils.formatCurrency(selectedOrder.totalAmount)}
                    </span>
                  </div>
                </div>
              </div>
            ) : (
              <div className="bg-gray-50 border-2 border-dashed border-gray-300 rounded-lg p-8 text-center">
                <Package className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                <h3 className="text-lg font-medium text-gray-900 mb-2">Select an Order</h3>
                <p className="text-gray-600">
                  Click on any order from the list to view its details.
                </p>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default OrderHistory;
