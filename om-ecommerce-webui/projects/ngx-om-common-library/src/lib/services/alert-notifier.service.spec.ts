import { TestBed } from '@angular/core/testing';

import { AlertNotifierService } from './alert-notifier.service';

describe('AlertNotifierService', () => {
  let service: AlertNotifierService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AlertNotifierService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
