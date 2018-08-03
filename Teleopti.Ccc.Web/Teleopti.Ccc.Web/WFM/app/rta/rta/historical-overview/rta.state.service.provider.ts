import { InjectionToken } from "@angular/core";

export const RTA_STATE_SERVICE = new InjectionToken<any>('RTA_STATE_SERVICE');

export function createRtaStateService(i) {
	return i.get('rtaStateService');
}

export const rtaStateServiceProvider = {
	provide: RTA_STATE_SERVICE,
	useFactory: createRtaStateService,
	deps: ['$injector']
};