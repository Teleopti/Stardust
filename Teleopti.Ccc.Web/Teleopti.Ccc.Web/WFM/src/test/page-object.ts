import { DebugElement } from '@angular/core';
import { ComponentFixture } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

export class PageObject {
	fixture: ComponentFixture<any>;

	constructor(fixture: ComponentFixture<any>) {
		this.fixture = fixture;
	}

	public queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}

	public query(selector: string): DebugElement {
		return this.fixture.debugElement.query(By.css(selector));
	}
}
