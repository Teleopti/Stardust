import { TestBed, inject } from '@angular/core/testing';

import { AppLogonService } from './app-logon.service';

describe('AppLogonService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AppLogonService]
    });
  });

  it('should be created', inject([AppLogonService], (service: AppLogonService) => {
    expect(service).toBeTruthy();
  }));
});
