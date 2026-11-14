import { inject, Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { describeError } from '../utils/api-error';

/** Avisos efímeros (MatSnackBar) en español. */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.snackBar.open(message, 'Cerrar', { duration: 4000 });
  }

  error(error: unknown, fallback?: string): void {
    const described = describeError(error);
    this.snackBar.open(fallback ?? described.message, 'Cerrar', {
      duration: 7000,
      politeness: 'assertive',
    });
  }
}
