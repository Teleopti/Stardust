import { NgModule } from '@angular/core';
import { DowngradeableComponent } from '@wfm/types';
import { WorkspaceComponent } from './components';

@NgModule({
	declarations: [WorkspaceComponent],
	imports: [],
	providers: [],
	exports: [],
	entryComponents: [WorkspaceComponent]
})
export class PmModule {
	ngDoBootstrap() {}
}

export const pmComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2PmWorkspacePage', ng2Component: WorkspaceComponent }
];
