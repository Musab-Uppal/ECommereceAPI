import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../services/product';
import { CartService } from '../../services/cart';
import { ToastService } from '../../services/toast';

@Component({
  selector: 'app-shop',
  imports: [CommonModule],
  templateUrl: './shop.html',
})
export class ShopComponent implements OnInit {
  productService = inject(ProductService);
  cartService = inject(CartService);
  toastService = inject(ToastService);
  cdr = inject(ChangeDetectorRef);

  products: any[] = [];
  loading = true;
  page = 1;
  totalPages = 1;
  addingId: number | null = null;

  ngOnInit() {
    this.load(1);
  }

  load(p = 1) {
    this.loading = true;
    console.log(`[SHOP] Calling getProducts for page ${p}`);
    this.productService.getProducts({ page: p, pageSize: 12 }).subscribe({
      next: (res) => {
        console.log('[SHOP] Received res.items:', res.items);
        this.products = p === 1 ? (res.items || []) : [...this.products, ...(res.items || [])];
        console.log('[SHOP] Assigned this.products, length is now:', this.products.length);
        this.totalPages = res.totalPages || 1;
        this.page = p;
        this.loading = false;
        this.cdr.detectChanges(); // Force UI update
      },
      error: (e) => {
        console.error('[SHOP] Error:', e);
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
