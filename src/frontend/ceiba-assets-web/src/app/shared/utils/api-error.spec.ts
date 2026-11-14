import { HttpErrorResponse } from '@angular/common/http';
import { describeError } from './api-error';

describe('describeError', () => {
  it.each([
    [401, 'unauthorized'],
    [403, 'forbidden'],
    [404, 'notFound'],
    [409, 'conflict'],
    [422, 'validation'],
    [0, 'network'],
    [500, 'server'],
  ])('clasifica el estado HTTP %i como %s', (status, kind) => {
    expect(describeError(new HttpErrorResponse({ status })).kind).toBe(kind);
  });

  it('usa el detalle del Problem Details en conflictos', () => {
    const error = new HttpErrorResponse({
      status: 409,
      error: { title: 'Conflict', detail: 'El número de serie ya existe.' },
    });

    expect(describeError(error).message).toBe('El número de serie ya existe.');
  });

  it('muestra el primer error de validación', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { errors: { Name: ['El nombre es obligatorio.'] } },
    });

    expect(describeError(error).message).toBe('El nombre es obligatorio.');
  });

  it('desenvuelve el error original de un recurso de Angular', () => {
    const wrapped = new Error('wrapper', { cause: new HttpErrorResponse({ status: 403 }) });

    expect(describeError(wrapped).kind).toBe('forbidden');
  });
});
