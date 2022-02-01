import { TestBed } from '@angular/core/testing';

import { OktaAuthService } from './okta-auth.service';

describe('OktaAuthServiceService', () => {
  let service: OktaAuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(OktaAuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
