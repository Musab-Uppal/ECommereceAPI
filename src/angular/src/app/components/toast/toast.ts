import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../services/toast';

@Component({
  selector: 'app-toast',
  imports: [CommonModule],
  templateUrl: './toast.html',
})
export class Toast {
  public toastService = inject(ToastService);

  getVariantClass(variant: string): string {
    switch(variant) {
      case 'success': return 'border-primary text-on-surface';
      case 'error': return 'border-error text-on-error-container';
      default: return 'border-outline-variant text-on-surface';
    }
  }
}
