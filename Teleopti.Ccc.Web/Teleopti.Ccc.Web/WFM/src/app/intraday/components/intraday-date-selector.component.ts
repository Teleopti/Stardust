import { Component, EventEmitter, Output, OnInit } from '@angular/core';

@Component({
	selector: 'intraday-date-selector',
	templateUrl: './intraday-date-selector.html'
})
export class IntradayDateSelectorComponent {
	value = '0';
	constructor() {}

	@Output()
	selected = new EventEmitter<number>();

	onClick() {
		this.selected.emit(+this.value);
	}
}
