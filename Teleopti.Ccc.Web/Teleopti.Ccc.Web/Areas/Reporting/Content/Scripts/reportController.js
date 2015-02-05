var mainApp = angular.module('mainApp', ['reportControllers', 'ui.bootstrap']);

var reportControllers = angular.module('reportControllers', []);


personControllers.controller('reportDataController', ['$scope', '$http', function ($scope, $http) {
	$scope.excel = null;
	$scope.pdf = null;

	$scope.getPdf = function () {
		$http.get('/api/ReportData/').success(function (data) {
			$scope.pdf = data;
		});
	};

}]);