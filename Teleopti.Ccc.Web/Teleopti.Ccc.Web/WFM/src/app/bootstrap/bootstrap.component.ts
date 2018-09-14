import { Component, Input } from '@angular/core';
import { interval } from 'rxjs';
import { filter, first, switchMap, tap } from 'rxjs/operators';
import { MainControllerStyle } from '../../main.controller';
import { ThemeService } from '../core/services';

@Component({
	selector: 'bootstrap',
	templateUrl: './bootstrap.component.html'
})
export class BootstrapComponent {
	@Input('style')
	style: MainControllerStyle;

	constructor(private themeService: ThemeService) {
		themeService
			.getTheme()
			.pipe(
				switchMap(() => {
					return interval(50).pipe(
						tap(() => {
							console.log('interval tick', this.style);
						}),
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
	}
}
