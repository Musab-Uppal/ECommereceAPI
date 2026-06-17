import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { OrderService } from '../../services/order';

@Component({
  selector: 'app-account',
  imports: [CommonModule, RouterModule],
  templateUrl: './account.html',
})
export class AccountComponent implements OnInit {
  authService = inject(AuthService);
  orderService = inject(OrderService);
  router = inject(Router);
  cdr = inject(ChangeDetectorRef);

  user: any = null;
  orders: any[] = [];
  recentOrders: any[] = [];

  STATUS_STYLES: Record<string, string> = {
    pending:   "bg-amber-50 text-amber-700 border-amber-200",
    shipped:   "bg-blue-50 text-blue-700 border-blue-200",
    delivered: "bg-emerald-50 text-emerald-700 border-emerald-200",
    cancelled: "bg-red-50 text-red-600 border-red-200",
  };

  get firstName() { return this.user?.firstName || this.user?.name?.split(" ")[0] || ""; }
  get lastName() { return this.user?.lastName || this.user?.name?.split(" ")[1] || ""; }
  get initials() { return [this.firstName[0], this.lastName[0]].filter(Boolean).join("").toUpperCase() || (this.user?.email?.[0] ?? "U").toUpperCase(); }
  get displayName() { return [this.firstName, this.lastName].filter(Boolean).join(" ") || this.user?.email || "Account"; }

  ngOnInit() {
    const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;
    const u = typeof window !== 'undefined' ? localStorage.getItem('user') : null;
    if (!token || !u) {
      this.router.navigate(['/login']);
      return;
    }

    const parsed = JSON.parse(u as string);
    this.user = parsed;

    const userId = parsed.userId ?? parsed.id;
    if (userId) {
      this.authService.getProfile(userId).subscribe({
        next: (profile) => { if (profile) this.user = profile; this.cdr.detectChanges(); },
        error: () => {}
      });
    }

    this.orderService.getUserOrders(1, 5).subscribe({
      next: (res) => {
        this.orders = res.items || [];
        this.recentOrders = [...this.orders]
          .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
          .slice(0, 4);
        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getStatusClass(status: string): string {
    const key = (status || "pending").toLowerCase();
    return this.STATUS_STYLES[key] || "bg-gray-50 text-gray-500 border-gray-200";
  }
}
