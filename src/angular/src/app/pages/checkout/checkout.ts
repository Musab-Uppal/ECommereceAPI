import { Component, OnInit, ViewChild, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { StripeService, NgxStripeModule, StripePaymentElementComponent } from 'ngx-stripe';
import { environment } from '../../../environments/environment';
import { ToastService } from '../../services/toast';
import { CartService } from '../../services/cart';
import { StripeElementsOptions } from '@stripe/stripe-js';

@Component({
  selector: 'app-checkout',
  imports: [CommonModule, NgxStripeModule],
  templateUrl: './checkout.html',
})
export class CheckoutComponent implements OnInit {
  router = inject(Router);
  route = inject(ActivatedRoute);
  http = inject(HttpClient);
  stripeService = inject(StripeService);
  toastService = inject(ToastService);
  cartService = inject(CartService);
  cdr = inject(ChangeDetectorRef);

  @ViewChild(StripePaymentElementComponent) paymentElement!: StripePaymentElementComponent;

  clientSecret = '';
  error = '';
  isProcessing = false;

  elementsOptions: StripeElementsOptions = {
    appearance: { theme: 'stripe' }
  };

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const orderId = params['orderId'];
      if (orderId) {
        this.http.post<any>(`${environment.apiUrl}/payment/create-intent?orderId=${orderId}`, {}).subscribe({
          next: (res) => {
            try {
              if (!res || !res.clientSecret) {
                this.error = "Server did not return a Stripe clientSecret. Response: " + JSON.stringify(res);
                this.cdr.detectChanges();
                return;
              }
              this.elementsOptions = {
                appearance: { theme: 'stripe' },
                clientSecret: res.clientSecret
              };
              this.clientSecret = res.clientSecret;
              this.cdr.detectChanges();
            } catch (e: any) {
              this.error = "Frontend error processing payment: " + e.message;
              this.cdr.detectChanges();
            }
          },
          error: (err) => {
            console.error(err);
            this.error = "Failed to initialize payment. The order might already be paid or server returned an error: " + (err.message || '');
            this.cdr.detectChanges();
          }
        });
      } else {
        this.error = "No orderId provided in URL.";
        this.cdr.detectChanges();
      }
    });
  }

  pay() {
    if (this.isProcessing) return;
    this.isProcessing = true;

    this.cartService.clearCart().subscribe();

    this.stripeService.confirmPayment({
      elements: this.paymentElement.elements,
      confirmParams: {
        return_url: `${window.location.origin}/orders?payment_success=true`
      }
    }).subscribe({
      next: (result) => {
        if (result.error) {
          this.toastService.showToast(result.error.message || "An unexpected error occurred.", 'error');
          this.isProcessing = false;
        } else {
          this.router.navigate(['/orders'], { queryParams: { payment_success: 'true' } });
        }
      },
      error: (e) => {
        this.toastService.showToast(e.message || "An unexpected error occurred.", 'error');
        this.isProcessing = false;
      }
    });
  }
}
