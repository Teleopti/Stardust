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

wfmCtrls.controller('PermissionsCtrl', [
	'$scope', '$stateParams', '$http', '$filter', 'Roles', 'OrganizationSelections', 'ApplicationFunctions', 'DuplicateRole', 'RolesPermissions', 'ManageRole', 'AssignFunction', 'AssignOrganizationSelection',
	function ($scope, $stateParams, $http, $filter, Roles, OrganizationSelections, ApplicationFunctions, DuplicateRole, RolesPermissions, ManageRole, AssignFunction, AssignOrganizationSelection) {
		$scope.roles = [];
		$scope.list = [];
		$scope.roleName = null;
		$scope.roleDetails = 'functionsAvailable';
		$scope.functionsDisplayed = [];
		$scope.functionsFlat = [];
		$scope.selectedRole = {};
		$scope.organization = { BusinessUnits: [{ BusinessUnit: { Sites: [] } }], DynamicOptions: [] };

		$scope.roles = Roles.get();
		OrganizationSelections.query().$promise.then(function(result) {
			// could we have directly an array from server?
			$scope.organization = { BusinessUnits: [result.BusinessUnit], DynamicOptions: result.DynamicOptions };

		});

		ApplicationFunctions.query().$promise.then(function(result) {
			$scope.functionsDisplayed = result;
			flatFunctions($scope.functionsDisplayed);
		});


		$scope.createRole = function() {
			var roleData = { Description: $scope.roleName };
			Roles.post(JSON.stringify(roleData)).$promise.then(function(result) {
				roleData.Id = result.Id;
				roleData.DescriptionText = result.DescriptionText;
				$scope.roles.unshift(roleData);
			});
		};

		$scope.copyRole = function(roleId) {
			var roleCopy = {};
			DuplicateRole.query({ Id: roleId }).$promise.then(function(result) {
				roleCopy.Id = result.Id;
				roleCopy.DescriptionText = result.DescriptionText;
				$scope.roles.unshift(roleCopy);
			});
		};

		$scope.toggleFunctionForRole = function (functionNode) {
			if (functionNode.selected) {
				AssignFunction.deleteFunctions({ Id: $scope.selectedRole, Functions: [functionNode.FunctionId] }).$promise.then(function (result) {
					functionNode.selected = false;
				});
			} else {
				AssignFunction.postFunctions({ Id: $scope.selectedRole, Functions: [functionNode.FunctionId] }).$promise.then(function (result) {
					functionNode.selected = true;
				});
			}
		};

		$scope.toggleOrganizationSelection = function (node, typeOfNode) {
			var data = {};
			data.Id = $scope.selectedRole;
			data[typeOfNode] = [node.Id];

			if (node.selected) {
				AssignOrganizationSelection.deleteData(data).$promise.then(function (result) {
					node.selected = false;
				});
			} else {
				AssignOrganizationSelection.postData(data).$promise.then(function (result) {
					node.selected = true;
				});
			}
		};

		$scope.removeRole = function (role, index) {
			if (confirm('Are you sure you want to delete this?')) {
				ManageRole.deleteRole({ Id: role.Id }).$promise.then(function(result) {
					$scope.roles.splice(index, 1);
				});
			}
		};

	$scope.updateRole = function (role) {
		ManageRole.update({ Id: role.Id, newDescription: role.DescriptionText });
	};

	$scope.showRole = function (roleId) {
		$scope.selectedRole = roleId;
		RolesPermissions.query({ Id: roleId }).$promise.then(function(result) {
			var permsFunc = result.AvailableFunctions;

			//yeah, we know, it's amazing
			$scope.organization.BusinessUnits.forEach(function(bu){
				var availableBu = $filter('filter')(result.AvailableBusinessUnits, { Id: bu.Id });
				bu.selected = availableBu.length != 0 ? true : false;
				bu.Sites.forEach(function (site) {
					var availableSite = $filter('filter')(result.AvailableSites, { Id: site.Id });
					site.selected = availableSite.length != 0 ? true : false;
					site.Teams.forEach(function (team) {
						var availableTeam = $filter('filter')(result.AvailableTeams, { Id: team.Id });
						team.selected = availableTeam.length != 0 ? true : false;
					});
				});
			});

			$scope.organization.DynamicOptions.forEach(function (dyna) {
				dyna.selected = result.AvailableDataRange === dyna.RangeOption ? true : false;
			});

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
