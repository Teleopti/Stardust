(function() {
	'use strict';
	var global = angular.module('wfm');

	global.controller('mainController', [
		'$scope', 'Toggle', '$rootScope', 'ThemeService', '$q', '$document',
		function($scope, Toggle, $rootScope, ThemeService, $q, $document) {

			var targets = [];
			$document.on('keyup', function(event){
					if (event.keyCode == 9) {
						targets.push(event.target);
						if(targets.length > 1){
							targets[0].style.outline = "";
							targets[1].style.outline = "1px dotted #09F";
							targets.splice(0, 1);
						}
					}
			});

			ThemeService.getTheme().then(function(result){
				$scope.currentStyle = result.data.Name;
			});

			$rootScope.setTheme = function(theme) {
				var darkThemeElement = document.getElementById('darkTheme');
				if (darkThemeElement) {
					darkThemeElement.checked = (theme === "dark");
				}
				$scope.style = theme;
				if (checkCurrentTheme() != theme) {
					modifyDOMHeader(theme);
				}

			};

			var modifyDOMHeader = function(theme) {
				var styleElements = ["Modules", "Style"];
				styleElements.forEach(function(element) {
					var themeComponent = document.getElementById('theme' + element);
					if (themeComponent) {
						var hash = extractHash(themeComponent);
						themeComponent.setAttribute('href', 'dist/' + element.toLowerCase() + '_' + theme + '.min.css' + hash);
						themeComponent.setAttribute('class',theme);
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
			var checkCurrentTheme = function(){
				var currentStyle;
				if (document.getElementById('themeStyle').className) {
					currentStyle = document.getElementById('themeStyle').className
				}
				return currentStyle;
			}

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
