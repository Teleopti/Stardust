
(function() {﻿
	'use strict';﻿
	angular.module('wfm.themes').service('ThemeService', ['$http', '$q', 'Toggle', function ($http, $q, Toggle) {
		var service = {};


		service.setTheme = function (theme) {
			var darkThemeElement = document.getElementById('darkTheme');
			if (darkThemeElement) {
				darkThemeElement.checked = (theme === "dark");
			}

			modifyDOMHeader(theme);

		};
		var modifyDOMHeader = function(theme){
			var styleElements = ["Modules","Style"];
			styleElements.forEach(function (element) {
				var themeComponent = document.getElementById('theme' + element);
				var hash = extractHash(themeComponent);
				themeComponent.setAttribute('href', 'dist/' + element.toLowerCase() + '_' + theme + '.min.css' + hash);
			});

		}
		var extractHash = function(element){
			var href = element.href;
			var hashvalue = href.match("\\?(.*)")//[^\\?]*$
			if (hashvalue == null)
				return "";
			else
				return hashvalue[0];

		};

		service.getTheme = function() {
			return $http.get('../api/Theme');
		};

		service.init = function () {
			var themeToggle = Toggle.togglesLoaded.then(function() {
				if (Toggle.WfmGlobalLayout_personalOptions_37114) {
					return service.getTheme();
				} else {
					return $q.reject();
				}
			});
			themeToggle.then(function(response) {
				var theme = response.data.Name ? response.data.Name : 'classic';
				service.setTheme(theme);
			}, function() {
				service.setTheme('classic');
			});
		};

		service.saveTheme = function(name, overlay) {
			$http.post("../api/Theme/Change", {
				Name: name,
				Overlay: overlay
			});
		};

		return service;
	}]);﻿
})();
