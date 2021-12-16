import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { BreakdownRequirementsComponent } from './breakdown-requirements.component';

describe('BreakdownRequirementsComponent', () => {
  let component: BreakdownRequirementsComponent;
  let fixture: ComponentFixture<BreakdownRequirementsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ BreakdownRequirementsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BreakdownRequirementsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
