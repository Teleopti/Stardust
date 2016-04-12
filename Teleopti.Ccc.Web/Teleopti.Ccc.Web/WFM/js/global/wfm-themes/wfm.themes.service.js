
(function() {﻿
	'use strict';﻿
	angular﻿.module('wfm.themes')﻿.service('ThemeService', ['$resource', function($resource) {
		var service = {};

  service.setTheme = function(theme){
   document.getElementById('themeModules').setAttribute('href', 'dist/modules_' + theme + '.min.css');
   document.getElementById('themeStylesheet').setAttribute('href', 'dist/style_' + theme + '.min.css');
  };

		service.getTheme = function(){
			return $resource('../api/Theme', {}, {
				query: {
					method: 'GET',
					isArray: false
				}
			}).query().$promise;
		};

		service.init = function(){
			service.getTheme().then(function(data){
				service.setTheme(data);
			});
		}

		service.saveTheme = function(name){
			return $resource('../api/Theme', {}, {
					post: { method: 'POST', params: name }
			}).post(name).$promise.then(function(){});
		};

		return service;
	}]);﻿
})();
