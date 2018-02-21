import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';

@Component({
	selector: 'chip',
	templateUrl: './chip.component.html',
	styleUrls: ['./chip.component.scss']
})
export class ChipComponent {
	@Input() disabled: boolean = false;
	@Input() activeClass: string = 'chip-success';
	@Input() activeIcon: string = 'mdi-check';
	@Input() inactiveIcon: string = 'mdi-plus';
	@Input() active: boolean = false;

	@HostListener('click')
	handleClick() {
		if (!this.disabled) {
			this.active = !this.active;
		}
	}
}
