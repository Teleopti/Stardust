(function (angular) {
	'use strict';

	function ImportAgentsService($http) {
		this._http = $http;
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

	angular.module('wfm.people')
		.service('importAgentsService', ['$http', ImportAgentsService]);
})(angular);
