import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';

@NgModule({
	imports: [CommonModule, FormsModule, ReactiveFormsModule, BrowserAnimationsModule, HttpClientModule],
	exports: [CommonModule, FormsModule, ReactiveFormsModule, BrowserAnimationsModule, HttpClientModule],
	providers: [],
	entryComponents: []
})
export class SharedModule {}
