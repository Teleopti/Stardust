(function() {
	'use strict';

	var permissionsModule = angular.module('wfm.permissions');
	permissionsModule.service('RolesFunctionsService', [
		'$q', 'PermissionsService', '$filter', function($q, PermissionsService, $filter) {
			var parseFunctions = function(functionTab, selectedFunctions, parentNode) {
				var selectedChildren = 0;
				functionTab.forEach(function(item) {
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


			var rolesFunctionsService = {};
			rolesFunctionsService.functionsDisplayed = [];
			rolesFunctionsService.nmbFunctionsTotal = {};
			PermissionsService.applicationFunctions.query().$promise.then(function(result) {
				parseFunctions(result, [], rolesFunctionsService.nmbFunctionsTotal);
				rolesFunctionsService.functionsDisplayed = result;
			});

			rolesFunctionsService.unselectFunction = function (functionId, selectedRole) {
				var deferred = $q.defer();
				PermissionsService.deleteFunction.query({ Id: selectedRole.Id, FunctionId: functionId }).$promise.then(function (result) {
					deferred.resolve();
				});

				PermissionsService.deleteFunction.query({ Id: selectedRole.Id, FunctionId: functionId }).$promise.then(function (result) {
					deferred.resolve();
				});

				return deferred.promise;
			};

			rolesFunctionsService.selectFunction = function (functionId, selectedRole) {
				var deferred = $q.defer();
				PermissionsService.postFunction.query({ Id: selectedRole.Id, Functions: functionId }).$promise.then(function (result) {
					deferred.resolve();
				});

				return deferred.promise;
			};

			rolesFunctionsService.refreshFunctions = function(newSelectedRoleId) {
				PermissionsService.rolesPermissions.query({ Id: newSelectedRoleId }).$promise.then(function(result) {
					var permsFunc = result.AvailableFunctions;
							console.log(permsFunc);
							console.log(i);
							console.log(permsFunc);
					parseFunctions(rolesFunctionsService.functionsDisplayed, permsFunc);
				});

			rolesFunctionsService.selectAllFunctions = function (selectedRole) {
				var functions = [];

				helperSelectAllFunctions(rolesFunctionsService.functionsDisplayed,functions);
				PermissionsService.postFunction.query({ Id: selectedRole.Id, Functions: functions });
			};

			rolesFunctionsService.unselectAllFunctions = function (selectedRole) {

				rolesFunctionsService.functionsDisplayed.forEach(function (item) {
					PermissionsService.deleteFunction.query({ Id: selectedRole.Id, FunctionId: item.FunctionId });
				});

				helperUnselectAllFunctions(rolesFunctionsService.functionsDisplayed, functions);
			};

			var helperUnselectAllFunctions = function (nodes, functions) {
				nodes.forEach(function (item) {
					item.selected = false;
					if (item.ChildFunctions && item.ChildFunctions.length !== 0) {
						helperUnselectAllFunctions(item.ChildFunctions, functions);
					}
				});
			};

			var helperSelectAllFunctions = function (nodes, functions) {

				nodes.forEach(function (item) {
					item.selected = true;
					functions.push(item.FunctionId);

					if (item.ChildFunctions && item.ChildFunctions.length !== 0) {
						helperSelectAllFunctions(item.ChildFunctions, functions);
					}
				});
			};


			};

			return rolesFunctionsService;
		}
	]);

})();