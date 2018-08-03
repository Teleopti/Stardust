import { InjectionToken } from "@angular/core";

export const RTA_SERVICE = new InjectionToken<any>('RTA_SERVICE');

export function createRtaService(i) {
	return i.get('rtaService');
}

export const rtaServiceProvider = {
	provide: RTA_SERVICE,
	useFactory: createRtaService,
	deps: ['$injector']
};