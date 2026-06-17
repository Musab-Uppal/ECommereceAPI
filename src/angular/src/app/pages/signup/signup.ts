import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-signup',
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './signup.html',
})
export class SignupComponent {
  authService = inject(AuthService);
  router = inject(Router);

  form = {
    firstName: "",
    lastName: "",
    email: "",
    phone: "",
    address: "",
    password: "",
    confirmPassword: ""
  };
  loading = false;
  error: string | null = null;

  submit() {
    this.error = null;
    if (this.form.password !== this.form.confirmPassword) {
      this.error = "Passwords do not match.";
      return;
    }
    this.loading = true;

    this.authService.register(this.form).subscribe({
      next: (res) => {
        if (res?.token) {
          this.router.navigate(['/shop']);
        } else {
          this.error = res?.message || "Registration failed.";
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.message || "Something went wrong.";
        this.loading = false;
      }
    });
  }
}
