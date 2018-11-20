import { DOCUMENT } from '@angular/common';
import { Component, Inject, ViewChild } from '@angular/core';
import { BehaviorSubject, interval } from 'rxjs';
import { map } from 'rxjs/operators';
import { NavigationService } from 'src/app/core/services';
import { ToggleMenuService } from '../../shared/toggle-menu.service';

@Component({
	selector: 'top-navigation',
	templateUrl: './top-navigation.component.html',
	styleUrls: ['./top-navigation.component.scss']
})
export class TopNavigationComponent {
	helpUrl$ = interval(400).pipe(
		map(() => {
			return 'https://wiki.teleopti.com/TeleoptiWFM/' + this.$state.current.name;
		})
	);

	mainUrl = this.$state.href('main');

	constructor(
		@Inject('$state') private $state,
		public navigationService: NavigationService,
		public toggleMenuService: ToggleMenuService
	) {}
}
