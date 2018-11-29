(function () {
	'use strict';

	angular
        .module('adminApp')
        .controller('hangfireController', hangfireController, ['tokenHeaderService'])
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

	function hangfireController($http, tokenHeaderService, $scope) {
		$http.get("./Hangfire/GetUrl", tokenHeaderService.getHeaders()).then(function (data) {
			$scope.HangfireUrl = data;
		}).catch(function (xhr, ajaxOptions, thrownError) {
			console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
		});
	}
})();
