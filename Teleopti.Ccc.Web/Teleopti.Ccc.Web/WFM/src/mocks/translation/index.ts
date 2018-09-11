import { Directive, NgModule, Pipe, PipeTransform, Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Observable, of } from 'rxjs';

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

	public get(keyString: string): Observable<string> {
		return of(keyString);
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
