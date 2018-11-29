(function() {
	'use strict';
	angular
	.module('adminAppHttp', [])
	.factory('httpInterceptor', function ($q) {

		return {
			// optional method
			'request': function(config) {
				// do something on success
				return config;
			},

			// optional method
			'requestError': function(rejection) {
				// do something on error
        return(rejection);
			},

			// optional method
			'response': function(response) {
				// do something on success
				return response;
			},

			// optional method
			'responseError': function(rejection) {
				// do something on error
				//if (rejection.status === 401) {
    //      window.location = "#/login";
    //    }
				return $q.reject(rejection);
			}
		};
	});

})();
