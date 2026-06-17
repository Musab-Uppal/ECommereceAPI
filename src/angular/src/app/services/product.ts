import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { PaginatedResponse, Product, ProductFilter } from '../models/types';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  constructor(private http: HttpClient) {}

  getProducts(filter?: ProductFilter): Observable<PaginatedResponse<Product>> {
    let params = new HttpParams();
    if (filter) {
      if (filter.category) params = params.set('category', filter.category);
      if (filter.searchTerm) params = params.set('searchTerm', filter.searchTerm);
      if (filter.minPrice) params = params.set('minPrice', filter.minPrice.toString());
      if (filter.maxPrice) params = params.set('maxPrice', filter.maxPrice.toString());
      if (filter.page) params = params.set('pageNumber', filter.page.toString());
      if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());
    }
    console.log('[PRODUCT-SVC] Calling API:', `${environment.apiUrl}/product`);
    return this.http.get<any>(`${environment.apiUrl}/product`, { params }).pipe(
      map(data => {
        console.log('[PRODUCT-SVC] Raw API response:', data);
        const mapProduct = (p: any): Product => ({
          ...p,
          id: p.productId ?? p.id,
          image: p.imageUrl || p.image || `https://picsum.photos/seed/product${p.productId ?? p.id}/600/400`,
          price: Number(p.price) || 0,
          inStock: (p.stock ?? 0) > 0,
        });

        if (Array.isArray(data)) {
          const mapped = data.map(mapProduct);
          console.log('[PRODUCT-SVC] Mapped array:', mapped);
          return { items: mapped, totalCount: data.length, page: 1, pageSize: 12, totalPages: 1 };
        } else if (data && Array.isArray(data.value)) {
          // Backend OData-style response: { value: [...], Count: N }
          const items = data.value.map(mapProduct);
          console.log('[PRODUCT-SVC] Mapped value array:', items);
          return { items, totalCount: data.Count ?? items.length, page: 1, pageSize: items.length, totalPages: 1 };
        } else if (data && Array.isArray(data.items)) {
          console.log('[PRODUCT-SVC] Mapped items array:', data.items.map(mapProduct));
          return { ...data, items: data.items.map(mapProduct) };
        }
        console.warn('[PRODUCT-SVC] Unrecognized shape, returning empty:', data);
        return { items: [], totalCount: 0, page: 1, pageSize: 12, totalPages: 0 };
      }),
      catchError((err) => {
        console.error('[PRODUCT-SVC] HTTP Error caught:', err?.status, err?.message, err);
        return of({ items: [], totalCount: 0, page: 1, pageSize: 12, totalPages: 0 });
      })
    );
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<any>(`${environment.apiUrl}/product/${id}`).pipe(
      map(p => ({
        ...p,
        id: p.productId ?? p.id,
        image: p.imageUrl || p.image || `https://picsum.photos/seed/product${p.productId ?? p.id}/600/400`,
        price: Number(p.price) || 0,
        inStock: (p.stock ?? 0) > 0,
      }))
    );
  }
}
