import { Component, Input } from '@angular/core';
import { interval } from 'rxjs';
import { filter, first, switchMap } from 'rxjs/operators';
import { MainControllerStyle } from '../../../main.controller';
import { ThemeService } from '../../core/services';

@Component({
	selector: 'bootstrap',
	styleUrls: ['./bootstrap.component.scss'],
	templateUrl: './bootstrap.component.html'
})
export class BootstrapComponent {
	@Input()
	style: MainControllerStyle;
	lowLightFilter = false;

	constructor(private themeService: ThemeService) {
		themeService.theme$
			.pipe(
				switchMap(() => {
					return interval(50).pipe(
						filter(() => typeof this.style !== 'undefined'),
						first()
					);
				})
			)
			.subscribe({
				next: () => {
					this.style.isFullyLoaded = true;
				}
			});
		this.themeService.theme$.subscribe({
			next: theme => {
				this.lowLightFilter = theme.Overlay;
			}
		});
	}
}
