(function() {
	'use strict';
	var themes = angular.module('wfm.themes', ['toggleService']);

	themes.controller('themeController', [
		'$scope', 'Toggle', 'ThemeService', '$q',
		function($scope, Toggle, ThemeService, $q) {
			$scope.showOverlay = false;
			Toggle.togglesLoaded.then(function() {
				$scope.personalizeToggle = Toggle.WfmGlobalLayout_personalOptions_37114;
			});


			var focusMenu = function() {
				document.getElementById("themeMenu").focus();
			};

			checkThemeState().then(function(result) {
				if (result.data.Name === "dark") {
					$scope.darkTheme = true;
				} else {
					$scope.darkTheme = false;
				}
			});

			function checkThemeState() {
				var deferred = $q.defer();
				ThemeService.getTheme().then(function(response) {
					deferred.resolve(response);
				});
				return deferred.promise;
			};

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
