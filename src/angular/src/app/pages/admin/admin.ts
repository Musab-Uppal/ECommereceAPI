import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { AdminService } from '../../services/admin';

@Component({
  selector: 'app-admin',
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin.html',
})
export class AdminComponent implements OnInit {
  authService = inject(AuthService);
  adminService = inject(AdminService);
  router = inject(Router);
  cdr = inject(ChangeDetectorRef);

  activeTab: 'dashboard' | 'products' | 'orders' | 'users' | 'categories' = 'dashboard';

  // Dashboard
  stats: any = null;
  statsLoading = true;

  // Products
  products: any[] = [];
  productsLoading = false;
  productPage = 1;
  productTotalPages = 1;
  showProductForm = false;
  editingProduct: any = null;
  productForm: any = { name: '', description: '', price: 0, stock: 0, imageUrl: '', categoryId: 0 };
  productSaving = false;

  // Categories
  categories: any[] = [];
  showCategoryForm = false;
  editingCategory: any = null;
  categoryForm: any = { name: '', description: '' };
  categorySaving = false;

  // Orders
  orders: any[] = [];
  ordersLoading = false;
  orderPage = 1;
  orderTotalPages = 1;
  orderStatusFilter = '';
  updatingOrderId: number | null = null;

  // Users
  users: any[] = [];
  usersLoading = false;
  userPage = 1;
  userTotalPages = 1;
  updatingUserId: number | null = null;

  // Toast
  toast: { msg: string; type: 'success' | 'error' } | null = null;

  ngOnInit() {
    this.loadDashboard();
    this.loadCategories();
  }

