export type ApiErrorKind =
  'unauthorized' | 'forbidden' | 'notFound' | 'conflict' | 'validation' | 'network' | 'server';

export interface DescribedError {
  kind: ApiErrorKind;
  title: string;
  message: string;
}

interface ProblemDetailsLike {
  title?: unknown;
  detail?: unknown;
  errors?: Record<string, unknown>;
}

/** Los recursos de Angular envuelven errores no estándar; el original viaja en `cause`. */
function unwrapError(error: unknown): unknown {
  if (
    error instanceof Error &&
    statusOf(error) === undefined &&
    'cause' in error &&
    error.cause !== undefined
  ) {
    return error.cause;
  }
  return error;
}

function statusOf(error: unknown): number | undefined {
  if (typeof error === 'object' && error !== null && 'status' in error) {
    const status = (error as { status: unknown }).status;
    return typeof status === 'number' ? status : undefined;
  }
  return undefined;
}

/** Extrae el detalle útil de un Problem Details (HttpErrorResponse o ApiException de NSwag). */
function problemOf(error: unknown): ProblemDetailsLike | undefined {
  if (typeof error !== 'object' || error === null) {
    return undefined;
  }
  const body = (error as { error?: unknown; response?: unknown }).error;
  if (typeof body === 'object' && body !== null) {
    return body as ProblemDetailsLike;
  }
  const raw = (error as { response?: unknown }).response;
  if (typeof raw === 'string') {
    try {
      return JSON.parse(raw) as ProblemDetailsLike;
    } catch {
      return undefined;
    }
  }
  return undefined;
}

function firstValidationMessage(problem: ProblemDetailsLike | undefined): string | undefined {
  const errors = problem?.errors;
  if (!errors) {
    return undefined;
  }
  for (const value of Object.values(errors)) {
    if (Array.isArray(value) && typeof value[0] === 'string') {
      return value[0];
    }
  }
  return undefined;
}

/**
 * Traduce cualquier error HTTP a un mensaje en español para la interfaz, sin exponer detalles
 * internos (trazas, SQL, etc.).
 */
export function describeError(rawError: unknown): DescribedError {
  const error = unwrapError(rawError);
  const status = statusOf(error);
  const problem = problemOf(error);
  const detail = typeof problem?.detail === 'string' ? problem.detail : undefined;

  switch (status) {
    case 401:
      return {
        kind: 'unauthorized',
        title: 'Sesión no válida',
        message: 'Tu sesión expiró o no has iniciado sesión. Vuelve a iniciar sesión.',
      };
    case 403:
      return {
        kind: 'forbidden',
        title: 'Acceso denegado',
        message: 'No tienes permisos para realizar esta acción.',
      };
    case 404:
      return {
        kind: 'notFound',
        title: 'No encontrado',
        message: detail ?? 'El recurso solicitado no existe o fue eliminado.',
      };
    case 409:
      return {
        kind: 'conflict',
        title: 'Conflicto',
        message: detail ?? 'La operación entra en conflicto con el estado actual del recurso.',
      };
    case 400:
    case 422:
      return {
        kind: 'validation',
        title: 'Datos no válidos',
        message:
          firstValidationMessage(problem) ?? detail ?? 'Revisa los datos e inténtalo de nuevo.',
      };
    case 0:
    case undefined:
      return {
        kind: 'network',
        title: 'Sin conexión con la API',
        message: 'No fue posible contactar la API. Verifica que el servicio esté disponible.',
      };
    default:
      return {
        kind: 'server',
        title: 'Error del servidor',
        message: 'Ocurrió un error inesperado. Inténtalo de nuevo en unos momentos.',
      };
  }
}
