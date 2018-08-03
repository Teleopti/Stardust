import { InjectionToken } from "@angular/core";

export const RTA_DATA_SERVICE = new InjectionToken<any>('RTA_DATA_SERVICE');

export function createRtaDataService(i) {
	return i.get('rtaDataService');
}

export const rtaDataServiceProvider = {
	provide: RTA_DATA_SERVICE,
	useFactory: createRtaDataService,
	deps: ['$injector']
};