(function () {
	'use strict';
	var permissionsModule = angular.module('wfm.permissions');
	permissionsModule.service('RolesFunctionsService', [
		'$q', 'PermissionsService', '$filter', function ($q, PermissionsService, $filter) {

			var rolesFunctionsService = {};
			rolesFunctionsService.functionsDisplayed = [];
			rolesFunctionsService.nmbFunctionsTotal = {};
			rolesFunctionsService.allFunctions = false; //fixme find a common name

			rolesFunctionsService.init = function () {
				var deferred = $q.defer();
				PermissionsService.applicationFunctions.query().$promise.then(function (result) {
					rolesFunctionsService.functionsDisplayed = result;
					deferred.resolve();
				});
				return deferred.promise;
			};

			rolesFunctionsService.unselectFunction = function (functionId, selectedRole) {
				var deferred = $q.defer();
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

			rolesFunctionsService.refreshFunctions = function (newSelectedRoleId) {
				var deferred = $q.defer();
				var refreshCallback = function () {
					PermissionsService.rolesPermissions.query({ Id: newSelectedRoleId }).$promise.then(function (result) {
						var permsFunc = result.AvailableFunctions;
						rolesFunctionsService.allFunctions = ($filter('filter')(permsFunc, { FunctionCode: 'All' }, true)).length !== 0;
						rolesFunctionsService.parseFunctions(rolesFunctionsService.functionsDisplayed, permsFunc, {}, rolesFunctionsService.allFunctions);
						deferred.resolve();
					});
				};

				if (rolesFunctionsService.functionsDisplayed.length === 0) {
					rolesFunctionsService.init().then(refreshCallback);
				} else {
					refreshCallback();
				}
				
				
				return deferred.promise;
			};

			rolesFunctionsService.parseFunctions = function (functionTab, selectedFunctions, parentNode, allSelected) {

				var selectedChildren = 0;
				functionTab.forEach(function (item) {
					var availableFunctions = $filter('filter')(selectedFunctions, { Id: item.FunctionId });
					item.selected = false;
					if (availableFunctions.length !== 0 || allSelected) {
						item.selected = true;
						selectedChildren++;
					}
					if (item.ChildFunctions.length !== 0) {
						rolesFunctionsService.parseFunctions(item.ChildFunctions, selectedFunctions, item, allSelected);
					}
				});
				if (parentNode) {
					parentNode.nmbSelectedChildren = selectedChildren;
				}
			};

			rolesFunctionsService.selectAllFunctions = function (selectedRole) {
				var functions = [];

				helperSelectAllFunctions(rolesFunctionsService.functionsDisplayed, functions);
				PermissionsService.postFunction.query({ Id: selectedRole.Id, Functions: functions });
			};

			rolesFunctionsService.unselectAllFunctions = function (selectedRole) {
				var functions = [];
				helperUnselectAllFunctions(rolesFunctionsService.functionsDisplayed, functions);
				PermissionsService.deleteAllFunction.query({
					Id: selectedRole.Id,
					FunctionId: rolesFunctionsService.functionsDisplayed[0].FunctionId,
					Functions: functions
				});
			};

			var helperUnselectAllFunctions = function (nodes,functions) {
				nodes.forEach(function (item) {
					functions.push(item.FunctionId);
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

			return rolesFunctionsService;
		}
	]);
})();