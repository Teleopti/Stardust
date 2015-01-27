'use strict';

var wfmCtrls = angular.module('wfmCtrls', []);

wfmCtrls.controller('MainCtrl',
    function ($scope) {

    }
);

wfmCtrls.controller('ForecastingCtrl', ['$scope', '$state',
        function ($scope, $state) {
            var startDate = moment().add(1,'months').startOf('month').toDate();
            var endDate = moment().add(2, 'months').startOf('month').toDate();
            $scope.period = { startDate: startDate, endDate: endDate }; //use moment to get first day of next month

            $scope.startPeriod = function(period) {
                $state.go('forecasting.run', { period: period }); //there's probably a better way to do that
            };
        }]
);

wfmCtrls.controller('ForecastingRunCtrl', [ '$scope', '$stateParams','$http',
        function ($scope, $stateParams, $http) {

            $scope.period = $stateParams.period;
            //api/Forecasting/forecast/QuickForecast
            $http.post('../../api/Forecasting/forecast', JSON.stringify({ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate})).
                success(function(data, status, headers, config) {
                    $scope.result = {success: true, message: 'You now have an updated forecast in your default scenario, based on last year\'s data.'};
                }).
                error(function(data, status, headers, config) {
                    $scope.result = {success: false, message: 'The forecast has failed. Please try again later'};
                });
        }]
);

wfmCtrls.controller('PermissionsCtrl', ['$scope', '$stateParams', '$http',
        function ($scope, $stateParams, $http) {
        	$scope.roles = [];
        	$scope.list = [];
        	$scope.data = brut;
        	$scope.roleDetails = 'function';

	        $http.get('../../api/Permissions/Roles').
				success(function (data, status, headers, config) {
			        $scope.roles = data;
				}).
				error(function (data, status, headers, config) {
					$scope.error = { success: false, message: 'Something has failed. Please try again later' };
				});
        	
        }]
);

var brut = [
	{
		"id": 1,
		"title": "node1",
		"nodes": [
			{
				"id": 11,
				"title": "node1.1",
				"nodes": [
					{
						"id": 110,
						"title": "node1.1.1",
						"nodes": []
					}
				]
			}
		]
	},
	{
		"id": 2,
		"title": "node2",
		"nodes": [
			{
				"id": 21,
				"title": "node2.1",
				"nodes": []
			},
			{
				"id": 22,
				"title": "node2.2",
				"nodes": []
			}
		]
	}
];