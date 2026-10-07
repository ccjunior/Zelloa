import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Classroom, PagedResponse, SchoolApiService, Student } from '../core/school-api.service';

@Component({ selector: 'app-students-page', imports: [ReactiveFormsModule], templateUrl: './students-page.html', styleUrl: './school-pages.scss' })
export class StudentsPage {
  private readonly api = inject(SchoolApiService);
  private readonly fb = inject(FormBuilder);
  readonly form = this.fb.nonNullable.group({ name: ['', [Validators.required, Validators.maxLength(200)]], classroomId: ['', Validators.required] });
  readonly students = signal<Student[]>([]);
  readonly classrooms = signal<Classroom[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  constructor() { void this.load(); }
  async load(): Promise<void> {
    this.loading.set(true);
    try {
      const [students, classrooms] = await Promise.all([
        this.api.get<PagedResponse<Student>>('/api/students?page=1&pageSize=100'),
        this.api.get<PagedResponse<Classroom>>('/api/classrooms?page=1&pageSize=100'),
      ]);
      this.students.set(students.items); this.classrooms.set(classrooms.items); this.errorMessage.set('');
    } catch { this.errorMessage.set('Não foi possível carregar os alunos e as turmas.'); }
    finally { this.loading.set(false); }
  }
  async submit(): Promise<void> {
    this.form.markAllAsTouched(); if (this.form.invalid || this.saving()) return;
    this.saving.set(true); this.errorMessage.set(''); this.successMessage.set('');
    try { await this.api.post('/api/students', this.form.getRawValue()); this.successMessage.set('Aluno cadastrado e vinculado à turma.'); this.form.reset({ name: '', classroomId: '' }); await this.load(); }
    catch { this.errorMessage.set('Não foi possível cadastrar o aluno. Confira os dados e tente novamente.'); }
    finally { this.saving.set(false); }
  }
}
