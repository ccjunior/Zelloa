import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Utilities } from './utilities';

describe('Utilities', () => {
  let component: Utilities;
  let fixture: ComponentFixture<Utilities>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Utilities],
    }).compileComponents();

    fixture = TestBed.createComponent(Utilities);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
