
(function() {﻿
	'use strict';﻿
	angular.module('wfm.themes').service('ThemeService', ['$http', '$rootScope', '$q', 'Toggle', function($http, $rootScope, $q, Toggle) {
		var service = {};

		service.getTheme = function() {
			return $http.get('../api/Theme');
		};

		service.init = function() {
			var themeToggle = Toggle.togglesLoaded.then(function() {
				if (Toggle.WfmGlobalLayout_personalOptions_37114) {
					return service.getTheme();
				} else {
					return $q.resolve({data:{Name:'classic'}});
				}
			});
			themeToggle.then(function(response) {
				var theme = response.data.Name ? response.data.Name : 'classic';
				$rootScope.setTheme(theme);
			}, function() {
				$rootScope.setTheme('classic');
			});
			return themeToggle;
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
