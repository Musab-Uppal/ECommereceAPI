/**
 * User and Authentication Types
 */
export interface User {
  id: number;
  name: string;
  email: string;
  phone: string;
  address: string;
  role: "admin" | "customer";
  createdAt: Date;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: UserAuthDto;
}

export interface UserAuthDto {
  id: number;
  email: string;
  name: string;
  role: "admin" | "customer";
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  phone: string;
  address: string;
}

/**
 * Product Types
 */
export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  category: string;
  categoryId: number;
  stock: number;
  rating: number;
  reviews: number;
  image?: string;
  inStock: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface ProductFilter {
  category?: string;
  minPrice?: number;
  maxPrice?: number;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  stock: number;
  categoryId: number;
}

/**
 * Cart Types
 */
export interface CartItem {
  productId: number;
  name: string;
  price: number;
  quantity: number;
  image?: string;
  inStock: boolean;
}

export interface Cart {
  items: CartItem[];
  subtotal: number;
  tax: number;
  shipping: number;
  total: number;
}

export interface AddToCartRequest {
  productId: number;
  quantity: number;
}

/**
 * Order Types
 */
export interface Order {
  id: number;
  userId: number;
  items: OrderItem[];
  subtotal: number;
  tax: number;
  shipping: number;
  total: number;
  status: OrderStatus;
  createdAt: Date;
  updatedAt: Date;
}

export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;
  price: number;
  discount: number;
}

export type OrderStatus = "pending" | "shipped" | "delivered" | "cancelled";

export interface CreateOrderRequest {
  items: OrderItemInput[];
}

export interface OrderItemInput {
  productId: number;
  quantity: number;
  discount: number;
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
}

/**
 * Category Types
 */
export interface Category {
  id: number;
  name: string;
  description?: string;
  createdAt: Date;
  updatedAt: Date;
}

/**
 * Review Types
 */
export interface Review {
  id: number;
  productId: number;
  userId: number;
  rating: number;
  comment: string;
  userName: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateReviewRequest {
  productId: number;
  rating: number;
  comment: string;
}

/**
 * API Response Types
 */
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: Record<string, string[]>;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Error Types
 */
export interface ApiError {
  code: string;
  message: string;
  statusCode: number;
}

/**
 * Form Types
 */
export interface LoginFormData {
  email: string;
  password: string;
}

export interface RegisterFormData {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  phone: string;
  address: string;
}

export interface UpdateProfileFormData {
  firstName: string;
  lastName: string;
  phone: string;
  address: string;
}

export interface ChangePasswordFormData {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
