import { NgModule } from '@angular/core';
import { DowngradeableComponent } from '@wfm/types';
import { SharedModule } from '../shared/shared.module';
import { AddAppPageComponent, ListPageComponent } from './components';
import { ExternalApplicationService, NavigationService } from './services';

@NgModule({
	declarations: [AddAppPageComponent, ListPageComponent],
	imports: [SharedModule],
	providers: [ExternalApplicationService, NavigationService],
	exports: [],
	entryComponents: [AddAppPageComponent, ListPageComponent]
})
export class ApiAccessModule {
	ngDoBootstrap() {}
}

export const apiAccessComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2ApiAccessAddAppPage', ng2Component: AddAppPageComponent },
	{ ng1Name: 'ng2ApiAccessListPage', ng2Component: ListPageComponent }
];
