import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DowngradeableComponent } from '@wfm/types';
import { IStateParamsService, IStateProvider, IUrlRouterProvider } from 'angular-ui-router';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { WorkspaceComponent } from './components';
import { ReportComponent } from './components/report/report.component';
import { ReportService } from './core/report.service';

@NgModule({
	declarations: [WorkspaceComponent, ReportComponent],
	imports: [CommonModule, FormsModule, NgZorroAntdModule, RouterModule],
	providers: [
		ReportService,
		{
			provide: '$stateParams',
			useFactory: (i: any): IStateParamsService => i.get('$stateParams'),
			deps: ['$injector']
		}
	],
	exports: [],
	entryComponents: [WorkspaceComponent, ReportComponent]
})
export class InsightsModule {
	ngDoBootstrap() {}
}

export const insightsComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2InsightsWorkspacePage', ng2Component: WorkspaceComponent },
	{ ng1Name: 'ng2InsightsReportPage', ng2Component: ReportComponent }
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
		})
		.state('insights.report', {
			url: '/report/:reportId/:action',
			views: {
				content: { template: '<ng2-insights-report-page></ng2-insights-report-page>' }
			},
			params: {
				reportId: undefined,
				action: undefined
			}
		});
}
