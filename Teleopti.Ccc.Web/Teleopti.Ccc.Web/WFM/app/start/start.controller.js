(function () {
	"use strict";
	var app = angular.module("wfm.start", []);
	app.controller("FeedCtrl", function ($scope, $http, $sce) {
		$http.get("../api/Anywhere/GetLandingPage")
				.then(function(response) {
					$scope.landing = response;
					$scope.landingPage = $sce.trustAsResourceUrl(($scope.landing).data);
				}),
			setTimeout(function () {
				$scope.$emit("FeedCtrl")
			}, 0);
	});
})();