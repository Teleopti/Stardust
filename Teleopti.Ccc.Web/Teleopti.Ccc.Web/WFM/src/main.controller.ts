import { IQService, IScope } from 'angular';
import { IWfmRootScopeService } from './main';

MainController.$inject = ['$scope', '$rootScope', 'ThemeService', '$q'];

type ThemeType = 'classic' | 'dark' | '';

export function MainController($scope: IScope, $rootScope: IWfmRootScopeService, ThemeService, $q: IQService) {
	var vm = this;
}
