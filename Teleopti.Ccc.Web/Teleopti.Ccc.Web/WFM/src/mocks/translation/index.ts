import { Directive, NgModule, Pipe, PipeTransform, Injectable } from '@angular/core';
import { inject } from '../../../node_modules/@angular/core/testing';
import { TranslateService } from '../../../node_modules/@ngx-translate/core';

@Directive({
	selector: '[translate]',
	inputs: ['translateParams']
})
export class MockTranslateParamsDirective {
	translateParams: any;
	constructor() {}
}

@Pipe({ name: 'translate' })
export class MockTranslationPipe implements PipeTransform {
	transform(value: any): any {
		return value;
	}
}

@Injectable()
export class MockTranslateService {
	public instant(keyString: string) {
		return keyString;
	}
}
export const mockTranslateServiceProvider = {
	provide: TranslateService,
	useClass: MockTranslateService
};

@NgModule({
	declarations: [MockTranslateParamsDirective, MockTranslationPipe],
	exports: [MockTranslationPipe, MockTranslateParamsDirective],
	providers: [mockTranslateServiceProvider]
})
export class MockTranslationModule {}
