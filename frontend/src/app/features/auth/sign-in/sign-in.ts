import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthStore } from '../../../core/auth/auth.store';

@Component({
  selector: 'et-sign-in',
  templateUrl: './sign-in.html',
  styleUrl: './sign-in.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignIn {
  readonly auth = inject(AuthStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly reason = this.route.snapshot.queryParamMap.get('reason');

  async signIn(): Promise<void> {
    if (!(await this.auth.signIn())) {
      return;
    }

    const requestedUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    const returnUrl =
      requestedUrl?.startsWith('/') && !requestedUrl.startsWith('//') ? requestedUrl : '/';
    await this.router.navigateByUrl(returnUrl);
  }
}
