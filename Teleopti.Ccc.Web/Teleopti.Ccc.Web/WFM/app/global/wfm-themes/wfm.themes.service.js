(function() {
	'use strict';

	angular
	.module('wfm.themes')
	.factory('ThemeService', ThemeService);

	ThemeService.$inject = ['$http', '$rootScope', '$q', 'Toggle'];

	function ThemeService($http, $rootScope, $q, Toggle) {
		var service = {
			getTheme: getTheme,
			init: init,
			saveTheme: saveTheme
		};

		return service;

		function getTheme() {
			return $http.get('../api/Theme');
		}

		function init(){
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
		}

		function saveTheme(name, overlay) {
			if (name === 'classic' || name === 'dark') {
				$http.post("../api/Theme/Change", {
					Name: name,
					Overlay: overlay
				});
			}
		}

	}
})();﻿
