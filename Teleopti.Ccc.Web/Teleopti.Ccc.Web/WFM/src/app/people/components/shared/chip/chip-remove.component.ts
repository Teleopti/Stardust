import { ChipComponent } from './chip.component';
import { Input, Component } from '@angular/core';

@Component({
	selector: 'chip-remove',
	templateUrl: './chip.component.html',
	styleUrls: ['./chip.component.scss']
})
export class ChipRemoveComponent extends ChipComponent {
	@Input() activeClass: string = 'chip-error';
	@Input() activeIcon: string = 'mdi-close';
	@Input() inactiveIcon: string = 'mdi-delete';
}
