angular
		 .module('adminApp').service('tokenHeaderService', function () {
	var tokenKey = 'accessToken';

	this.getHeaders = function() {
		var token = sessionStorage.getItem(tokenKey);
		if (token === null) {
			return null;
		}
		return {
			headers: { 'Authorization': 'Bearer ' + token }
		};
	}
	}
	)