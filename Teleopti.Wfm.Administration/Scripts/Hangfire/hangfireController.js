(function () {
	'use strict';

	angular
        .module('adminApp')
        .controller('hangfireController', hangfireController)
		.directive("jqTable", function () {
			return function (scope, element, attrs) {
				scope.$watch("HangfireUrl", function (value) {
					var val = value || null;
					if (val) {
						// TODO make it work with angular not changing DOM objects
						document.getElementById("hangfireDashboardFrame").src = value;
					}
				});
			};
	});

	function hangfireController($http, $scope) {
		$http.get("./Hangfire/GetUrl")
			.then(function (response) {
				$scope.HangfireUrl = response.data;
		});
	}
})();
