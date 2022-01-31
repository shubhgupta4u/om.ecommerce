import { TestBed } from '@angular/core/testing';

import { NgxAccountLibraryService } from './ngx-account-library.service';

describe('NgxAccountLibraryService', () => {
  let service: NgxAccountLibraryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NgxAccountLibraryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
