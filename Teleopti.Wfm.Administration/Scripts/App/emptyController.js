(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('emptyController', emptyController, []);

	function emptyController($scope, $http) {
		$scope.viewName = 'TOMT OCH INNEHÅLLSLÖST som en påse';

	}

})();