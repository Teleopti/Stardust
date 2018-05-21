angular
	.module('adminApp').service('tokenHeaderService',
	function ($cookies) {
		var tokenKey = 'accessToken';
		this.getHeaders = function () {
			var cookie = $cookies.getObject('WfmAdminAuth');
			var token = cookie ? cookie.tokenKey : null;
			if (token === null) {
				return null;
			}
			return {
				headers: { 'Authorization': 'Bearer ' + token }
			};
		};

		this.getHeadersAndParams = function (parameters) {
			var cookie = $cookies.getObject('WfmAdminAuth');
			var token = cookie ? cookie.tokenKey : null;
			if (token === null) {
				return null;
			}
			return { params: parameters, headers: { 'Authorization': 'Bearer ' + token }} ;
		};
	}
	);