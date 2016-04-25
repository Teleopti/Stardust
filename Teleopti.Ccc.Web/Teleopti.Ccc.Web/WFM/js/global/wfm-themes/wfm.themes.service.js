
(function() {﻿
	'use strict';﻿
	angular﻿.module('wfm.themes')﻿.service('ThemeService', ['$http', function($http) {
		var service = {};

		service.setTheme = function(theme) {
			if (theme === "dark") {
				document.getElementById(theme).setAttribute("checked", "true");
			}

			document.getElementById('themeModules').setAttribute('href', 'dist/modules_' + theme + '.min.css');
			document.getElementById('themeStylesheet').setAttribute('href', 'dist/style_' + theme + '.min.css');
		};

		service.getTheme = function() {
			return $http.get('../api/Theme');
		};

		service.init = function() {
			service.getTheme().success(function(data) {
				if (data.Name !== null)
				service.setTheme(data.Name);
			});
		};

		service.saveTheme = function(name) {
			$http.post("../api/Theme/Change", {
				Name: name
			});
		};

		return service;
	}]);﻿
})();
