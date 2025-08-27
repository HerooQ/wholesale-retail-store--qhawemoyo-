import { useState } from 'react';
import { Plus, ChevronDown } from 'lucide-react';
import type { Customer } from '../types';
import { CustomerType } from '../types';
import { ApiService, PricingUtils } from '../services/api';

interface CustomerSelectorProps {
  customers: Customer[];
  selectedCustomerId: number | null;
  onCustomerSelect: (customer: Customer) => void;
  onCreateCustomer: () => void;
}

const CustomerSelector: React.FC<CustomerSelectorProps> = ({
  customers,
  selectedCustomerId,
  onCustomerSelect,
  onCreateCustomer,
}) => {
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [newCustomer, setNewCustomer] = useState({
    name: '',
    email: '',
    customerType: 0,
  });
  const [isCreating, setIsCreating] = useState(false);

  const handleCreateCustomer = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newCustomer.name || !newCustomer.email) return;

    setIsCreating(true);
    try {
      const createdCustomer = await ApiService.createCustomer({
        name: newCustomer.name,
        email: newCustomer.email,
        customerType: newCustomer.customerType as CustomerType,
      });
      
      onCustomerSelect(createdCustomer);
      setNewCustomer({ name: '', email: '', customerType: 0 });
      setShowCreateForm(false);
      onCreateCustomer(); // Refresh the customer list
    } catch (error) {
      console.error('Failed to create customer:', error);
      alert('Failed to create customer. Please try again.');
    } finally {
      setIsCreating(false);
    }
  };

  return (
    <div className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Select Customer
        </label>
        
        <div className="flex space-x-3">
          {/* Customer Dropdown */}
          <div className="flex-1 relative">
            <select
              value={selectedCustomerId || ''}
              onChange={(e) => {
                const customerId = parseInt(e.target.value);
                const customer = customers.find(c => c.id === customerId);
                if (customer) {
                  onCustomerSelect(customer);
                }
              }}
              className="w-full px-4 py-3 pr-10 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 appearance-none bg-white"
            >
              <option value="">Choose a customer...</option>
              {customers.map((customer) => (
                <option key={customer.id} value={customer.id}>
                  {customer.name} ({PricingUtils.getCustomerTypeLabel(customer.customerType)})
                </option>
              ))}
            </select>
            <ChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400 pointer-events-none" />
          </div>

          {/* New Customer Button */}
          <button
            onClick={() => setShowCreateForm(!showCreateForm)}
            className="flex items-center space-x-2 px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors whitespace-nowrap"
          >
            <Plus className="h-4 w-4" />
            <span>New Customer</span>
          </button>
        </div>
      </div>

      {/* Create Customer Form */}
      {showCreateForm && (
        <div className="p-4 border border-gray-200 rounded-lg bg-gray-50">
          <form onSubmit={handleCreateCustomer} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Name
                </label>
                <input
                  type="text"
                  value={newCustomer.name}
                  onChange={(e) => setNewCustomer(prev => ({ ...prev, name: e.target.value }))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  placeholder="Customer name"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Email
                </label>
                <input
                  type="email"
                  value={newCustomer.email}
                  onChange={(e) => setNewCustomer(prev => ({ ...prev, email: e.target.value }))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  placeholder="customer@example.com"
                  required
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Customer Type
              </label>
              <select
                value={newCustomer.customerType}
                onChange={(e) => setNewCustomer(prev => ({ ...prev, customerType: parseInt(e.target.value) }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              >
                <option value={0}>Retail</option>
                <option value={1}>Wholesale</option>
              </select>
            </div>

            <div className="flex space-x-2">
              <button
                type="submit"
                disabled={isCreating}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {isCreating ? 'Creating...' : 'Create Customer'}
              </button>
              <button
                type="button"
                onClick={() => setShowCreateForm(false)}
                className="px-4 py-2 bg-gray-300 text-gray-700 rounded-md hover:bg-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};

export default CustomerSelector;