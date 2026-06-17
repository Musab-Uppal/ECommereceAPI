import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-login',
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './login.html',
})
export class LoginComponent {
  authService = inject(AuthService);
  router = inject(Router);

  email = '';
  password = '';
  loading = false;
  error: string | null = null;

  submit() {
    this.error = null;
    this.loading = true;

    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: (res) => {
        if (res?.token) {
          const role = res.user?.role || "Customer";
          this.router.navigate([role.toLowerCase() === "admin" ? "/admin" : "/shop"]);
        } else {
          this.error = res?.message || "Invalid credentials.";
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
