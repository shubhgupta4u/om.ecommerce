import { TestBed } from '@angular/core/testing';

import { MsalAuthService } from './msal-auth.service';

describe('MsalAuthService', () => {
  let service: MsalAuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MsalAuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
