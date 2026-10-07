import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SchoolApiService, SchoolInfo, Classroom, Student, PagedResponse } from '../core/school-api.service';

@Component({ selector: 'app-dashboard-page', imports: [RouterLink], templateUrl: './dashboard-page.html', styleUrls: ['./school-pages.scss', './dashboard-page.scss'] })
export class DashboardPage {
  private readonly api = inject(SchoolApiService);
  readonly school = signal<SchoolInfo | null>(null);
  readonly classroomCount = signal(0);
  readonly studentCount = signal(0);
  readonly errorMessage = signal('');

  constructor() {
    void Promise.all([
      this.api.get<SchoolInfo>('/api/school'),
      this.api.get<PagedResponse<Classroom>>('/api/classrooms?page=1&pageSize=1'),
      this.api.get<PagedResponse<Student>>('/api/students?page=1&pageSize=1'),
    ]).then(([school, classrooms, students]) => {
      this.school.set(school);
      this.classroomCount.set(classrooms.totalCount);
      this.studentCount.set(students.totalCount);
    }).catch(() => this.errorMessage.set('Não foi possível carregar os dados da instituição. Atualize a página ou verifique a conexão com a API.'));
  }
}
