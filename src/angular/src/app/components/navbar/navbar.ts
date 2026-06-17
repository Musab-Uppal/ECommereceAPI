import { Component, HostListener, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-navbar',
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar implements OnInit {
  authService = inject(AuthService);
  userLabel: string | null = null;
  isAdmin = false;
  scrolled = false;

  ngOnInit() {
    this.authService.user$.subscribe(user => {
      if (!user) {
        this.userLabel = null;
        this.isAdmin = false;
      } else {
        this.isAdmin = user.role?.toLowerCase() === 'admin';
        const nameParts = [(user as any).firstName, (user as any).lastName]
          .map((v: any) => (typeof v === "string" ? v.trim() : ""))
          .filter(Boolean);
        this.userLabel = nameParts.join(" ") || user.email || user.name || "Account";
      }
    });
  }

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.scrolled = window.scrollY > 20;
  }
}
