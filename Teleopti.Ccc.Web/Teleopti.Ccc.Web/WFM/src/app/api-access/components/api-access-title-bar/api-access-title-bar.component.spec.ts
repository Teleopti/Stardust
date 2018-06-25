import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ApiAccessTitleBarComponent } from './api-access-title-bar.component';

describe('ApiAccessTitleBarComponent', () => {
	let component: ApiAccessTitleBarComponent;
	let fixture: ComponentFixture<ApiAccessTitleBarComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
		declarations: [ApiAccessTitleBarComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
	  fixture = TestBed.createComponent(ApiAccessTitleBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
