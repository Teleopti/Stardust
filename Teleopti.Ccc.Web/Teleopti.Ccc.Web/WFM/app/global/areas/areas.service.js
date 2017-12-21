(function() {
	'use strict';

	angular
		.module('wfm.areas')
		.factory('areasService', areasService);

	areasService.$inject = ['$resource'];

	function areasService($resource) {
		var areasWithPermission, areasList;
		var service = {
			getAreasWithPermission: getAreasWithPermission,
			getAreasList: getAreasList
		};

		return service;

		function getAreasWithPermission() {
			if (!areasWithPermission) {
				areasWithPermission = getAreasWithPermissionFromServer();
			}
			return areasWithPermission;
		}

		function getAreasList() {
			if (!areasList) {
				areasList = getAreasListFromServer();
			}
			return areasList;
		}

		function getAreasWithPermissionFromServer() {
			return $resource('../api/Global/Application/WfmAreasWithPermission', {}, {
				query: {
					method: 'GET',
					params: {},
					isArray: true
				}
			}).query().$promise;
		}

		function getAreasListFromServer () {
			return $resource('../api/Global/Application/WfmAreasList', {}, {
				query: {
					method: 'GET',
					params: {},
					isArray: true
				}
			}).query().$promise;
		}
	}
})();