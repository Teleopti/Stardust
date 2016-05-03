
(function() {﻿
	'use strict';﻿
	angular﻿.module('wfm.themes')﻿.service('ThemeService', ['$http','$q', function($http,$q) {
		var service = {};

		service.setTheme = function(theme) {
			if (theme === "dark") {
				document.getElementById('darkTheme').checked = true;
			}else{
				document.getElementById('darkTheme').checked = false;
			}
			modifyDOMHeader(theme);

		};
		var modifyDOMHeader = function(theme){
			var styleElements = ["Modules","Style"];
			styleElements.forEach(function(element){
				var themeComponent = document.getElementById('theme'+element);
				var hash = extractHash(themeComponent);
				themeComponent.setAttribute('href','dist/'+element.toLowerCase()+'_'+ theme +'.min.css'+hash);
			})

		}
		var extractHash = function(element){
			var href = element.href;
			var hashvalue = href.match("\\?(.*)")//[^\\?]*$
			if (hashvalue == null)
				return ""
			else
				return hashvalue[0]

		};

		service.getTheme = function() {
			return $http.get('../api/Theme');
		};

		service.init = function() {
			var deferred = $q.defer();
			service.getTheme().then(function(response) {
				if (response.data.Name !== null)
					service.setTheme(response.data.Name);
				else
					service.setTheme('classic');
				deferred.resolve(response);
			});
			return deferred.promise;
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
