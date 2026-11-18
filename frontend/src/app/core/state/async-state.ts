import { ApplicationError } from '../errors/application-error';

export type AsyncState<T> =
  | { readonly status: 'idle' }
  | { readonly status: 'loading' }
  | { readonly status: 'success'; readonly data: T }
  | { readonly status: 'error'; readonly error: ApplicationError };

export function idleState<T>(): AsyncState<T> {
  return { status: 'idle' };
}
