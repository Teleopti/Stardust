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
		if (getCurrentTheme() != theme) {
			vm.styleIsFullyLoaded = false;
			const waitForThemes = Promise.all([
				applyAngularMatrialTheme(theme),
				applyStyleguideTheme(theme),
				applyAntTheme(theme)
			]);
			waitForThemes.then(() => {
				$scope.$apply(function() {
					vm.styleIsFullyLoaded = true;
				});
			});
		}
	};

	async function applyAngularMatrialTheme(theme: ThemeType) {
		if (theme === 'dark' && document.documentElement) {
			document.documentElement.classList.add('angular-theme-dark');
			document.documentElement.classList.remove('angular-theme-classic');
		}
		if (theme === 'classic' && document.documentElement) {
			document.documentElement.classList.add('angular-theme-classic');
			document.documentElement.classList.remove('angular-theme-dark');
		}
		return;
	}

	function applyStyleguideTheme(theme) {
		return replaceCssFile(`dist/styleguide_${theme}.css`, 'themeStyleguide', theme);
	}

	function applyAntTheme(theme: ThemeType) {
		return replaceCssFile(`dist/ant_${theme}.css`, 'themeAnt', theme);
	}

	function replaceCssFile(path: string, id: string, theme: ThemeType) {
		return new Promise((res, rej) => {
			var oldNode = document.getElementById(id);
			var newNode = document.createElement('link');
			newNode.id = id;
			newNode.rel = 'stylesheet';

			newNode.addEventListener('load', () => {
				res();
			});

			newNode.setAttribute('href', path);
			newNode.setAttribute('class', theme);

			document.body.replaceChild(newNode, oldNode);
		});
	}

	function getCurrentTheme(): ThemeType {
		let classList = document.documentElement.classList;
		if (classList.contains('angular-theme-dark')) {
			return 'dark';
		} else if (classList.contains('angular-theme-classic')) {
			return 'classic';
		} else {
			return '';
		}
	}
}
