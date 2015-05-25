(function () {
	'use strict';

	var rolesService = angular.module('rolesService', []);
	rolesService.service('Roles', ['$q', 'Permissions',
		function ($q, Permissions) {
			var roles = {};

			roles.selectedRole = {};

			roles.list = Permissions.roles.get();

			roles.createRole = function (roleName) {
				var deferred = $q.defer();
				var roleData = { Description: roleName };
				Permissions.roles.post(JSON.stringify(roleData)).$promise.then(function (result) {
					roleData.Id = result.Id;
					roleData.DescriptionText = result.DescriptionText;
					roles.list.unshift(roleData);
					deferred.resolve();
				});
				return deferred.promise;
			};

			roles.copyRole = function (roleId) {
				var roleCopy = {};
				var deferred = $q.defer();
				Permissions.duplicateRole.query({ Id: roleId }).$promise.then(function (result) {
					roleCopy.Id = result.Id;
					roleCopy.DescriptionText = result.DescriptionText;
					roles.list.unshift(roleCopy);
					deferred.resolve();
				});
				return deferred.promise;
			};

			roles.removeRole = function (role) {
				Permissions.manageRole.deleteRole({ Id: role.Id }).$promise.then(function (result) {
					roles.list.splice(roles.list.indexOf(role), 1);
				});
			};

			roles.selectRole = function (role) {
				roles.selectedRole = role;
			}

			return roles;
		}
	]);
	rolesService.service('Functions', [
		'$q', 'Permissions', function ($q, Permissions) {
			var functions = {};
			functions.list = Permissions.applicationFunctions.query();
			functions.getDisplayedFunctions = function () {
				var displayedFunction = functions.list;
				return displayedFunction;
			};
			return functions;
		}
	]);
	rolesService.service('RolesFunctions', [
			'$q', 'Permissions', '$filter', function ($q, Permissions, $filter) {
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


				var rolesFunctions = {};
				rolesFunctions.functionsDisplayed = [];
				rolesFunctions.nmbFunctionsTotal = {};
				Permissions.applicationFunctions.query().$promise.then(function (result) {
					parseFunctions(result, [], rolesFunctions.nmbFunctionsTotal);
					rolesFunctions.functionsDisplayed = result;
				});

				rolesFunctions.unselectFunction = function (functionNode, selectedRole) {
					var deferred = $q.defer();
					Permissions.deleteFunction.query({ Id: selectedRole.Id, FunctionId: [functionNode] }).$promise.then(function (result) {
						deferred.resolve();
					});
					return deferred.promise;
				};

				rolesFunctions.selectFunction = function (functionNode, selectedRole) {
					var deferred = $q.defer();
					Permissions.postFunction.query({ Id: selectedRole.Id, Functions: [functionNode] }).$promise.then(function (result) {
						deferred.resolve();
					});
					return deferred.promise;
				};

				rolesFunctions.refreshFunctions = function (newSelectedRoleId) {
					Permissions.rolesPermissions.query({ Id: newSelectedRoleId }).$promise.then(function (result) {
						var permsFunc = result.AvailableFunctions;
						parseFunctions(rolesFunctions.functionsDisplayed, permsFunc);
					});
				};

				return rolesFunctions;
			}
	]);

})();