import React from 'react';
import { Users, Package } from 'lucide-react';
import CustomerSelector from './CustomerSelector';
import ProductCatalog from './ProductCatalog';
import type { Customer, CartItem } from '../types';

interface HomePageProps {
  customers: Customer[];
  selectedCustomerId: number | null;
  onCustomerSelect: (customer: Customer) => void;
  onCustomerCreate: () => void;
  customerId: number | null;
  customerType: number | null;
  cartItems: CartItem[];
  onAddToCart: (productId: number, quantity: number) => void;
  onUpdateCart: (productId: number, quantity: number) => void;
  onRemoveFromCart: (productId: number) => void;
}

const HomePage: React.FC<HomePageProps> = ({
  customers,
  selectedCustomerId,
  onCustomerSelect,
  onCustomerCreate,
  customerId,
  customerType,
  cartItems,
  onAddToCart,
  onUpdateCart,
  onRemoveFromCart,
}) => {
  return (
    <div className="space-y-6">
      {/* Customer Management Card */}
      <div className="bg-white rounded-lg shadow-sm border p-6">
        <div className="flex items-center space-x-2 mb-6">
          <Users className="h-5 w-5 text-gray-600" />
          <h2 className="text-lg font-semibold text-gray-900">Customer Management</h2>
        </div>
        
        <CustomerSelector
          customers={customers}
          selectedCustomerId={selectedCustomerId}
          onCustomerSelect={onCustomerSelect}
          onCreateCustomer={onCustomerCreate}
        />
      </div>

      {/* Product Catalog Card */}
      <div className="bg-white rounded-lg shadow-sm border">
        <div className="p-6 border-b">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <Package className="h-5 w-5 text-gray-600" />
              <h2 className="text-lg font-semibold text-gray-900">Product Catalog</h2>
            </div>
            {!customerId && (
              <div className="text-sm text-orange-600 bg-orange-50 px-3 py-1 rounded-md">
                Please select a customer to see pricing and add items to cart
              </div>
            )}
          </div>
        </div>
        
        <div className="p-6">
          <ProductCatalog
            customerId={customerId}
            customerType={customerType as any}
            cartItems={cartItems}
            onAddToCart={onAddToCart}
            onUpdateCart={onUpdateCart}
            onRemoveFromCart={onRemoveFromCart}
          />
        </div>
      </div>
    </div>
  );
};

export default HomePage;
