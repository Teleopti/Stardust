import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { DowngradeableComponent } from '@wfm/types';
import { en_US, NgZorroAntdModule, NZ_I18N, NZ_ICON_DEFAULT_TWOTONE_COLOR, NZ_ICONS } from 'ng-zorro-antd';
import { FeedbackMessageComponent } from './components';
import { LogonInfoService } from './services';

@NgModule({
	imports: [
		CommonModule,
		FormsModule,
		ReactiveFormsModule,
		BrowserAnimationsModule,
		HttpClientModule,
		NgZorroAntdModule
	],
	exports: [
		CommonModule,
		FormsModule,
		ReactiveFormsModule,
		BrowserAnimationsModule,
		HttpClientModule,
		NgZorroAntdModule,
		FeedbackMessageComponent
	],
	providers: [
		{ provide: NZ_I18N, useValue: en_US },
		LogonInfoService,
		{ provide: NZ_ICON_DEFAULT_TWOTONE_COLOR, useValue: '#00ff00' } // If not provided, Ant Design's official blue would be used
	],
	entryComponents: [FeedbackMessageComponent],
	declarations: [FeedbackMessageComponent]
})
export class SharedModule {}

export const sharedComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2FeedbackMessage', ng2Component: FeedbackMessageComponent }
];
