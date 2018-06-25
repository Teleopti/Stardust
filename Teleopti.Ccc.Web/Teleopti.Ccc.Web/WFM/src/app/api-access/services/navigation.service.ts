import { Injectable } from '@angular/core';

@Injectable()
export class NavigationService {
	private go(hash: string): void {
		location.hash = hash;
		this.resetScroll();
	}

	public navToAddApp() {
		this.go('#/api-access/add-app');
	}

	public navToList() {
		this.go('#/api-access/list');
	}

	private resetScroll() {
		var element = document.getElementById('materialcontainer');
		if (element) element.scrollTop = 0;
	}
}
