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

wfmCtrls.controller('PermissionsCtrl', ['$scope', '$stateParams', '$http', '$filter', 'Roles', 'OrganizationSelections', 'ApplicationFunctions', 'DuplicateRole', 'RolesFunctions', 'ManageRole',
function ($scope, $stateParams, $http, $filter, Roles, OrganizationSelections, ApplicationFunctions, DuplicateRole, RolesFunctions, ManageRole) {
	$scope.roles = [];
	$scope.list = [];
	$scope.roleName = null;
	$scope.roleDetails = 'functionsAvailable';
	$scope.functionsDisplayed = [];
	$scope.functionsFlat = [];

	$scope.roles = Roles.get();
	$scope.organization = OrganizationSelections.query();

	ApplicationFunctions.query().$promise.then(function (result) {
		$scope.functionsDisplayed = result;
		flatFunctions($scope.functionsDisplayed);
	});


	$scope.createRole = function () {
		var roleData = { Description: $scope.roleName };
		Roles.post(JSON.stringify(roleData)).$promise.then(function (result) {
			roleData.Id = result.Id;
			roleData.DescriptionText = result.DescriptionText;
			$scope.roles.push(roleData);
		});
	};

	$scope.copyRole = function (roleId) {
		var roleCopy = {};
		DuplicateRole.query({ Id: roleId }).$promise.then(function (result) {
			roleCopy.Id = result.Id;
			roleCopy.DescriptionText = result.DescriptionText;
			$scope.roles.push(roleCopy);
		});
	};


	$scope.removeRole = function (role, index) {
		ManageRole.deleteRole({ Id: role.Id }).$promise.then(function (result) {
			$scope.roles.splice(index, 1);
		});
	};

	$scope.updateRole = function (role) {
		ManageRole.update({ Id: role.Id, NewDescription: JSON.stringify({ NewDescription: role.DescriptionText }) })
			.$promise.then(function (result) {
 // ?
		});
	};

	$scope.showRole = function (roleId) {
		RolesFunctions.query({ Id: roleId }).$promise.then(function (result) {
			var permsFunc = result.AvailableFunctions;
			$scope.functionsFlat.forEach(function (item) {
				var availableFunctions = $filter('filter')(permsFunc, { Id: item.FunctionId });
				item.selected = availableFunctions.length != 0 ? true : false;
			});
		});
	};

	var flatFunctions = function (functionTab) {
		functionTab.forEach(function (item) {
			$scope.functionsFlat.push(item);
			if (item.ChildFunctions.length != 0) {
				flatFunctions(item.ChildFunctions);
			}
		});
	};
}]
);
