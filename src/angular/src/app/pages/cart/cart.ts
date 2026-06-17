import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CartService, CartResponse } from '../../services/cart';
import { OrderService } from '../../services/order';
import { ToastService } from '../../services/toast';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-cart',
  imports: [CommonModule, RouterModule],
  templateUrl: './cart.html',
})
export class CartComponent implements OnInit {
  cartService = inject(CartService);
  orderService = inject(OrderService);
  toastService = inject(ToastService);
  authService = inject(AuthService);
  router = inject(Router);
  cdr = inject(ChangeDetectorRef);

  cart: CartResponse | null = null;
  loading = true;
  updatingId: number | null = null;
  checkingOut = false;

  ngOnInit() {
    this.loadCart();
  }

  loadCart() {
    this.loading = true;
    this.cartService.getCart().subscribe({
      next: (cart) => { this.cart = cart; this.loading = false; this.cdr.detectChanges(); },
      error: () => { this.toastService.showToast("Failed to load cart.", 'error'); this.loading = false; this.cdr.detectChanges(); }
    });
  }

  update(item: any, qty: number) {
    if (qty < 1) { this.remove(item.productId); return; }
    this.updatingId = item.productId;
    this.cartService.updateItem(item.productId, qty).subscribe({
      next: (cart) => { this.cart = cart; this.updatingId = null; this.cdr.detectChanges(); },
      error: () => { this.toastService.showToast("Failed to update.", 'error'); this.updatingId = null; this.cdr.detectChanges(); }
    });
  }

  remove(id: number) {
    this.updatingId = id;
    this.cartService.removeItem(id).subscribe({
      next: (cart) => { this.cart = cart; this.toastService.showToast("Removed.", 'success'); this.updatingId = null; this.cdr.detectChanges(); },
      error: () => { this.toastService.showToast("Failed to remove.", 'error'); this.updatingId = null; this.cdr.detectChanges(); }
    });
  }

  clearCart() {
    this.cartService.clearCart().subscribe({
      next: () => { this.cart = { items: [], total: 0, totalItems: 0 }; this.toastService.showToast("Cart cleared.", 'success'); this.cdr.detectChanges(); },
      error: () => { this.toastService.showToast("Failed to clear.", 'error'); this.cdr.detectChanges(); }
    });
  }

  checkout() {
    const user = this.authService.currentUser;
    if (!user) {
      this.toastService.showToast("Please log in first.", 'error');
      this.router.navigate(['/login']);
      return;
    }
    const userId = user.id ?? (user as any).userId;
    const items = this.cart?.items || [];
    if (!items.length) return;

    this.checkingOut = true;
    this.orderService.createOrder(userId, { items: items.map(i => ({ productId: i.productId, quantity: i.quantity, discount: 0 })) }).subscribe({
      next: (order: any) => {
        const orderId = order.orderId || order.OrderId || order.id;
        this.router.navigate(['/checkout'], { queryParams: { orderId } });
        this.checkingOut = false;
        this.cdr.detectChanges();
      },
      error: (e) => {
        this.toastService.showToast(e?.message || "Checkout failed.", 'error');
        this.checkingOut = false;
        this.cdr.detectChanges();
      }
    });
  }
}
