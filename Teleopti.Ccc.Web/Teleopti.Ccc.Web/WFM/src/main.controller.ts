import { IQService, IScope } from 'angular';
import { IWfmRootScopeService } from './main';

MainController.$inject = ['$scope', '$rootScope', '$q'];

export interface MainControllerStyle {
	isFullyLoaded: boolean;
}

export function MainController($scope: IScope, $rootScope: IWfmRootScopeService, $q: IQService) {
	var vm = this;

	vm.style = {
		isFullyLoaded: false
	};
}
