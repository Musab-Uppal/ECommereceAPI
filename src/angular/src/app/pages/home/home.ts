import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProductService } from '../../services/product';
import { CartService } from '../../services/cart';
import { ToastService } from '../../services/toast';

@Component({
  selector: 'app-home',
  imports: [CommonModule, RouterModule],
  templateUrl: './home.html',
})
export class HomeComponent implements OnInit {
  productService = inject(ProductService);
  cartService = inject(CartService);
  toastService = inject(ToastService);
  cdr = inject(ChangeDetectorRef);

  products: any[] = [];
  loading = true;
  addingId: number | null = null;
  features = [
    { icon: "🚚", label: "Free Shipping", sub: "On orders over $50" },
    { icon: "↩️", label: "Easy Returns", sub: "30-day return policy" },
    { icon: "🔒", label: "Secure Payment", sub: "256-bit SSL" },
    { icon: "💬", label: "24/7 Support", sub: "We're always here" }
  ];

  ngOnInit() {
    console.log('[HOME] ngOnInit called, loading=', this.loading);
    this.productService.getProducts({ page: 1, pageSize: 8 }).subscribe({
      next: (r) => {
        console.log('[HOME] next() fired, items count=', r?.items?.length, r);
        this.products = r.items || [];
        this.loading = false;
        this.cdr.detectChanges(); // Force UI update
      },
      error: (err) => {
        console.error('[HOME] error() fired:', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  addToCart(productId: number) {
    this.addingId = productId;
    this.cartService.addToCart({ productId, quantity: 1 }).subscribe({
      next: () => {
        this.toastService.showToast("Added to cart.", 'success');
        this.addingId = null;
      },
      error: (e) => {
        this.toastService.showToast(e?.message || "Failed to add.", 'error');
        this.addingId = null;
      }
    });
  }
}
