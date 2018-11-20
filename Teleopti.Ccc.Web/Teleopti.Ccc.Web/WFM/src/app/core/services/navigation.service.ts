import { Inject, Injectable } from '@angular/core';
import { IStateService } from 'angular-ui-router';

type StateId = string;

@Injectable()
export class NavigationService {
	constructor(@Inject('$state') private $state: IStateService) {}

	public go(state: StateId, resetScroll = true): void {
		this.$state.go(state);
		if (resetScroll) this.resetScroll();
	}

	private resetScroll() {
		const element = document.getElementById('materialcontainer');
		if (element) element.scrollTop = 0;
	}
}
