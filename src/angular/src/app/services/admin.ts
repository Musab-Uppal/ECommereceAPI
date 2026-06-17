import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private base = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  getDashboard(): Observable<any> {
    return this.http.get(`${this.base}/dashboard`);
  }

  // Users
  getUsers(page = 1, pageSize = 10): Observable<any> {
    return this.http.get(`${this.base}/users`, { params: { page, pageSize } });
  }

  changeUserRole(userId: number, newRole: string): Observable<any> {
    return this.http.put(`${this.base}/users/${userId}/role`, { newRole });
  }

  // Products
  getProducts(page = 1, pageSize = 10): Observable<any> {
    return this.http.get(`${this.base}/products`, { params: { page, pageSize } });
  }

  createProduct(dto: any): Observable<any> {
    return this.http.post(`${this.base}/products`, dto);
  }

  updateProduct(id: number, dto: any): Observable<any> {
    return this.http.put(`${this.base}/products/${id}`, dto);
  }

  deleteProduct(id: number): Observable<any> {
    return this.http.delete(`${this.base}/products/${id}`);
  }

  addStock(id: number, quantity: number): Observable<any> {
    return this.http.post(`${this.base}/products/${id}/add-stock`, { quantity });
  }

  getLowStockProducts(): Observable<any> {
    return this.http.get(`${this.base}/products/low-stock`);
  }

  // Categories
  getCategories(): Observable<any> {
    return this.http.get(`${this.base}/categories`);
  }

  createCategory(dto: any): Observable<any> {
    return this.http.post(`${this.base}/categories`, dto);
  }

  updateCategory(id: number, dto: any): Observable<any> {
    return this.http.put(`${this.base}/categories/${id}`, dto);
  }

  deleteCategory(id: number): Observable<any> {
    return this.http.delete(`${this.base}/categories/${id}`);
  }

  // Orders
  getOrders(page = 1, pageSize = 10, status?: string): Observable<any> {
    const params: any = { page, pageSize };
    if (status) params['status'] = status;
    return this.http.get(`${this.base}/orders`, { params });
  }

  updateOrderStatus(id: number, status: string): Observable<any> {
    return this.http.put(`${this.base}/orders/${id}/status`, { status });
  }
}
