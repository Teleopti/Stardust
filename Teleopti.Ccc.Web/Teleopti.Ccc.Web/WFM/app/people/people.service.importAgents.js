(function (angular) {
	'use strict';

	function ImportAgentsService($q, $http) {
		this._q = $q;
		this._http = $http;
		this.hierarchyUrl = '../api/TeamScheduleData/FetchPermittedTeamHierachy';
	}

	ImportAgentsService.prototype.optionsUrl = '../api/People/GetImportAgentSettingsData';
	ImportAgentsService.prototype.jobsUrl = '../api/People/AgentJobList';



	ImportAgentsService.prototype.fetchOptions = function () {
		return this._http.get(this.optionsUrl)
			.then(function (response) {
				return response.data;
			});
	};

	ImportAgentsService.prototype.fetchJobs = function () {
		return this._http.get(this.jobsUrl)
			.then(function (response) {
				return response.data;
			});
	};

	ImportAgentsService.prototype.fetchHierarchy = function (dateStr) {
		if (!dateStr) {
			return;
		}
		return this._http.get(this.hierarchyUrl, { params: { date: dateStr } }).then(function (response) {
			return response.data;
		});
	};

	angular.module('wfm.people')
		.service('importAgentsService', ['$q', '$http', ImportAgentsService]);
})(angular);
