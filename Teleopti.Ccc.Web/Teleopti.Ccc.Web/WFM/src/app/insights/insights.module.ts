import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DowngradeableComponent } from '@wfm/types';
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { IStateProvider, IUrlRouterProvider } from 'angular-ui-router';
import { ReportService } from './core/report.service';
import { WorkspaceComponent } from './components';
import { ReportComponent } from './components/report/report.component';
import { IStateParamsService } from 'angular-ui-router';

@NgModule({
	declarations: [WorkspaceComponent, ReportComponent],
	imports: [CommonModule, FormsModule, NgZorroAntdModule, RouterModule],
	providers: [ReportService,
		{
			provide: '$stateParams',
			useFactory: (i: any): IStateParamsService => i.get('$stateParams'),
			deps: ['$injector']
		}],
	exports: [],
	entryComponents: [WorkspaceComponent, ReportComponent]
})
export class InsightsModule {
	ngDoBootstrap() { }
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
			url: '/report',
			views: {
				content: { template: '<ng2-insights-report-page></ng2-insights-report-page>' }
			},
			params: {
				report: undefined,
				action: undefined
			}
		});
}
