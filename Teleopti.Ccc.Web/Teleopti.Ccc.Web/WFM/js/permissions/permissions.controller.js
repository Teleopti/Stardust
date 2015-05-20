'use strict';

var permissions = angular.module('wfm.permissions', []);
permissions.controller('PermissionsCtrl', [
	'$scope', '$filter', 'Permissions', 'Roles',
	function ($scope, $filter, Permissions, Roles) {
		$scope.list = [];
		$scope.roleName = null;
		$scope.roleDetails = 'functionsAvailable';
		$scope.functionsDisplayed = [];
		$scope.functionsFlat = [];
		$scope.dataFlat = [];
		$scope.selectedRole = Roles.selectedRole;
		$scope.organization = { BusinessUnit: [{ BusinessUnit: { Sites: [] } }], DynamicOptions: [] };
		$scope.selectedFunctionsToggle = false;
		$scope.selectedDataToggle = false;
		$scope.unselectedFunctionsToggle = false;
		$scope.unselectedDataToggle = false;
		
		$scope.roles = Roles.list;
		Permissions.organizationSelections.query().$promise.then(function (result) {
			$scope.organization = { BusinessUnit: [result.BusinessUnit], DynamicOptions: result.DynamicOptions };
			flatData($scope.organization.BusinessUnit);
		});

		Permissions.applicationFunctions.query().$promise.then(function (result) {
			parseFunctions(result, [], $scope.nmbFunctionsTotal);
			$scope.functionsDisplayed = result;
		});


		$scope.createRole = function () {
			Roles.createRole($scope.roleName).then(function() {
				//here a notice
				$scope.reset();
			});
		};

		$scope.reset = function () {
			$scope.roleName = "";
		};

		$scope.copyRole = function (roleId) {
			Roles.copyRole(roleId);
		};

		$scope.toggleFunctionForRole = function (node) {
			var functionNode = node.$modelValue;
			if (functionNode.selected) { //functionNode
				Permissions.deleteFunction.query({ Id: $scope.selectedRole, FunctionId: [functionNode.FunctionId] }).$promise.then(function (result) {
					functionNode.selected = false;
					node.$parentNodeScope.$modelValue.nmbSelectedChildren--;
				});
			} else {
				Permissions.postFunction.query({ Id: $scope.selectedRole, Functions: [functionNode.FunctionId] }).$promise.then(function (result) {
					functionNode.selected = true;
					node.$parentNodeScope.$modelValue.nmbSelectedChildren++;
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

		$scope.changeOption = function (option) {
			var data = {};
			data.Id = $scope.selectedRole;
			data['RangeOption'] = option;
			Permissions.assignOrganizationSelection.postData(data);
		}

		$scope.removeRole = function (role) {
			if (confirm('Are you sure you want to delete this?')) {
				Roles.removeRole(role);
			}
		};

		$scope.updateRole = function (role) {
			Permissions.manageRole.update({ Id: role.Id, newDescription: role.DescriptionText });
		};

		$scope.showRole = function (role) {
			Roles.selectRole(role);
			$scope.selectedRole = role.Id;
			Permissions.rolesPermissions.query({ Id: role.Id }).$promise.then(function (result) {
				var permsFunc = result.AvailableFunctions;
				var permsData = result.AvailableBusinessUnits.concat(result.AvailableSites.concat(result.AvailableTeams));
				$scope.dynamicOptionSelected = result.AvailableDataRange;
	
				parseFunctions($scope.functionsDisplayed, permsFunc);

				$scope.dataFlat.forEach(function (item) {
					var availableData = $filter('filter')(permsData, { Id: item.Id });
					item.selected = availableData.length != 0 ? true : false;
				});
			});
		};

		$scope.nbSelected = function($nodes) {
			var nb = 0;
			$nodes.forEach(function($node) {
				if ($node.$modelValue.selected)
					nb++;
			});
			return nb;
		};

		var parseFunctions = function (functionTab, selectedFunctions, parentNode) {
			var selectedChildren = 0;
			functionTab.forEach(function (item) { 
				var availableFunctions = $filter('filter')(selectedFunctions, { Id: item.FunctionId });
				item.selected = false;
				if (availableFunctions.length != 0) {
					item.selected = true;
					selectedChildren++;
				}
				if (item.ChildFunctions.length != 0) { 
					parseFunctions(item.ChildFunctions, selectedFunctions, item);
				}
			});
			if (parentNode) {
				parentNode.nmbSelectedChildren = selectedChildren;
			}
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

