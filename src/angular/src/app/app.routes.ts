import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home';
import { ShopComponent } from './pages/shop/shop';
import { CartComponent } from './pages/cart/cart';
import { CheckoutComponent } from './pages/checkout/checkout';
import { OrdersComponent } from './pages/orders/orders';
import { LoginComponent } from './pages/login/login';
import { SignupComponent } from './pages/signup/signup';
import { AccountComponent } from './pages/account/account';
import { AdminComponent } from './pages/admin/admin';
import { authGuard } from './guards/auth-guard';
import { adminGuard } from './guards/admin-guard';
import { customerGuard } from './guards/customer-guard';

export const routes: Routes = [
  { path: '', component: HomeComponent, canActivate: [customerGuard] },
  { path: 'shop', component: ShopComponent, canActivate: [customerGuard] },
  { path: 'cart', component: CartComponent, canActivate: [customerGuard] },
  { path: 'checkout', component: CheckoutComponent, canActivate: [authGuard, customerGuard] },
  { path: 'orders', component: OrdersComponent, canActivate: [authGuard, customerGuard] },
  { path: 'account', component: AccountComponent, canActivate: [authGuard, customerGuard] },
  { path: 'admin', component: AdminComponent, canActivate: [adminGuard] },
  { path: 'login', component: LoginComponent },
  { path: 'signup', component: SignupComponent },
  { path: '**', redirectTo: 'login' }
];
