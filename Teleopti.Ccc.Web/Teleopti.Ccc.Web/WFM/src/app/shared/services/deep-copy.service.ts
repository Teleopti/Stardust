import { Injectable } from '@angular/core';

@Injectable({
	providedIn: 'root'
})
export class DeepCopyService {
	copy(value: any): any {
		const type = Object.prototype.toString.call(value);
		if (type !== '[object Array]' && type !== '[object Object]') return value;

		let newObj = <any>{};

		if (type === '[object Array]') {
			newObj = <any>[];
		}

		for (const key of Object.keys(value)) {
			const val = value[key];
			if (typeof val === 'object') {
				newObj[key] = this.copy(val);
			} else {
				newObj[key] = val;
			}
		}
		return newObj;
	}
	constructor() {}
}
