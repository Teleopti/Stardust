import { Injectable } from '@angular/core';

@Injectable()
export class IntradayHelper {
	constructor() {
	}
	public static isCritical(interval, dayRelativeDifference: number) {
		const limit = 0.4;
		if (dayRelativeDifference < 0) {
			return (dayRelativeDifference - (interval.s - interval.f)/interval.f > limit);

		}
		return (0 - (interval.s - interval.f)/interval.f > limit);
	}
}
