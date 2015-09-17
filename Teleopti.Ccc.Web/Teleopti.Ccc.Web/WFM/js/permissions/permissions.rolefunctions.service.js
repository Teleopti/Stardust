(function() {
	'use strict';
	var permissionsModule = angular.module('wfm.permissions');
	permissionsModule.service('RolesFunctionsService', [
		'$q', 'PermissionsService', '$filter', function($q, PermissionsService, $filter) {
		
			var rolesFunctionsService = {};
			rolesFunctionsService.functionsDisplayed = [];
			rolesFunctionsService.nmbFunctionsTotal = {};
			rolesFunctionsService.allFunctions = false; //fixme find a common name

			PermissionsService.applicationFunctions.query().$promise.then(function(result) {
				rolesFunctionsService.parseFunctions(result, [], rolesFunctionsService.nmbFunctionsTotal, false);
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

			rolesFunctionsService.refreshFunctions = function (newSelectedRoleId) {
				var deferred = $q.defer();
				PermissionsService.rolesPermissions.query({ Id: newSelectedRoleId }).$promise.then(function(result) {
					var permsFunc = result.AvailableFunctions;

					rolesFunctionsService.allFunctions = ($filter('filter')(permsFunc, { FunctionCode: 'All' }, true)).length !== 0;
				
					rolesFunctionsService.parseFunctions(rolesFunctionsService.functionsDisplayed, permsFunc, {}, rolesFunctionsService.allFunctions);
					deferred.resolve();
				});
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

			    helperSelectAllFunctions(rolesFunctionsService.functionsDisplayed,functions);
			    PermissionsService.postFunction.query({ Id: selectedRole.Id, Functions: functions });
			};

			rolesFunctionsService.unselectAllFunctions = function (selectedRole) {

				rolesFunctionsService.functionsDisplayed.forEach(function (item) {
					PermissionsService.deleteFunction.query({ Id: selectedRole.Id, FunctionId: item.FunctionId });
				});

				helperUnselectAllFunctions(rolesFunctionsService.functionsDisplayed);
			};

			var helperUnselectAllFunctions = function (nodes) {
				nodes.forEach(function (item) {

					item.selected = false;
					if (item.ChildFunctions && item.ChildFunctions.length !== 0) {
						helperUnselectAllFunctions(item.ChildFunctions);
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