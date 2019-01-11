import { Injectable } from '@angular/core';
import { TranslateParser } from '@ngx-translate/core';

/**
 * Adopted from
 * https://github.com/ngx-translate/core/blob/v10.0.2/projects/ngx-translate/core/src/lib/translate.parser.ts#L24
 */

@Injectable()
export class CustomTranslateParser extends TranslateParser {
	/** csutomized pattern */
	templateMatcher: RegExp = /{\s?([^{}\s]*)\s?}/g;

	private isDefined(value: any): boolean {
		return typeof value !== 'undefined' && value !== null;
	}

	public interpolate(expr: string | Function, params?: any): string {
		let result: string;

		if (typeof expr === 'string') {
			result = this.interpolateString(expr, params);
		} else if (typeof expr === 'function') {
			result = this.interpolateFunction(expr, params);
		} else {
			// this should not happen, but an unrelated TranslateService test depends on it
			result = expr as string;
		}

		return result;
	}

	getValue(target: any, key: string): any {
		const keys = key.split('.');
		key = '';
		do {
			key += keys.shift();
			if (
				this.isDefined(target) &&
				this.isDefined(target[key]) &&
				(typeof target[key] === 'object' || !keys.length)
			) {
				target = target[key];
				key = '';
			} else if (!keys.length) {
				target = undefined;
			} else {
				key += '.';
			}
		} while (keys.length);

		return target;
	}

	private interpolateFunction(fn: Function, params?: any) {
		return fn(params);
	}

	private interpolateString(expr: string, params?: any) {
		if (!params) {
			return expr;
		}

		return expr.replace(this.templateMatcher, (substring: string, b: string) => {
			const r = this.getValue(params, b);
			return this.isDefined(r) ? r : substring;
		});
	}
}
