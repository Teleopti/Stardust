import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
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
