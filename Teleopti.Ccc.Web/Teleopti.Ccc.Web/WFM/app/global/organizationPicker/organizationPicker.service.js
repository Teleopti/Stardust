(function() {
	'use strict';

	angular.module('wfm.organizationPicker').service('organizationPickerSvc', OrganizationPickerService);

	OrganizationPickerService.$inject = ['$http'];

	function OrganizationPickerService($http) {
		var svc = this;
		var moduleMap = {
			'wfm.teamSchedule': '../api/TeamScheduleData/FetchPermittedTeamHierachy',
			'wfm.requests': '../api/Requests/FetchPermittedTeamHierachy'
		}
		var currentModule;

		svc.setModule = function (moduleStr) {
			currentModule = moduleStr;
		}

		svc.getAvailableHierarchy = function (dateStr) {
			var input = (moduleMap[currentModule] || moduleMap['wfm.teamSchedule']) + "?date=" + dateStr;
			return $http.get(input);
		};
	}
})();