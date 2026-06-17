import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  let token: string | null = null;
  if (typeof window !== 'undefined') {
    token = localStorage.getItem('token');
  }

  // Only send withCredentials (session cookies) for the cart endpoint,
  // which is session-based. Sending withCredentials on ALL requests
  // forces CORS preflight on every call and can block non-cart responses.
  const isCartRequest = req.url.includes('/api/cart');

  let newReq = req.clone({ withCredentials: isCartRequest });

  if (token) {
    newReq = newReq.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  console.log(`[INTERCEPTOR] ${req.method} ${req.url} | token=${!!token} | withCredentials=${isCartRequest}`);
  return next(newReq);
};
