import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DowngradeableComponent } from '@wfm/types';
import { IStateProvider, IUrlRouterProvider } from 'angular-ui-router';
import { WorkspaceComponent } from './components';
import { ReportService } from './core/report.service';

@NgModule({
	declarations: [WorkspaceComponent],
	imports: [CommonModule, FormsModule],
	providers: [ReportService],
	exports: [],
	entryComponents: [WorkspaceComponent]
})
export class InsightsModule {
	ngDoBootstrap() {}
}

export const insightsComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2InsightsWorkspacePage', ng2Component: WorkspaceComponent }
];

export function insightsRouterConfig($stateProvider: IStateProvider, $urlRouterProvider: IUrlRouterProvider) {
	$urlRouterProvider.when('/insights', '/insights/workspace');
	$stateProvider
		.state('insights', {
			url: '/insights',
			template: '<div ui-view="content"></div>'
		})
		.state('insights.workspace', {
			url: '/workspace',
			views: {
				content: { template: '<ng2-insights-workspace-page></ng2-insights-workspace-page>' }
			}
		});
}
