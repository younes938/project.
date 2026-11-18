import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'et-access-denied',
  imports: [RouterLink],
  templateUrl: './access-denied.html',
  styleUrl: './access-denied.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccessDenied {}
