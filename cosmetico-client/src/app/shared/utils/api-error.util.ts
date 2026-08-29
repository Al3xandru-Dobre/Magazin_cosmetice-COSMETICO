import { HttpErrorResponse } from '@angular/common/http';

/// API-ul intoarce ProblemDetails (detail) sau ValidationProblemDetails (errors).
export function extractApiError(err: unknown): string {
  const httpErr = err as HttpErrorResponse;
  if (!httpErr) return 'A aparut o eroare necunoscuta.';

  if (httpErr.status === 0) return 'Serverul nu raspunde. Este pornit API-ul? (docker compose up)';

  const body = httpErr.error;
  if (body?.detail) return body.detail as string;
  if (body?.errors) return Object.values(body.errors as Record<string, string[]>).flat().join(' ');
  if (body?.message) return body.message as string;
  return `Eroare ${httpErr.status}: ${httpErr.statusText}`;
}
