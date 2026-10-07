import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Classroom, PagedResponse, SchoolApiService } from '../core/school-api.service';

@Component({ selector: 'app-classrooms-page', imports: [ReactiveFormsModule], templateUrl: './classrooms-page.html', styleUrl: './school-pages.scss' })
export class ClassroomsPage {
  private readonly api = inject(SchoolApiService);
  private readonly fb = inject(FormBuilder);
  readonly form = this.fb.nonNullable.group({ name: ['', [Validators.required, Validators.maxLength(120)]], level: ['', [Validators.required, Validators.maxLength(100)]], shift: ['Manhã', Validators.required], academicYear: [new Date().getFullYear(), [Validators.required, Validators.min(2000), Validators.max(2100)]] });
  readonly classrooms = signal<Classroom[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  constructor() { void this.load(); }
  async load(): Promise<void> {
    this.loading.set(true);
    try { this.classrooms.set((await this.api.get<PagedResponse<Classroom>>('/api/classrooms?page=1&pageSize=100')).items); this.errorMessage.set(''); }
    catch { this.errorMessage.set('Não foi possível carregar as turmas.'); }
    finally { this.loading.set(false); }
  }
  async submit(): Promise<void> {
    this.form.markAllAsTouched(); if (this.form.invalid || this.saving()) return;
    this.saving.set(true); this.errorMessage.set(''); this.successMessage.set('');
    try { await this.api.post('/api/classrooms', this.form.getRawValue()); this.successMessage.set('Turma cadastrada com sucesso.'); this.form.reset({ name: '', level: '', shift: 'Manhã', academicYear: new Date().getFullYear() }); await this.load(); }
    catch { this.errorMessage.set('Não foi possível cadastrar a turma. Confira os dados e tente novamente.'); }
    finally { this.saving.set(false); }
  }
}
