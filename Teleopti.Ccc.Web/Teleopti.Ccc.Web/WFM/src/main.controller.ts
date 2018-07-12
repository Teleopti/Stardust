import { IAngularStatic, IControllerConstructor, IScope, IRootScopeService, IQService } from 'angular';
import { IWfmRootScopeService } from './main';

MainController.$inject = ['$scope', '$rootScope', 'ThemeService', '$q'];

export function MainController($scope: IScope, $rootScope: IWfmRootScopeService, ThemeService, $q: IQService) {
	var vm = this;

	ThemeService.getTheme().then(function(result) {
		vm.currentStyle = result.data.Name;
	});

	$rootScope.setTheme = function(theme: string) {
		var darkThemeElement: any = document.getElementById('darkTheme');
		if (darkThemeElement) {
			darkThemeElement.checked = theme === 'dark';
		}

		if (checkCurrentTheme() != theme) {
			vm.styleIsFullyLoaded = false;
			applyThemeToModules(theme);
		}

		if (theme === 'dark' && document.body) {
			document.body.classList.add('angular-theme-dark');
			document.body.classList.remove('angular-theme-classic');
		}
		if (theme === 'classic' && document.body) {
			document.body.classList.add('angular-theme-classic');
			document.body.classList.remove('angular-theme-dark');
		}
	};

	function applyThemeToModules(theme) {
		var oldNode = document.getElementById('themeModules');
		var newNode = document.createElement('link');

		newNode.id = 'themeModules';
		newNode.rel = 'stylesheet';
		newNode.href = '';
		newNode.onload = function() {
			$scope.$apply(function() {
				vm.styleIsFullyLoaded = true;
			});
		};

		newNode.setAttribute('href', 'dist/resources/modules_' + theme + '.min.css');
		newNode.setAttribute('class', theme);

		document.body.replaceChild(newNode, oldNode);
	}

	function checkCurrentTheme() {
		let classList = document.body.classList;
		if (classList.contains('angular-theme-dark')) {
			return 'dark';
		} else if (classList.contains('angular-theme-classic')) {
			return 'classic';
		} else {
			return 'classic';
		}
	}
}
