(function() {
	'use strict';
	var global = angular.module('wfm');

	global.controller('mainController', [
		'$scope', 'Toggle', '$rootScope', 'ThemeService', '$q',
		function($scope, Toggle, $rootScope, ThemeService, $q) {

			$rootScope.setTheme = function(theme) {
				var darkThemeElement = document.getElementById('darkTheme');
				if (darkThemeElement) {
					darkThemeElement.checked = (theme === "dark");
				}
				modifyDOMHeader(theme);
			};

			var modifyDOMHeader = function(theme) {
				var styleElements = ["Modules", "Style"];
				styleElements.forEach(function(element) {
					var themeComponent = document.getElementById('theme' + element);
					if (themeComponent) {
						var hash = extractHash(themeComponent);
						themeComponent.setAttribute('href', 'dist/' + element.toLowerCase() + '_' + theme + '.min.css' + hash);
						if (element === "Modules") {
							themeComponent.onload = function() {
								$scope.$apply(function() {
									$scope.styleIsFullyLoaded = true;
								});
							};
						}
					}
				});
			};
			var extractHash = function(element) {
				var hashvalue = element.href.match("\\?(.*)"); //[^\\?]*$
				var returnValue = "";
				if (hashvalue) {
					returnValue = hashvalue[0];
				}
				return returnValue;
			};

		}
	]);
})();
