import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, UserAuthDto } from '../models/types';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private userSubject = new BehaviorSubject<UserAuthDto | null>(null);
  public user$ = this.userSubject.asObservable();
  
  private tokenKey = 'token';
  private userKey = 'user';

  constructor(private http: HttpClient) {
    this.loadAuthFromStorage();
  }

  private loadAuthFromStorage() {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem(this.tokenKey);
      const userStr = localStorage.getItem(this.userKey);
      if (token && userStr) {
        try {
          this.userSubject.next(JSON.parse(userStr));
        } catch (e) {
          this.logout();
        }
      }
    }
  }

  public get currentUser(): UserAuthDto | null {
    return this.userSubject.value;
  }

  public get isAdmin(): boolean {
    return this.currentUser?.role?.toLowerCase() === 'admin';
  }

  public get isAdmin$(): Observable<boolean> {
    return this.user$.pipe(
      map(user => user?.role?.toLowerCase() === 'admin')
    );
  }

  public getToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(this.tokenKey);
    }
    return null;
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/user/login`, credentials).pipe(
      tap(response => {
        if (response.success && response.token && response.user) {
          localStorage.setItem(this.tokenKey, response.token);
          localStorage.setItem(this.userKey, JSON.stringify(response.user));
          this.userSubject.next(response.user);
        }
      })
    );
  }

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/user/register`, data).pipe(
      tap(response => {
        if (response.success && response.token && response.user) {
          localStorage.setItem(this.tokenKey, response.token);
          localStorage.setItem(this.userKey, JSON.stringify(response.user));
          this.userSubject.next(response.user);
        }
      })
    );
  }

  getProfile(userId: number): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/user/profile`, { params: { userId: userId.toString() } });
  }

  updateProfile(userId: number, data: any): Observable<any> {
    return this.http.put<any>(`${environment.apiUrl}/user/profile`, data, { params: { userId: userId.toString() } });
  }

  logout() {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.userKey);
    }
    this.userSubject.next(null);
  }
}
