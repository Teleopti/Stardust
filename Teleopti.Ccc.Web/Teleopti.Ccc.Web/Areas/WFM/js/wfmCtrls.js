'use strict';

var wfmCtrls = angular.module('wfmCtrls', []);

wfmCtrls.controller('MainCtrl',
    function ($scope) {

    }
);

wfmCtrls.controller('ForecastingCtrl', ['$scope', '$state',
        function ($scope, $state) {
        	var startDate = moment().add(1, 'months').startOf('month').toDate();
        	var endDate = moment().add(2, 'months').startOf('month').toDate();
        	$scope.period = { startDate: startDate, endDate: endDate }; //use moment to get first day of next month

        	$scope.startPeriod = function (period) {
        		$state.go('forecasting.run', { period: period }); //there's probably a better way to do that
        	};
        }]
);

wfmCtrls.controller('ForecastingRunCtrl', ['$scope', '$stateParams', '$http',
        function ($scope, $stateParams, $http) {

        	$scope.period = $stateParams.period;
        	//api/Forecasting/forecast/QuickForecast
        	$http.post('../../api/Forecasting/forecast', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate })).
                success(function (data, status, headers, config) {
                	$scope.result = { success: true, message: 'You now have an updated forecast in your default scenario, based on last year\'s data.' };
                }).
                error(function (data, status, headers, config) {
                	$scope.result = { success: false, message: 'The forecast has failed. Please try again later' };
                });
        }]
);

wfmCtrls.controller('PermissionsCtrl', ['$scope', '$stateParams', '$http', '$filter', 'Roles', 'ApplicationFunctions',
function ($scope, $stateParams, $http, $filter, Roles, ApplicationFunctions) {
        	$scope.roles = [];
        	$scope.list = [];
			$scope.roleName = null;
        	$scope.roleDetails = 'functionsAvailable';
        	$scope.functionsDisplayed = [];
        	$scope.functionsFlat = [];
        	
        	$scope.roles = Roles.query();
        	
        	ApplicationFunctions.query().$promise.then(function (result) {
        		$scope.functionsDisplayed = result;
		        flatFunctions($scope.functionsDisplayed);
        	});

        	$scope.createRole = function () {
        		var roleData = { DescriptionText: $scope.roleName };
        		$http.post('/', roleData)
					.success(function (data, status, headers, config) {
							$scope.roles.push(roleData);
							console.log("New role added");
					})
					.error(function (data, status, headers, config) {
						$scope.error = { success: false, message: 'Something has failed. Please try again later' };
					});
        	};

        	$scope.duplicateRole = function (roleId) {
        		$http.get('../../api/Permissions/Roles/' + roleId ).
								success(function (data, status, headers, config) {
									var dupName = { DescriptionText: data.DescriptionText + " copy" };
									$scope.roles.push(dupName);
									console.log("Role duplicated");
								}).error(function (data, status, headers, config) {
									$scope.error = { success: false, message: 'Something has failed. Please try again later' };
								});
        	};

			// TODO should be refator in a service
        	$http.get('../../api/Permissions/OrganizationSelection').
							success(function (data, status, headers, config) {
								$scope.organization = data;
							}).
							error(function (data, status, headers, config) {
								$scope.error = { success: false, message: 'Something has failed. Please try again later' };
							});



        	$scope.showRole = function (roleId) {
        		$http.get('../../api/Permissions/Roles/' + roleId).
								success(function (data, status, headers, config) {
									var permsFunc = data.AvailableFunctions;
									$scope.functionsFlat.forEach(function (item) {
										var availableFunctions = $filter('filter')(permsFunc, { Id: item.FunctionId });
										item.selected = availableFunctions.length != 0  ? true : false;
									});
								}).error(function (data, status, headers, config) {
									$scope.error = { success: false, message: 'Something has failed. Please try again later' };
								});
        	};

        	var flatFunctions = function (functionTab) {
        		functionTab.forEach(function (item) {
        			$scope.functionsFlat.push(item);
        			if (item.ChildFunctions.length != 0) {
        				flatFunctions(item.ChildFunctions);
        			}
        		});
        	}
        }]
);
