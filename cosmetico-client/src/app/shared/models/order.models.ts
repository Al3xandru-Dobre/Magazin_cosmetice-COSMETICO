export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Order {
  id: number;
  placedAt: string;
  status: string;
  totalAmount: number;
  shippingAddress: string;
  items: OrderItem[];
}

export interface CreateOrderRequest {
  shippingAddress: string;
  items: { productId: number; quantity: number }[];
}

export const ORDER_STATUSES = ['Pending', 'Paid', 'Shipped', 'Delivered', 'Cancelled'] as const;
