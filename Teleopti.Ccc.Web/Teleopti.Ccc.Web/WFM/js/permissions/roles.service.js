﻿(function () {
	'use strict';

	var rolesService = angular.module('wfm.permissions');
	rolesService.service('Roles', ['$q', 'PermissionsService',
		function ($q, PermissionsService) {
			var roles = {};

			roles.selectedRole = {};

			roles.list = PermissionsService.roles.get();

			roles.createRole = function (roleName) {
				var deferred = $q.defer();
				var roleData = { Description: roleName };
				PermissionsService.roles.post(JSON.stringify(roleData)).$promise.then(function (result) {
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
				PermissionsService.duplicateRole.query({ Id: roleId }).$promise.then(function (result) {
					roleCopy.Id = result.Id;
					roleCopy.DescriptionText = result.DescriptionText;
					roles.list.unshift(roleCopy);
					deferred.resolve();
				});
				return deferred.promise;
			};

			roles.removeRole = function (role) {
				PermissionsService.manageRole.deleteRole({ Id: role.Id }).$promise.then(function (result) {
					roles.list.splice(roles.list.indexOf(role), 1);
				});
			};

			roles.selectRole = function (role) {
				roles.selectedRole = role;
			}

			return roles;
		}
	]);
})();