import { Injectable } from '@angular/core';

@Injectable()
export class IntradayHelper {
	constructor() {
	}
	public static isCritical(interval, dayAverage: number, dayRelativeDifference: number) {
		
		return ((interval.f - interval.s)/dayAverage > 0.5 ) && (interval.f > 0.5) && (dayRelativeDifference>0?0:dayRelativeDifference - (interval.s - interval.f)/interval.f > 0.4);

	}
}
