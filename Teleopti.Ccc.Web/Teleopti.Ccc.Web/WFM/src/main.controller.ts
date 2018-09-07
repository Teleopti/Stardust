import { IQService, IScope } from 'angular';
import { IWfmRootScopeService } from './main';

MainController.$inject = ['$scope', '$rootScope', 'ThemeService', '$q'];

type ThemeType = 'classic' | 'dark' | '';

export function MainController($scope: IScope, $rootScope: IWfmRootScopeService, ThemeService, $q: IQService) {
	var vm = this;

	ThemeService.getTheme().then(function(result) {
		vm.currentStyle = result.data.Name;
	});

	$rootScope.setTheme = function(theme: ThemeType) {
		vm.styleIsFullyLoaded = true;
	};
}
