import { Injectable } from '@angular/core';
import { NavigationService as NavigationServiceCore } from './../../core/services';
@Injectable()
export class NavigationService {
	constructor(private navService: NavigationServiceCore) {}

	public navToSearch() {
		this.navService.go('people.index');
	}

	public navToGrant() {
		this.navService.go('people.grant');
	}

	public navToRevoke() {
		this.navService.go('people.revoke');
	}

	public navToApplicationLogon() {
		this.navService.go('people.applogon');
	}

	public navToIdentityLogon() {
		this.navService.go('people.identitylogon');
	}
}