  // ─── Navigation ───────────────────────────────────────────────
  setTab(tab: typeof this.activeTab) {
    this.activeTab = tab;
    if (tab === 'products' && !this.products.length) this.loadProducts();
    if (tab === 'orders' && !this.orders.length) this.loadOrders();
    if (tab === 'users' && !this.users.length) this.loadUsers();
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  // ─── Toast ────────────────────────────────────────────────────
  showToast(msg: string, type: 'success' | 'error') {
    this.toast = { msg, type };
    setTimeout(() => { this.toast = null; this.cdr.detectChanges(); }, 3500);
    this.cdr.detectChanges();
  }

  // ─── Dashboard ────────────────────────────────────────────────
  loadDashboard() {
    this.statsLoading = true;
    this.adminService.getDashboard().subscribe({
      next: (res: any) => { this.stats = res.data; this.statsLoading = false; this.cdr.detectChanges(); },
      error: () => { this.statsLoading = false; this.cdr.detectChanges(); }
    });
  }

  // ─── Products ─────────────────────────────────────────────────
  loadProducts(page = 1) {
    this.productsLoading = true;
    this.adminService.getProducts(page, 10).subscribe({
      next: (res: any) => {
        this.products = res.data?.items || [];
        this.productTotalPages = res.data?.totalPages || 1;
        this.productPage = page;
        this.productsLoading = false;
        this.cdr.detectChanges();
      },
      error: () => { this.productsLoading = false; this.cdr.detectChanges(); }
    });
  }

  openNewProduct() {
    this.editingProduct = null;
    this.productForm = { name: '', description: '', price: 0, stock: 0, imageUrl: '', categoryId: this.categories[0]?.categoryId || 0 };
    this.showProductForm = true;
  }

  openEditProduct(p: any) {
    this.editingProduct = p;
    this.productForm = { name: p.name, description: p.description, price: p.price, stock: p.stock, imageUrl: p.imageUrl || '', categoryId: p.categoryId };
    this.showProductForm = true;
  }

  saveProduct() {
    this.productSaving = true;
    const obs = this.editingProduct
      ? this.adminService.updateProduct(this.editingProduct.productId, this.productForm)
      : this.adminService.createProduct(this.productForm);
    obs.subscribe({
      next: () => {
        this.showToast(this.editingProduct ? 'Product updated!' : 'Product created!', 'success');
        this.showProductForm = false;
        this.productSaving = false;
        this.loadProducts(this.productPage);
      },
      error: (e: any) => { this.showToast(e?.error?.message || 'Failed to save.', 'error'); this.productSaving = false; this.cdr.detectChanges(); }
    });
  }

  deleteProduct(id: number) {
    if (!confirm('Delete this product?')) return;
    this.adminService.deleteProduct(id).subscribe({
      next: () => { this.showToast('Product deleted.', 'success'); this.loadProducts(this.productPage); },
      error: () => this.showToast('Failed to delete.', 'error')
    });
  }

  addStock(p: any) {
    const qty = parseInt(prompt('Add how many units?') || '0', 10);
    if (!qty || qty < 1) return;
    this.adminService.addStock(p.productId, qty).subscribe({
      next: () => { this.showToast(`Added ${qty} units.`, 'success'); this.loadProducts(this.productPage); },
      error: () => this.showToast('Failed to add stock.', 'error')
    });
  }

  // ─── Categories ───────────────────────────────────────────────
  loadCategories() {
    this.adminService.getCategories().subscribe({
      next: (res: any) => { this.categories = res.data || []; this.cdr.detectChanges(); },
      error: () => {}
    });
  }

  openNewCategory() {
    this.editingCategory = null;
    this.categoryForm = { name: '', description: '' };
    this.showCategoryForm = true;
  }

  openEditCategory(c: any) {
    this.editingCategory = c;
    this.categoryForm = { name: c.name, description: c.description || '' };
    this.showCategoryForm = true;
  }

  saveCategory() {
    this.categorySaving = true;
    const obs = this.editingCategory
      ? this.adminService.updateCategory(this.editingCategory.categoryId, this.categoryForm)
      : this.adminService.createCategory(this.categoryForm);
    obs.subscribe({
      next: () => {
        this.showToast(this.editingCategory ? 'Category updated!' : 'Category created!', 'success');
        this.showCategoryForm = false;
        this.categorySaving = false;
        this.loadCategories();
      },
      error: (e: any) => { this.showToast(e?.error?.message || 'Failed to save.', 'error'); this.categorySaving = false; this.cdr.detectChanges(); }
    });
  }

  deleteCategory(id: number) {
    if (!confirm('Delete this category?')) return;
    this.adminService.deleteCategory(id).subscribe({
      next: () => { this.showToast('Category deleted.', 'success'); this.loadCategories(); },
      error: () => this.showToast('Cannot delete — it may have products.', 'error')
    });
  }

  // ─── Orders ───────────────────────────────────────────────────
  loadOrders(page = 1) {
    this.ordersLoading = true;
    this.adminService.getOrders(page, 10, this.orderStatusFilter || undefined).subscribe({
      next: (res: any) => {
        this.orders = res.data?.items || [];
        this.orderTotalPages = res.data?.totalPages || 1;
        this.orderPage = page;
        this.ordersLoading = false;
        this.cdr.detectChanges();
      },
      error: () => { this.ordersLoading = false; this.cdr.detectChanges(); }
    });
  }

  updateOrderStatus(order: any, status: string) {
    this.updatingOrderId = order.orderId;
    this.adminService.updateOrderStatus(order.orderId, status).subscribe({
      next: () => {
        this.showToast('Status updated!', 'success');
        order.status = status;
        this.updatingOrderId = null;
        this.cdr.detectChanges();
      },
      error: () => { this.showToast('Failed to update status.', 'error'); this.updatingOrderId = null; this.cdr.detectChanges(); }
    });
  }

  // ─── Users ────────────────────────────────────────────────────
  loadUsers(page = 1) {
    this.usersLoading = true;
    this.adminService.getUsers(page, 10).subscribe({
      next: (res: any) => {
        this.users = res.data?.items || [];
        this.userTotalPages = res.data?.totalPages || 1;
        this.userPage = page;
        this.usersLoading = false;
        this.cdr.detectChanges();
      },
      error: () => { this.usersLoading = false; this.cdr.detectChanges(); }
    });
  }

  toggleUserRole(u: any) {
    const newRole = u.role?.toLowerCase() === 'admin' ? 'Customer' : 'Admin';
    if (!confirm(`Change ${u.email} to ${newRole}?`)) return;
    this.updatingUserId = u.userId;
    this.adminService.changeUserRole(u.userId, newRole).subscribe({
      next: () => {
        this.showToast(`Role changed to ${newRole}.`, 'success');
        u.role = newRole;
        this.updatingUserId = null;
        this.cdr.detectChanges();
      },
      error: () => { this.showToast('Failed to change role.', 'error'); this.updatingUserId = null; this.cdr.detectChanges(); }
    });
  }

  // ─── Helpers ──────────────────────────────────────────────────
  statusColor(status: string) {
    const s = status?.toLowerCase();
    if (s === 'pending') return 'bg-amber-100 text-amber-700';
    if (s === 'shipped') return 'bg-blue-100 text-blue-700';
    if (s === 'delivered') return 'bg-emerald-100 text-emerald-700';
    if (s === 'cancelled') return 'bg-red-100 text-red-600';
    if (s === 'paid') return 'bg-violet-100 text-violet-700';
    return 'bg-gray-100 text-gray-600';
  }
}
