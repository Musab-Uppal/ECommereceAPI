import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { OrderService } from '../../services/order';
import { ToastService } from '../../services/toast';

@Component({
  selector: 'app-orders',
  imports: [CommonModule, RouterModule],
  templateUrl: './orders.html',
})
export class OrdersComponent implements OnInit {
  orderService = inject(OrderService);
  toastService = inject(ToastService);
  router = inject(Router);
  route = inject(ActivatedRoute);
  cdr = inject(ChangeDetectorRef);

  orders: any[] = [];
  loading = true;
  error: string | null = null;

  BADGE: Record<string, string> = {
    pending: "badge badge-pending",
    shipped: "badge badge-shipped",
    delivered: "badge badge-delivered",
    cancelled: "badge badge-cancelled",
    paid: "badge badge-success",
  };

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['payment_success'] === 'true') {
        this.toastService.showToast("Payment successful! Your order is confirmed. 🎉", 'success');
        this.router.navigate([], { queryParams: { payment_success: null }, queryParamsHandling: 'merge' });
      }
    });

    const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;
    const u = typeof window !== 'undefined' ? localStorage.getItem('user') : null;
    if (!token || !u) {
      this.router.navigate(['/login']);
      return;
    }

    this.orderService.getUserOrders(1, 50).subscribe({
      next: (res) => {
        this.orders = (res.items || []).sort((a: any, b: any) => 
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (e) => {
        this.error = e?.message || "Failed to load orders.";
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getBadgeClass(status: string): string {
    const key = (status || 'pending').toLowerCase();
    return this.BADGE[key] || 'badge';
  }

  getPrimaryItem(order: any): string {
    return order.items?.[0]?.productName || 'Order';
  }
}
