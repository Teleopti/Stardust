import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { DowngradeableComponent } from '@wfm/types';
import { AuthenticationModule } from '../authentication/authentication.module';
import { SharedModule } from '../shared/shared.module';
import { SettingsMenuComponent } from './components';

@NgModule({
	declarations: [SettingsMenuComponent],
	imports: [SharedModule, TranslateModule.forChild(), AuthenticationModule],
	providers: [TranslateModule],
	entryComponents: [SettingsMenuComponent]
})
export class NavigationModule {}

export const navigationComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2SettingsMenu', ng2Component: SettingsMenuComponent }
];
