import { TestBed } from '@angular/core/testing';

import { NgxOmCommonLibraryService } from './ngx-om-common-library.service';

describe('NgxOmCommonLibraryService', () => {
  let service: NgxOmCommonLibraryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NgxOmCommonLibraryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
