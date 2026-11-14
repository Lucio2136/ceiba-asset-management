import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import {
  LucideActivity,
  LucideBoxes,
  LucideBuilding2,
  LucideLogOut,
  LucideMapPin,
  LucideMenu,
  LucideMonitorSmartphone,
  LucideUser,
  LucideUsers,
} from '@lucide/angular';
import { map } from 'rxjs';
import { AuthService } from '../../authentication/auth.service';
import { NotificationService } from '../../../shared/services/notification.service';

interface NavItem {
  path: string;
  label: string;
}

/**
 * Estructura principal: barra superior, navegación lateral y área de contenido.
 * En pantallas pequeñas la navegación se superpone y se abre bajo demanda.
 * La navegación y el usuario solo se muestran con sesión iniciada.
 */
@Component({
  selector: 'ceiba-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatButtonModule,
    MatListModule,
    MatSidenavModule,
    MatToolbarModule,
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    LucideActivity,
    LucideBoxes,
    LucideBuilding2,
    LucideLogOut,
    LucideMapPin,
    LucideMenu,
    LucideMonitorSmartphone,
    LucideUser,
    LucideUsers,
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);
  protected readonly auth = inject(AuthService);

  protected readonly isCompact = toSignal(
    inject(BreakpointObserver)
      .observe([Breakpoints.XSmall, Breakpoints.Small])
      .pipe(map((state) => state.matches)),
    { initialValue: false },
  );

  protected readonly navItems: NavItem[] = [
    { path: '/assets', label: 'Activos' },
    { path: '/buildings', label: 'Edificios' },
    { path: '/locations', label: 'Ubicaciones' },
    { path: '/people', label: 'Responsables' },
    { path: '/status', label: 'Estado de la API' },
  ];

  protected logout(): void {
    const goToLogin = () => void this.router.navigate(['/login']);
    this.auth.logout().subscribe({
      next: goToLogin,
      error: (error: unknown) => {
        // La sesión local ya se descartó; se informa del fallo del servidor y se sale igual.
        this.notifications.error(error);
        goToLogin();
      },
    });
  }
}
