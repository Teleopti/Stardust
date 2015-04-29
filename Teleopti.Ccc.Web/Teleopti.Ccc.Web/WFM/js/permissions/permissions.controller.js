'use strict';

var permissions = angular.module('wfm.permissions',[]);
permissions.controller('PermissionsCtrl', [
	'$scope', '$filter', 'Permissions',
	function ($scope, $filter, Permissions) {
		$scope.roles = [];
		$scope.list = [];
		$scope.roleName = null;
		$scope.roleDetails = 'functionsAvailable';
		$scope.functionsDisplayed = [];
		$scope.functionsFlat = [];
		$scope.dataFlat = [];
		$scope.selectedRole = {};
		$scope.organization = { BusinessUnit: [{ BusinessUnit: { Sites: [] } }], DynamicOptions: [] };

		$scope.roles = Permissions.roles.get();
		Permissions.organizationSelections.query().$promise.then(function (result) {
			$scope.organization = { BusinessUnit: [result.BusinessUnit], DynamicOptions: result.DynamicOptions };
			flatData($scope.organization.BusinessUnit);
		});

		Permissions.applicationFunctions.query().$promise.then(function (result) {
			$scope.functionsDisplayed = result;
			flatFunctions($scope.functionsDisplayed);
		});


		$scope.createRole = function () {
			var roleData = { Description: $scope.roleName };
			Permissions.roles.post(JSON.stringify(roleData)).$promise.then(function (result) {
				roleData.Id = result.Id;
				roleData.DescriptionText = result.DescriptionText;
				$scope.roles.unshift(roleData);
			});
		};

		$scope.reset = function () {
			$scope.roleName = "";
			$scope.form.$setPristine();
		};

		$scope.copyRole = function (roleId) {
			var roleCopy = {};
			Permissions.duplicateRole.query({ Id: roleId }).$promise.then(function (result) {
				roleCopy.Id = result.Id;
				roleCopy.DescriptionText = result.DescriptionText;
				$scope.roles.unshift(roleCopy);
			});
		};

		$scope.toggleFunctionForRole = function (functionNode) {
			if (functionNode.selected) {
				Permissions.deleteFunction.query({ Id: $scope.selectedRole, FunctionId: [functionNode.FunctionId] }).$promise.then(function (result) {
					functionNode.selected = false;
				});
			} else {
				Permissions.postFunction.query({ Id: $scope.selectedRole, Functions: [functionNode.FunctionId] }).$promise.then(function (result) {
					functionNode.selected = true;
				});
			}
		};

		$scope.toggleOrganizationSelection = function (node) {
			var data = {};
			data.Id = $scope.selectedRole;

			if (node.selected) {
				data.Type = node.Type;
				data.DataId = node.Id;
				Permissions.deleteAvailableData.query(data).$promise.then(function (result) {
					node.selected = false;
				});
			} else {
				data[node.Type + 's'] = [node.Id];
				Permissions.assignOrganizationSelection.postData(data).$promise.then(function (result) {
					node.selected = true;
				});
			}
		};

		$scope.changeRangeOption = function (node) {
			if (node.selected && node.RangeOption !== 0) {
				$scope.organization.DynamicOptions[0].selected = true;
				node.selected = false;
				$scope.changeRangeOption($scope.organization.DynamicOptions[0]);
			} else {
				var data = {};
				data.Id = $scope.selectedRole;
				data['RangeOption'] = node.RangeOption;
				Permissions.assignOrganizationSelection.postData(data).$promise.then(function (result) {
					$scope.organization.DynamicOptions.forEach(function (option) {
						option.selected = false;
					});
					node.selected = true;
				});
			}
		};

		$scope.changeOption = function (option) {
			var data = {};
			data.Id = $scope.selectedRole;
			data['RangeOption'] = option;
			Permissions.assignOrganizationSelection.postData(data);
		}

		$scope.removeRole = function (role, index) {
			if (confirm('Are you sure you want to delete this?')) {
				Permissions.manageRole.deleteRole({ Id: role.Id }).$promise.then(function (result) {
					$scope.roles.splice(index, 1);
				});
			}
		};

		$scope.updateRole = function (role) {
			Permissions.manageRole.update({ Id: role.Id, newDescription: role.DescriptionText });
		};

		$scope.showRole = function (roleId) {
			$scope.selectedRole = roleId;
			Permissions.rolesPermissions.query({ Id: roleId }).$promise.then(function (result) {
				var permsFunc = result.AvailableFunctions;
				var permsData = result.AvailableBusinessUnits.concat(result.AvailableSites.concat(result.AvailableTeams));
				$scope.dynamicOptionSelected = result.AvailableDataRange;
				

				$scope.functionsFlat.forEach(function (item) {
					var availableFunctions = $filter('filter')(permsFunc, { Id: item.FunctionId });
					item.selected = availableFunctions.length != 0 ? true : false;
				});

				$scope.dataFlat.forEach(function (item) {
					var availableData = $filter('filter')(permsData, { Id: item.Id });
					item.selected = availableData.length != 0 ? true : false;
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

		var flatData = function (dataTab) {
			dataTab.forEach(function (item) {
				$scope.dataFlat.push(item);
				if (item.ChildNodes && item.ChildNodes.length != 0) {
					flatData(item.ChildNodes);
				}
			});
		};

	}]
);