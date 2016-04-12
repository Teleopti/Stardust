(function() {
	'use strict';
	var themes = angular.module('wfm.themes', ['toggleService']);

	themes.controller('themeController', [
		'$scope', 'Toggle', 'ThemeService',
		function($scope, Toggle, ThemeService) {
			$scope.showOverlay = false;
			$scope.darkTheme = false;
			Toggle.togglesLoaded.then(function() {
				$scope.personalizeToggle = Toggle.WfmGlobalLayout_personalOptions_37114;
			});

			var focusMenu = function() {
				document.getElementById("themeMenu").focus();
			}

			$scope.toggleOverlay = function() {
				$scope.showOverlay = !$scope.showOverlay;
				focusMenu();
			};

			var replaceCurrentTheme = function(theme) {
				ThemeService.setTheme(theme);
				ThemeService.saveTheme(theme);
			};

			$scope.toggleDarkTheme = function() {
				focusMenu();
				var theme = 'classic';
				if ($scope.darkTheme) {
					replaceCurrentTheme('classic');
				} else {
					replaceCurrentTheme('dark');
				}
				$scope.darkTheme = !$scope.darkTheme;
			};
		}
	]);
})();
