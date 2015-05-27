(function() {
	'use strict';

	var permissions = angular.module('wfm.permissions', ['ngResource']);
	permissions.controller('PermissionsCtrl', [
			'$scope', '$filter', 'PermissionsService', 'Roles',
			function($scope, $filter, Permissions, Roles) {
				$scope.list = [];
				$scope.roleName = null;
				$scope.roleDetails = 'functionsAvailable';
				
				$scope.functionsFlat = [];
				$scope.dataFlat = [];
				$scope.selectedRole = Roles.selectedRole;
				$scope.organization = { BusinessUnit: [{ BusinessUnit: { Sites: [] } }], DynamicOptions: [] };
				$scope.selectedDataToggle = false;
				$scope.unselectedDataToggle = false;

				$scope.roles = Roles.list;
				Permissions.organizationSelections.query().$promise.then(function(result) {
					$scope.organization = { BusinessUnit: [result.BusinessUnit], DynamicOptions: result.DynamicOptions };
					flatData($scope.organization.BusinessUnit);
				});

				$scope.createRole = function() {
					Roles.createRole($scope.roleName).then(function() {
						//here a notice
						$scope.reset();
					});
				};

				$scope.reset = function() {
					$scope.roleName = "";
				};

				$scope.copyRole = function(roleId) {
					Roles.copyRole(roleId);
				};

				$scope.toggleOrganizationSelection = function(node) {
					var data = {};
					data.Id = $scope.selectedRole;

					if (node.selected) {
						data.Type = node.Type;
						data.DataId = node.Id;
						Permissions.deleteAvailableData.query(data).$promise.then(function(result) {
							node.selected = false;
						});
					} else {
						data[node.Type + 's'] = [node.Id];
						Permissions.assignOrganizationSelection.postData(data).$promise.then(function(result) {
							node.selected = true;
						});
					}
				};

				$scope.changeOption = function(option) {
					var data = {};
					data.Id = $scope.selectedRole;
					data['RangeOption'] = option;
					Permissions.assignOrganizationSelection.postData(data);
				}

				$scope.removeRole = function(role) {
					if (confirm('Are you sure you want to delete this?')) {
						Roles.removeRole(role);
					}
				};

				$scope.updateRole = function(role) {
					Permissions.manageRole.update({ Id: role.Id, newDescription: role.DescriptionText });
				};

				$scope.showRole = function(role) {
					Roles.selectRole(role);
					$scope.selectedRole = role.Id;
					Permissions.rolesPermissions.query({ Id: role.Id }).$promise.then(function(result) {
						var permsData = result.AvailableBusinessUnits.concat(result.AvailableSites.concat(result.AvailableTeams));
						$scope.dynamicOptionSelected = result.AvailableDataRange;

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

				var flatData = function(dataTab) {
					dataTab.forEach(function(item) {
						$scope.dataFlat.push(item);
						if (item.ChildNodes && item.ChildNodes.length != 0) {
							flatData(item.ChildNodes);
						}
					});
				};

			}
		]
	);

	permissions.controller('RoleFunctionsController', [
		'$scope', '$filter', 'RolesFunctions',  'Roles',
		function ($scope, $filter, RolesFunctions,  Roles) {
			$scope.unselectedFunctionsToggle = false;
			$scope.selectedFunctionsToggle = false;
			$scope.rolesService = Roles;
			$scope.rolesFunctionsService = RolesFunctions;
			$scope.selectedRole = $scope.rolesService.selectedRole;
			$scope.functionsDisplayed = [];

			$scope.$watch(function () { return Roles.selectedRole; },
				function (newSelectedRole) {
					if (!newSelectedRole.Id) return;
					$scope.selectedRole = newSelectedRole;
					RolesFunctions.refreshFunctions(newSelectedRole.Id);
				}
			);

			$scope.$watch(function () { return RolesFunctions.functionsDisplayed; },
					function (rolesFunctionsData) {
						$scope.functionsDisplayed = rolesFunctionsData;
					}
			);



			$scope.toggleFunctionForRole = function (node) {
				var functionNode = node.$modelValue;
				if (functionNode.selected) { //functionNode
					RolesFunctions.unselectFunction(functionNode.FunctionId, $scope.selectedRole).then(function () {
						functionNode.selected = false;
						node.$parentNodeScope.$modelValue.nmbSelectedChildren--;
					});
				} else {
					RolesFunctions.selectFunction(functionNode.FunctionId, $scope.selectedRole).then(function () {
						functionNode.selected = true;
						node.$parentNodeScope.$modelValue.nmbSelectedChildren++;
					});
				}
			};
		}
	]);

})();