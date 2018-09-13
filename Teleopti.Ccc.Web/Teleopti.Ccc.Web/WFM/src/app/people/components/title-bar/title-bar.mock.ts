import { Component, NgModule } from '@angular/core';

@Component({
	selector: 'people-title-bar',
	template: '<div>MockTitleBarComponent</div>'
})
export class MockTitleBarComponent {}

@NgModule({
	declarations: [MockTitleBarComponent],
	exports: [MockTitleBarComponent]
})
export class MockTitleBarModule {}
