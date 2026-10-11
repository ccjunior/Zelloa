import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Classroom, PagedResponse, SchoolApiService } from '../core/school-api.service';

interface OrderCutoffResponse {
  shift: string;
  openingTime: string | null;
  cutoffTime: string;
}

interface OrderCutoffRow {
  key: string;
  shift: string;
  form: FormGroup<{ openingTime: FormControl<string>; cutoffTime: FormControl<string> }>;
  isConfigured: boolean;
}

const validOrderWindow: ValidatorFn = control => {
  const openingTime = control.get('openingTime')?.value as string;
  const cutoffTime = control.get('cutoffTime')?.value as string;
  if (!openingTime || !cutoffTime) return null;
  return openingTime < cutoffTime ? null : { invalidWindow: true };
};

@Component({
  selector: 'app-order-cutoffs-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './order-cutoffs-page.html',
  styleUrl: './order-cutoffs-page.scss',
})
export class OrderCutoffsPage {
  private readonly api = inject(SchoolApiService);

  readonly rows = signal<OrderCutoffRow[]>([]);
  readonly loading = signal(false);
  readonly savingShift = signal<string | null>(null);
  readonly savedShift = signal<string | null>(null);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  constructor() {
    void this.load();
  }

  async load(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');

    try {
      const [configuredCutoffs, classrooms] = await Promise.all([
        this.api.get<OrderCutoffResponse[]>('/api/school/order-cutoffs'),
        this.loadClassrooms(),
      ]);

      const configuredByShift = new Map(
        configuredCutoffs.map(cutoff => [this.shiftKey(cutoff.shift), cutoff]),
      );
      const shifts = new Map<string, string>();

      for (const classroom of classrooms) {
        if (classroom.isActive && classroom.shift.trim()) {
          const key = this.shiftKey(classroom.shift);
          if (!shifts.has(key)) shifts.set(key, classroom.shift.trim());
        }
      }

      for (const cutoff of configuredCutoffs) {
        const key = this.shiftKey(cutoff.shift);
        if (!shifts.has(key)) shifts.set(key, cutoff.shift.trim());
      }

      this.rows.set(
        [...shifts.entries()]
          .sort(([, first], [, second]) => first.localeCompare(second, 'pt-BR'))
          .map(([key, shift]) => {
            const cutoff = configuredByShift.get(key);
            return {
              key,
              shift,
              form: new FormGroup({
                openingTime: new FormControl(this.timeInputValue(cutoff?.openingTime), {
                  nonNullable: true,
                  validators: [Validators.pattern(/^([01]\d|2[0-3]):[0-5]\d$/)],
                }),
                cutoffTime: new FormControl(this.timeInputValue(cutoff?.cutoffTime), {
                  nonNullable: true,
                  validators: [Validators.required, Validators.pattern(/^([01]\d|2[0-3]):[0-5]\d$/)],
                }),
              }, { validators: validOrderWindow }),
              isConfigured: cutoff !== undefined,
            };
          }),
      );
    } catch {
      this.errorMessage.set('Não foi possível carregar os turnos e horários. Tente novamente.');
    } finally {
      this.loading.set(false);
    }
  }

  async save(row: OrderCutoffRow): Promise<void> {
    row.form.markAllAsTouched();
    if (row.form.invalid || this.savingShift()) return;

    this.savingShift.set(row.key);
    this.savedShift.set(null);
    this.errorMessage.set('');
    this.successMessage.set('');

    try {
      const { openingTime, cutoffTime } = row.form.getRawValue();
      await this.api.put<OrderCutoffResponse>(
        `/api/school/order-cutoffs/${encodeURIComponent(row.shift)}`,
        { openingTime: openingTime || null, cutoffTime },
      );
      this.rows.update(rows => rows.map(item =>
        item.key === row.key ? { ...item, isConfigured: true } : item,
      ));
      this.savedShift.set(row.key);
      this.successMessage.set(openingTime
        ? `Janela do turno ${row.shift} salva: ${openingTime}–${cutoffTime}.`
        : `Limite final do turno ${row.shift} salvo: ${cutoffTime}.`);
    } catch {
      this.errorMessage.set(`Não foi possível salvar o horário do turno ${row.shift}. Confira a conexão e tente novamente.`);
    } finally {
      this.savingShift.set(null);
    }
  }

  clearFeedback(key: string): void {
    if (this.savedShift() === key) {
      this.savedShift.set(null);
      this.successMessage.set('');
    }
    this.errorMessage.set('');
  }

  private async loadClassrooms(): Promise<Classroom[]> {
    const pageSize = 100;
    const firstPage = await this.api.get<PagedResponse<Classroom>>(
      `/api/classrooms?page=1&pageSize=${pageSize}`,
    );
    const classrooms = [...firstPage.items];
    const pageCount = Math.ceil(firstPage.totalCount / pageSize);

    for (let page = 2; page <= pageCount; page += 1) {
      const response = await this.api.get<PagedResponse<Classroom>>(
        `/api/classrooms?page=${page}&pageSize=${pageSize}`,
      );
      classrooms.push(...response.items);
    }

    return classrooms;
  }

  private shiftKey(shift: string): string {
    return shift.trim().toLocaleLowerCase('pt-BR');
  }

  private timeInputValue(value?: string | null): string {
    return value?.slice(0, 5) ?? '';
  }
}
