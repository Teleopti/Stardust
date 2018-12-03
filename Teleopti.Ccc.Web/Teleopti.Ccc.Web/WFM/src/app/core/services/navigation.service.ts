import { DOCUMENT } from '@angular/common';
import { Inject, Injectable } from '@angular/core';
import { IStateService } from 'angular-ui-router';

type StateId = string;

@Injectable()
export class NavigationService {
	constructor(@Inject(DOCUMENT) private document: Document, @Inject('$state') private $state: IStateService) {}

	public go(state: StateId, resetScroll = true): void {
		this.$state.go(state);
		if (resetScroll) this.resetScroll();
	}

	private resetScroll() {
		const element = this.document.getElementById('materialcontainer');
		if (element) element.scrollTop = 0;
	}
}
