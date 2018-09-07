import { IQService, IScope } from 'angular';
import { IWfmRootScopeService } from './main';

MainController.$inject = ['$scope', '$rootScope', '$q'];

type ThemeType = 'classic' | 'dark' | '';

export function MainController($scope: IScope, $rootScope: IWfmRootScopeService, $q: IQService) {
	var vm = this;
}
