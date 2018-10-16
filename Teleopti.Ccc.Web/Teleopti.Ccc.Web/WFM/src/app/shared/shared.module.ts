import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { en_US, NgZorroAntdModule, NZ_I18N } from 'ng-zorro-antd';
import { FeedbackMessageComponent, TableOptionsMenu } from './components';
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
		FeedbackMessageComponent,
		TableOptionsMenu
	],
	providers: [{ provide: NZ_I18N, useValue: en_US }, LogonInfoService],
	entryComponents: [FeedbackMessageComponent],
	declarations: [FeedbackMessageComponent, TableOptionsMenu]
})
export class SharedModule {}
