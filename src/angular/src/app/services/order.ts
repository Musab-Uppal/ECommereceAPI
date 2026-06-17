import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  constructor(private http: HttpClient) {}

  createOrder(userId: number, data: { items: any[] }): Observable<any> {
    return this.http.post<any>(`${environment.apiUrl}/order?userId=${userId}`, data);
  }

  getUserOrders(page: number = 1, pageSize: number = 10): Observable<any> {
    let userId = 0;
    if (typeof window !== 'undefined') {
      const userStr = localStorage.getItem('user');
      const user = userStr ? JSON.parse(userStr) : null;
      userId = user?.userId ?? user?.id ?? 0;
    }
    
    return this.http.get<any>(`${environment.apiUrl}/order/my-orders`, {
      params: { pageNumber: page.toString(), pageSize: pageSize.toString(), userId: userId.toString() }
    }).pipe(
      map(data => {
        const mapOrder = (dto: any) => {
          const itemsMapped = (dto.orderItems || dto.OrderItems || []).map((it: any) => ({
            productId: it.productId ?? it.ProductId ?? it.productId,
            productName: it.productName ?? it.ProductName ?? it.Product?.name ?? "",
            quantity: it.quantity ?? it.Quantity ?? 1,
            price: it.unitPrice ?? it.UnitPrice ?? it.price ?? 0,
            discount: it.discount ?? it.Discount ?? 0,
          }));
          const total = dto.totalAmount ?? dto.TotalAmount ?? dto.total ?? 0;
          const subtotal = itemsMapped.reduce((s: number, it: any) => s + (it.price * it.quantity - (it.discount || 0)), 0);
          return {
            id: dto.orderId ?? dto.OrderId ?? dto.id,
            userId: dto.userId ?? dto.UserId ?? 0,
            items: itemsMapped,
            subtotal,
            tax: dto.tax ?? dto.Tax ?? 0,
            shipping: dto.shipping ?? dto.Shipping ?? 0,
            total,
            status: (dto.status ?? dto.Status ?? "pending").toString().toLowerCase(),
            createdAt: new Date(dto.createdAt ?? dto.CreatedAt ?? dto.orderDate ?? dto.OrderDate ?? Date.now()),
            updatedAt: new Date(dto.updatedAt ?? dto.UpdatedAt ?? Date.now()),
          };
        };

        let items: any[] = [];
        if (Array.isArray(data)) items = data.map(mapOrder);
        else if (data && Array.isArray(data.items)) items = data.items.map(mapOrder);
        else if (data && Array.isArray(data.data)) items = data.data.map(mapOrder);
        else if (data) items = [mapOrder(data)];

        return {
          items,
          totalCount: items.length,
          page,
          pageSize,
          totalPages: 1,
        };
      })
    );
  }

  getOrder(id: number): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/order/${id}`);
  }
}
