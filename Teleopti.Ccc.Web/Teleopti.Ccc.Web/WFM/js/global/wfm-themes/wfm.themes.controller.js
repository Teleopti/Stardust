(function() {
	'use strict';
	var themes = angular.module('wfm.themes', ['toggleService']);

	themes.controller('themeController', [
		'$scope', 'Toggle', 'ThemeService', '$q',
		function($scope, Toggle, ThemeService, $q) {

			Toggle.togglesLoaded.then(function() {
				$scope.personalizeToggle = Toggle.WfmGlobalLayout_personalOptions_37114;
			});

			var focusMenu = function() {
				document.getElementById("themeMenu").focus();
			};

			checkThemeState().then(function(result) {
				$scope.showOverlay = result.data.Overlay;
				$scope.currentTheme = result.data.Name;
				replaceCurrentTheme($scope.currentTheme, $scope.showOverlay);
				if ($scope.currentTheme === "dark") {
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
			}

			$scope.toggleOverlay = function() {
				$scope.showOverlay = !$scope.showOverlay;
				replaceCurrentTheme($scope.currentTheme, $scope.showOverlay);
				focusMenu();
			};

			var replaceCurrentTheme = function(theme, overlay) {
				ThemeService.setTheme(theme);
				ThemeService.saveTheme(theme, overlay);
			};

			$scope.toggleTheme = function() {
				focusMenu();
				if ($scope.darkTheme) {
					$scope.currentTheme = 'classic';
					replaceCurrentTheme('classic',	$scope.showOverlay );
				} else {
					$scope.currentTheme = 'dark';
					replaceCurrentTheme('dark', $scope.showOverlay );
				}
				$scope.darkTheme = !$scope.darkTheme;
			};
		}
	]);
})();
