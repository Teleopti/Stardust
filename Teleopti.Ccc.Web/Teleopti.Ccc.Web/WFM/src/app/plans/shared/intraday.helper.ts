import { Injectable } from '@angular/core';

@Injectable()
export class IntradayHelper {
	constructor() {
	}
	public static isCritical(interval, dayAverage: number, dayRelativeDifference: number) {
		return (interval.f - interval.s > 0.4 * dayAverage) && (interval.f > 0.5) && (dayRelativeDifference - (interval.s - interval.f)/interval.f > 0.7);
	}
}
