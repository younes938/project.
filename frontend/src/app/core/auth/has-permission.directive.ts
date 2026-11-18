import {
  Directive,
  effect,
  inject,
  Input,
  signal,
  TemplateRef,
  ViewContainerRef,
} from '@angular/core';
import { Permission } from '../../domain/permissions/permission';
import { AuthStore } from './auth.store';

@Directive({ selector: '[etHasPermission]' })
export class HasPermissionDirective {
  private readonly auth = inject(AuthStore);
  private readonly template = inject(TemplateRef<unknown>);
  private readonly container = inject(ViewContainerRef);
  private readonly permission = signal<Permission | null>(null);
  private rendered = false;

  @Input({ required: true })
  set etHasPermission(permission: Permission) {
    this.permission.set(permission);
  }

  constructor() {
    effect(() => {
      const permission = this.permission();
      const shouldRender = permission !== null && this.auth.hasPermission(permission);

      if (shouldRender && !this.rendered) {
        this.container.createEmbeddedView(this.template);
        this.rendered = true;
      } else if (!shouldRender && this.rendered) {
        this.container.clear();
        this.rendered = false;
      }
    });
  }
}
