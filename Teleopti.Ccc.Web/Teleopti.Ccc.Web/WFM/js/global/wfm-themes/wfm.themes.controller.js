(function() {
	'use strict';
	var themes = angular.module('wfm.themes', []);

	themes.controller('themeController', [
		'$scope', 'Toggle',
		function($scope, toggleService) {
			$scope.showOverlay = false;
			$scope.darkTheme = false;
			toggleService.togglesLoaded.then(function() {
				$scope.personalizeToggle = toggleService.WfmGlobalLayout_personalOptions_37114;
			});

			var focusMenu = function() {
				document.getElementById("themeMenu").focus();
			}

			$scope.toggleOverlay = function() {
				$scope.showOverlay = !$scope.showOverlay;
				focusMenu();
			};

			var replaceCurrentTheme = function(theme) {
				document.getElementById('themeModules').setAttribute('href', 'dist/modules_' + theme + '.min.css');
				document.getElementById('themeStylesheet').setAttribute('href', 'dist/style_' + theme + '.min.css');
			};

			$scope.toggleDarkTheme = function() {
				focusMenu();
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
