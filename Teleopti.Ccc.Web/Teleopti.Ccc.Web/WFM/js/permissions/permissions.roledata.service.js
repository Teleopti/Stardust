(function() {
	'use strict';

	var permissionsModule = angular.module('wfm.permissions');
	permissionsModule.service('RoleDataService', [
		'$q', 'PermissionsService', '$filter', function($q, PermissionsService, $filter) {
			var dataFlat = [];
			var flatData = function (dataTab) {
				dataTab.forEach(function (item) {
					dataFlat.push(item);
					if (item.ChildNodes && item.ChildNodes.length != 0) {
						flatData(item.ChildNodes);
					}
				});
			};
			var roleDataService = {};
			roleDataService.organization = { BusinessUnit: [{ BusinessUnit: { Sites: [] } }], DynamicOptions: [] };
			PermissionsService.organizationSelections.query().$promise.then(function (result) {
				roleDataService.organization = { BusinessUnit: [result.BusinessUnit], DynamicOptions: result.DynamicOptions };
				flatData(roleDataService.organization.BusinessUnit);
			});

			roleDataService.deleteAvailableData = function (selectedRole, type, id) {
				var data = {};
				data.Type = type;
				data.DataId = id;
				data.Id = selectedRole;
				var deferred = $q.defer();
				PermissionsService.deleteAvailableData.query(data).$promise.then(function (result) {
					deferred.resolve();
				});

				return deferred.promise;
			};

			roleDataService.assignOrganizationSelection = function(selectedRole, type, id) {
				var data = {};
				data.Id = selectedRole;
				data[type + 's'] = [id];
				var deferred = $q.defer();
				PermissionsService.assignOrganizationSelection.postData(data).$promise.then(function (result) {
					deferred.resolve();
				});
				return deferred.promise;
			}
	
			roleDataService.assignAuthorizationLevel = function (selectedRole, option) {
				var data = {};
				data.Id = selectedRole;
				data['RangeOption'] = option;
				PermissionsService.assignOrganizationSelection.postData(data);
			}

			return roleDataService;
		}
	]);
})();