import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FeedbackMessageComponent } from './components';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';

import { en_US, NgZorroAntdModule, NZ_I18N } from 'ng-zorro-antd';

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
	providers: [{ provide: NZ_I18N, useValue: en_US }],
	entryComponents: [FeedbackMessageComponent],
	declarations: [FeedbackMessageComponent]
})
export class SharedModule {}
