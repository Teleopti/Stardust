import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';

import { NzAlertModule } from 'ng-zorro-antd';

@NgModule({
	imports: [CommonModule, FormsModule, ReactiveFormsModule, BrowserAnimationsModule, HttpClientModule, NzAlertModule],
	exports: [CommonModule, FormsModule, ReactiveFormsModule, BrowserAnimationsModule, HttpClientModule, NzAlertModule],
	providers: [],
	entryComponents: []
})
export class SharedModule {}
