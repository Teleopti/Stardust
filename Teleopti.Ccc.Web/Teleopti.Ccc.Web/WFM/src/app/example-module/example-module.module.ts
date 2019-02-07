/**
 * This entire module serves as a starting point for new Angular wfm modules.
 * Take what you can, rename the examples, and claim it as yours :D
 */

import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { DowngradeableComponent } from '@wfm/types';
import { IStateProvider, IUrlRouterProvider } from 'angular-ui-router';
import { SharedModule } from '../shared/shared.module';
import { ExampleComponent } from './components';
import { ExampleService } from './shared';

@NgModule({
	declarations: [ExampleComponent],
	imports: [SharedModule, TranslateModule.forChild()],
	providers: [ExampleService],
	exports: [],
	entryComponents: [ExampleComponent]
})
export class ExampleModule {}

export const exampleComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2ExampleComponent', ng2Component: ExampleComponent }
];

// AngularJS router configuration
export function peopleRouterConfig($stateProvider: IStateProvider, $urlRouterProvider: IUrlRouterProvider) {
	$urlRouterProvider.when('/example-module-name', '/example-module-name/index');
	$stateProvider
		.state('example-module', {
			url: '/example-module-name',
			template: '<div ui-view="content"></div>'
		})
		.state('example-module.index', {
			url: '/index',
			views: {
				content: { template: '<ng2-example-component></ng2-example-component>' }
			}
		});
}
