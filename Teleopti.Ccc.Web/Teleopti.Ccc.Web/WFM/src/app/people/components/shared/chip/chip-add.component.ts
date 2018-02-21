import { ChipComponent } from './chip.component';
import { Input, Component } from '@angular/core';

@Component({
	selector: 'chip-add',
	templateUrl: './chip.component.html',
	styleUrls: ['./chip.component.scss']
})
export class ChipAddComponent extends ChipComponent {
	@Input() activeClass: string = 'chip-success';
	@Input() activeIcon: string = 'mdi-check';
	@Input() inactiveIcon: string = 'mdi-plus';
}
