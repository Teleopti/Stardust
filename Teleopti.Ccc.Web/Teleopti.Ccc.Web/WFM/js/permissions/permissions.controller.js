(function() {
	'use strict';

	var permissions = angular.module('wfm.permissions', ['ngResource']);
	permissions.controller('PermissionsCtrl', [
			'$scope', '$filter', 'PermissionsService', 'Roles', '$stateParams',
			function($scope, $filter, Permissions, Roles, $stateParams) {
				$scope.list = [];
				$scope.roleName = null;
				$scope.roleDetails = 'functionsAvailable';
				
				$scope.functionsFlat = [];
				$scope.dataFlat = [];
				$scope.selectedRole = Roles.selectedRole;
				$scope.selectedDataToggle = false;
				$scope.unselectedDataToggle = false;
				$scope.roles = [];
				
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

				Roles.list.$promise.then(function (result) {
					$scope.roles = result;
					if ($stateParams.id != null) {
						for (var i = 0; i < result.length; i++) {
							if ($stateParams.id == result[i].Id) {
								$scope.showRole(result[i]);
							}
						}
					} else {
						if (result.length > 0)
							$scope.showRole(result[0]);
					}
				});
			}
		]
	);
})();