export interface ApplicationError {
  readonly code: string;
  readonly userMessage: string;
  readonly cause?: unknown;
}

export function toApplicationError(error: unknown): ApplicationError {
  return {
    code: 'unexpected_error',
    userMessage: 'An unexpected error occurred. Please try again.',
    cause: error,
  };
}
