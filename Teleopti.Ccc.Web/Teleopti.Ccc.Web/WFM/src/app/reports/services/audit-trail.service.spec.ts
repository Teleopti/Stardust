import { of } from 'rxjs';
import { AuditEntry } from 'src/app/shared/types';
import { AuditTrailService, PersonByKeyWordResponse } from './audit-trail.service';

describe('Audit-trail service', () => {
	let httpClientSpy: { get: jasmine.Spy };
	let auditTrailService: AuditTrailService;

	beforeEach(() => {
		httpClientSpy = jasmine.createSpyObj('HttpClient', ['get']);
		auditTrailService = new AuditTrailService(<any>httpClientSpy);
	});

	it('should return person by keyword', () => {
		const mockPersonResponse: PersonByKeyWordResponse = {
			Persons: [{ Id: '', FirstName: 'Foo', LastName: 'Bar' }]
		};

		httpClientSpy.get.and.returnValue(of(mockPersonResponse));

		auditTrailService.getPersonByKeyword('Ash').subscribe(persons => {
			expect(persons).toBe(mockPersonResponse);
		});

		expect(httpClientSpy.get.calls.count()).toEqual(1);
	});

	it('should return staffing audit trail', () => {
		const mockAuditEntry: AuditEntry = {
			Action: 'Importera bemanning',
			ActionPerformedBy: 'Ashley Andeen',
			Context: 'Staffing',
			Data: 'File name: template.csv',
			TimeStamp: '2018-11-27T17:41:19.95'
		};
		const auditTrailMockData: AuditEntry[] = [mockAuditEntry];

		httpClientSpy.get.and.returnValue(of(auditTrailMockData));

		auditTrailService
			.getStaffingAuditTrail('1', '2018-11-27', '2018-11-27', 'Staffing')
			.subscribe(auditTrailData => {
				expect(auditTrailData).toBe(auditTrailData);
			});
		expect(httpClientSpy.get.calls.count()).toEqual(1);
	});
});
