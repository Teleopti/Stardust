import { Component, EventEmitter, Output, Input } from '@angular/core';

@Component({
	selector: 'intraday-date-selector',
	templateUrl: './intraday-date-selector.html'
})
export class IntradayDateSelectorComponent {
	@Input()
	value: string;

	@Output()
	selected = new EventEmitter<number>();

	onClick() {
		this.selected.emit(+this.value);
	}
}
