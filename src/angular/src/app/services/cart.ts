import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AddToCartRequest } from '../models/types';

export type CartResponse = {
  items: Array<{
    productId: number;
    name?: string;
    price: number;
    quantity: number;
  }>;
  total: number;
  totalItems: number;
};

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartSubject = new BehaviorSubject<CartResponse | null>(null);
  public cart$ = this.cartSubject.asObservable();

  constructor(private http: HttpClient) {}

  getCart(): Observable<CartResponse> {
    return this.http.get<CartResponse>(`${environment.apiUrl}/cart`).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  addToCart(data: AddToCartRequest): Observable<CartResponse> {
    return this.http.post<CartResponse>(`${environment.apiUrl}/cart`, data).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  updateItem(productId: number, quantity: number): Observable<CartResponse> {
    return this.http.put<CartResponse>(`${environment.apiUrl}/cart/${productId}`, { quantity }).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  removeItem(productId: number): Observable<CartResponse> {
    return this.http.delete<CartResponse>(`${environment.apiUrl}/cart/${productId}`).pipe(
      tap(cart => this.cartSubject.next(cart))
    );
  }

  clearCart(): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/cart/clear`).pipe(
      tap(() => this.cartSubject.next(null))
    );
  }
}
