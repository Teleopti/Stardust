import { Injectable } from '@angular/core';

@Injectable()
export class IntradayHelper {
	constructor() {
	}
	public static isCritical(interval, dayAverage: number, dayRelativeDifference: number) {
		
		return (dayRelativeDifference>0?0:dayRelativeDifference - (interval.s - interval.f)/interval.f > 0.4);

	}
}
