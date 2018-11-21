import { IQService, IScope } from 'angular';
import { IWfmRootScopeService } from './main';

MainController.$inject = ['$scope', '$rootScope', '$q'];

export interface MainControllerStyle {
	isFullyLoaded: boolean;
}

export function MainController<IControllerConstructor>(
	$scope: IScope,
	$rootScope: IWfmRootScopeService,
	$q: IQService
) {
	this.style = {
		isFullyLoaded: false
	};
}
