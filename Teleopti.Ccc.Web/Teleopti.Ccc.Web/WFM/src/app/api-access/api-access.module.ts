import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { AddAppPageComponent, ListPageComponent } from './components';
import { ExternalApplicationService, NavigationService } from './services';

@NgModule({
	declarations: [AddAppPageComponent, ListPageComponent],
	imports: [
		SharedModule
	],
	providers: [ExternalApplicationService, NavigationService],
	exports: [],
	entryComponents: [AddAppPageComponent, ListPageComponent]
})
export class ApiAccessModule {
	ngDoBootstrap() {}
}
