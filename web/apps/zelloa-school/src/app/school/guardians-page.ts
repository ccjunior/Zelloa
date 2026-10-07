import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Classroom, GuardianInvitation, PagedResponse, SchoolApiService, Student } from '../core/school-api.service';

@Component({ selector: 'app-guardians-page', imports: [ReactiveFormsModule, DatePipe], templateUrl: './guardians-page.html', styleUrls: ['./school-pages.scss', './guardians-page.scss'] })
export class GuardiansPage {
  private readonly api = inject(SchoolApiService);
  private readonly fb = inject(FormBuilder);
  readonly form = this.fb.nonNullable.group({ displayName: ['', [Validators.required, Validators.maxLength(200)]], email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]], studentId: ['', Validators.required] });
  readonly students = signal<Student[]>([]);
  readonly invitation = signal<GuardianInvitation | null>(null);
  readonly pendingStudentId = signal('');
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly copied = signal(false);
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
      const roomNames = new Map(classrooms.items.map(item => [item.id, item.name]));
      this.students.set(students.items.map(student => ({ ...student, classroomName: student.classroomName || roomNames.get(student.classroomId) || 'Turma' })));
    } catch { this.errorMessage.set('Não foi possível carregar os alunos para o vínculo.'); }
    finally { this.loading.set(false); }
  }
  async submit(): Promise<void> {
    this.form.markAllAsTouched(); if (this.form.invalid || this.saving()) return;
    this.saving.set(true); this.errorMessage.set(''); this.successMessage.set(''); this.invitation.set(null);
    const { displayName, email, studentId } = this.form.getRawValue();
    try {
      const invite = await this.api.post<GuardianInvitation>('/api/guardians', { displayName, email });
      this.invitation.set(invite);
      this.pendingStudentId.set(studentId);
      try {
        await this.linkInvitation();
        this.successMessage.set('Responsável convidado e vinculado ao aluno.');
      } catch {
        this.errorMessage.set('O convite foi criado, mas não foi possível concluir o vínculo. Copie o link de ativação e tente vincular novamente; não gere outro convite para o mesmo e-mail.');
      }
      this.form.controls.displayName.reset(''); this.form.controls.email.reset('');
    } catch (error) {
      this.errorMessage.set(error instanceof HttpErrorResponse && error.status === 409
        ? 'Já existe uma conta com este e-mail. Use a conta existente ou outro e-mail.'
        : 'Não foi possível criar o convite. Verifique a configuração e tente novamente.');
    } finally { this.saving.set(false); }
  }
  async retryLink(): Promise<void> {
    this.errorMessage.set('');
    this.saving.set(true);
    try {
      await this.linkInvitation();
      this.successMessage.set('Vínculo concluído. O convite continua disponível para envio.');
      this.errorMessage.set('');
    } catch {
      this.errorMessage.set('Ainda não foi possível concluir o vínculo. Tente novamente.');
    } finally { this.saving.set(false); }
  }
  private async linkInvitation(): Promise<void> {
    const invitation = this.invitation();
    const studentId = this.pendingStudentId();
    if (!invitation || !studentId) throw new Error('Vínculo indisponível.');
    await this.api.post<void>(`/api/students/${studentId}/guardians/${invitation.userId}`);
    this.pendingStudentId.set('');
  }
  async copyLink(): Promise<void> {
    const invitation = this.invitation();
    if (!invitation) return;
    try { await navigator.clipboard.writeText(invitation.activationUrl); this.copied.set(true); setTimeout(() => this.copied.set(false), 2500); }
    catch { this.errorMessage.set('Não foi possível copiar automaticamente. Selecione e copie o link exibido.'); }
  }
}
