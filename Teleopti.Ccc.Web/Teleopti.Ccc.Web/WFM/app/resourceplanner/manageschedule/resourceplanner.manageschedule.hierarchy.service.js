(function() {
	'use strict';

	angular.module('wfm.resourceplanner').service('HierarchyService', [
		'$q', '$resource', function ($q, $resource) {
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

			var organisationSelections = $resource('../api/ResourcePlanner/OrganizationSelection', {}, {
				query: { method: 'GET', params: {}, isArray: false }
			});

			var hierarchyService = {};
			hierarchyService.organization = { BusinessUnit: [{ BusinessUnit: { Sites: [] } }] };
			hierarchyService.displayedData = {};
			
			hierarchyService.refreshOrganizationSelection = function () {
				organisationSelections.query().$promise.then(function (result) {
					hierarchyService.organization = { BusinessUnit: [result.BusinessUnit] };
					flatData(hierarchyService.organization.BusinessUnit);
				});
			};

			return hierarchyService;
		}
	]);
})();