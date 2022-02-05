import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NgxOmCommonLibraryComponent } from './ngx-om-common-library.component';

describe('NgxOmCommonLibraryComponent', () => {
  let component: NgxOmCommonLibraryComponent;
  let fixture: ComponentFixture<NgxOmCommonLibraryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NgxOmCommonLibraryComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NgxOmCommonLibraryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
