import { Injectable } from '@angular/core';
import { NavigationService as NavigationServiceCore } from './../../core/services';

@Injectable()
export class NavigationService {
	constructor(private navService: NavigationServiceCore) {}

	public navToAddApp() {
		this.navService.go('apiaccess.addapp');
	}

	public navToList() {
		this.navService.go('apiaccess.index');
	}
}
