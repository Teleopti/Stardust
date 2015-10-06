(function() {
	'use strict';

	var permissionsModule = angular.module('wfm.permissions');
	permissionsModule.service('RoleDataService', [
		'$q', 'PermissionsService', '$filter', function($q, PermissionsService, $filter) {
			var dataFlat = [];
			
			var flatData = function (dataTab) {
				dataTab.forEach(function (item) {
					dataFlat.push(item);
					if (item.ChildNodes && item.ChildNodes.length !== 0) {
						flatData(item.ChildNodes);
					} else {
						item.ChildNodes = [];
						item.selected = false;
					}
				});
			};
			var roleDataService = {};
			roleDataService.dynamicOptionSelected = 0;
			roleDataService.organization = { BusinessUnit: [{ BusinessUnit: { Sites: [] } }], DynamicOptions: [] };
			roleDataService.displayedData = {};
			
			roleDataService.refreshOrganizationSelection = function() {
				PermissionsService.organizationSelections.query().$promise.then(function (result) {
					roleDataService.organization = { BusinessUnit: [result.BusinessUnit], DynamicOptions: result.DynamicOptions };
					flatData(roleDataService.organization.BusinessUnit);
				});
			};

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

			roleDataService.deleteAllNodes = function (selectedRole, dataNodes) {
				var data = {};
				data.Id = selectedRole;

				dataNodes.forEach(function (node) {
					var attributeName = node.type + 's';
					if (!data[attributeName]) {
						data[attributeName] = [];
					}
					data[attributeName].push(node.id);
				});

				PermissionsService.deleteAllData.query(data);
			};

			roleDataService.assignOrganizationSelection = function(selectedRole, dataNodes) {
				var data = {};
				data.Id = selectedRole;
				dataNodes.forEach(function (node) {
					var attributeName = node.type + 's';
					if (!data[attributeName]) {
						data[attributeName]= [];
					}
					data[attributeName].push(node.id);
				});

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
				roleDataService.dynamicOptionSelected = option;
				PermissionsService.assignOrganizationSelection.postData(data);
			}

			roleDataService.refreshpermissions = function (newSelectedRoleId) {
				PermissionsService.rolesPermissions.query({ Id: newSelectedRoleId }).$promise.then(function (result) {
					var permsData = result.AvailableBusinessUnits.concat(result.AvailableSites.concat(result.AvailableTeams));
					roleDataService.dynamicOptionSelected = result.AvailableDataRange;
					dataFlat.forEach(function (item) {
						
						var availableData = $filter('filter')(permsData, { Id: item.Id });
						item.selected = availableData.length !== 0 ? true : false;

					});
					checkChild(roleDataService.organization.BusinessUnit[0]);
				});

			};

			var checkChild = function (node) {
				node.ChildNodes.forEach(function (subnode) {
					if (subnode.ChildNodes && subnode.ChildNodes.length > 0) {
						checkChild(subnode);
					}
					if (subnode.selected) {
						node.selected = true;
					}
				});
			};

			return roleDataService;
		}
	]);
})();