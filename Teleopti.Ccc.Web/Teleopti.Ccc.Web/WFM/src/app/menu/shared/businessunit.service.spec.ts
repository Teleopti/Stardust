import { of } from 'rxjs';
import { BusinessUnit, BusinessUnitService } from './businessunit.service';

describe('BusinessUnitService', () => {
	let httpClientSpy: { get: jasmine.Spy };
	let businessUnitService: BusinessUnitService;

	beforeEach(() => {
		httpClientSpy = jasmine.createSpyObj('HttpClient', ['get']);
		businessUnitService = new BusinessUnitService(<any>httpClientSpy);
	});

	it('should return business units', () => {
		const businessUnits: BusinessUnit[] = [{ Id: '1', Name: 'First' }];
		httpClientSpy.get.and.returnValue(of(businessUnits));

		businessUnitService.getBusinessUnits().subscribe(units => {
			expect(units).toBe(businessUnits);
		});
		expect(httpClientSpy.get.calls.count()).toEqual(1);
	});
});
