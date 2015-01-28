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

wfmCtrls.controller('PermissionsCtrl', ['$scope', '$stateParams', '$http', '$filter',
        function ($scope, $stateParams, $http, $filter) {
        	$scope.roles = [];
        	$scope.list = [];
        	$scope.roleDetails = 'function';
        	$scope.functions = [];
	        $scope.functionsDisplayed = [];
			
	        $http.get('../../api/Permissions/Roles').
				success(function (data, status, headers, config) {
			        $scope.roles = data;
				}).
				error(function (data, status, headers, config) {
					$scope.error = { success: false, message: 'Something has failed. Please try again later' };
				});

        	$http.get('../../api/Permissions/ApplicationFunctions').
				success(function (data, status, headers, config) {
					$scope.functions = data.Functions;
					$scope.functionsDisplayed = data;
					$scope.functionsFlat = [];
					
					var flatFunctions = function(functionTab) { 
						functionTab.forEach(function(item) {
							$scope.functionsFlat.push(item);
							if (item.ChildFunctions.length != 0) {
								flatFunctions(item.ChildFunctions);
							}
						});
					}

			        flatFunctions($scope.functionsDisplayed);

		        }).
				error(function (data, status, headers, config) {
					$scope.error = { success: false, message: 'Something has failed. Please try again later' };
				});

        	$scope.showRole = function (roleId) {
        		$http.get('../../api/Permissions/Roles/'+roleId).
								success(function (data, status, headers, config) {
									var permsFunc = data.AvailableFunctions;
				        $scope.functionsFlat.forEach(function(item) {
					        var availableFunctions = $filter('filter')(permsFunc, { Id: item.FunctionId });
					        console.log(item, availableFunctions.length);
					        item.selected = availableFunctions.length != 0 ? true : false;
				        });
			        }).error(function (data, status, headers, config) {
									$scope.error = { success: false, message: 'Something has failed. Please try again later' };
								});
        	};
        }]
);
