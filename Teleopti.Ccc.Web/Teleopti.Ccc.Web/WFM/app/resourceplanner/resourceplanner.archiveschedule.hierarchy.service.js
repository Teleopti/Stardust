(function() {
	'use strict';

	angular.module('wfm.resourceplanner').service('HierarchyService', [
		'$q', '$filter', 'PermissionsService', function ($q, $filter, PermissionsService) {
			var dataFlat = [];
			
			var flatData = function (dataTab, parentNode) {
			    dataTab.forEach(function (item) {
			        parentNode = parentNode ? parentNode : { searchableName: ''};
			        item.searchableName = parentNode.searchableName + ' ' + item.Name;
			        dataFlat.push(item);
			        if (item.ChildNodes && item.ChildNodes.length !== 0) {
				        flatData(item.ChildNodes, item);
					} else {
						item.ChildNodes = [];
						item.selected = false;
					}
				});
			};
			var hierarchyService = {};
			hierarchyService.organization = { BusinessUnit: [{ BusinessUnit: { Sites: [] } }], DynamicOptions: [] };
			hierarchyService.displayedData = {};
			
			hierarchyService.refreshOrganizationSelection = function () {
				PermissionsService.organizationSelections.query().$promise.then(function (result) {
					hierarchyService.organization = { BusinessUnit: [result.BusinessUnit], DynamicOptions: result.DynamicOptions };
					flatData(hierarchyService.organization.BusinessUnit);
				});
			};

			return hierarchyService;
		}
	]);
})();