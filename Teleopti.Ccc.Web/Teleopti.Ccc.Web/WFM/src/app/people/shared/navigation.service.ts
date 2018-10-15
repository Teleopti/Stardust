import { Injectable } from '@angular/core';

@Injectable()
export class NavigationService {
	private go(hash: string): void {
		location.hash = hash;
		this.resetScroll();
	}

	public navToSearch() {
		this.go('#/people/search');
	}

	public navToGrant() {
		this.go('#/people/roles/grant');
	}

	public navToRevoke() {
		this.go('#/people/roles/revoke');
	}

	public navToApplicationLogon() {
		this.go('#/people/access/applicationlogon');
	}

	public navToIdentityLogon() {
		this.go('#/people/access/identitylogon');
	}

	private resetScroll() {
		var element = document.getElementById('materialcontainer');
		if (element) element.scrollTop = 0;
	}
}
