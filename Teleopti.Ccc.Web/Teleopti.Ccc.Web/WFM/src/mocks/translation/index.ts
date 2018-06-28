import { Directive, NgModule, Pipe, PipeTransform } from '@angular/core';

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

@NgModule({
	declarations: [MockTranslateParamsDirective, MockTranslationPipe],
	exports: [MockTranslationPipe, MockTranslateParamsDirective]
})
export class MockTranslationModule {}
