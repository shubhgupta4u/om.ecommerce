import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NgxAccountLibraryComponent } from './ngx-account-library.component';

describe('NgxAccountLibraryComponent', () => {
  let component: NgxAccountLibraryComponent;
  let fixture: ComponentFixture<NgxAccountLibraryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NgxAccountLibraryComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NgxAccountLibraryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
