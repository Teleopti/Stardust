(function (angular) {
	'use strict';

	var blankId = '00000000-0000-0000-0000-000000000000';
	var defaultFallbacks = {
		externalLogon: blankId
	};
	function WfmImportAgentsCtrl($translate, svc, peopleSvc) {
		this._svc = svc;
		this._peopleSvc = peopleSvc;
		this._translate = $translate;
		this.fallbacks = angular.copy(defaultFallbacks);
		this.now = new Date();
	}

	WfmImportAgentsCtrl.prototype.blankId = blankId;
	WfmImportAgentsCtrl.prototype.fetchingFieldOptions = true;
	WfmImportAgentsCtrl.prototype.started = false;
	WfmImportAgentsCtrl.prototype.done = false;



	WfmImportAgentsCtrl.prototype.reset = function () {
		this.done = false;
		this.started = false;
		this.fallbacks = angular.copy(defaultFallbacks);
		this.result = null;
		this.file = null;
		this.setFallbacks = false;
	};

	WfmImportAgentsCtrl.prototype.$onInit = function () {
		this._svc.fetchOptions()
			.then(this.onFieldOptionsFetched.bind(this));
	};

	WfmImportAgentsCtrl.prototype.onFieldOptionsFetched = function (options) {
		this.fieldOptions = options;
		this.fetchingFieldOptions = false;
	};

	WfmImportAgentsCtrl.prototype.setTeam = function (teamId) {
		this.fallbacks.team = teamId;
	};

	WfmImportAgentsCtrl.prototype.noFile = function () {
		return !(!!this.file);
	};

	WfmImportAgentsCtrl.prototype.clickImport = function () {
		this.started = true;
		this._peopleSvc.uploadAgentFromFile(this.file, this.fallbacks)
			.then(this.handleImportResult.bind(this));
	};

	WfmImportAgentsCtrl.prototype.getTemplate = function () {
		this._peopleSvc.downloadFileTemplateAgent()
			.then(function (response) {
				this.saveFile(response, 'agent_template.xls');
			}.bind(this));
	};

	WfmImportAgentsCtrl.prototype.handleImportResult = function (response) {
		this.done = true;

		var isXlsx = response.headers()['content-type'] === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';

		var processResult = response.headers()['message'].match(/success count:(\d+), failed count:(\d+), warning count:(\d+)/);

		this.result = {
			success: processResult[1],
			failure: processResult[2],
			warning: processResult[3]
		};

		if (processResult[2] > 0 || processResult[3] > 0) {
			var ext = isXlsx ? '.xlsx' : '.xls';
			this.saveFile(response, 'invalid_agents' + ext);
		}
	};

	WfmImportAgentsCtrl.prototype.getMessage = function (type, count) {
		if (type === 'success') {
			return this._translate.instant('SuccessfullyImportedAgents').replace('{0}', count);
		} else if (type === 'failure') {
			return this._translate.instant('FailedToImportAgents').replace('{0}', count);
		} else if (type === 'warning') {
			return this._translate.instant('WarnImportedAgents').replace('{0}', count);
		}
		return '';
	};

	WfmImportAgentsCtrl.prototype.saveFile = function (response, filename) {
		var blob = new Blob([response.data], {
			type: response.headers()['content-type']
		});
		saveAs(blob, filename);
	};

	angular.module('wfm.people')
		.component('wfmImportAgents',
		{
			templateUrl: 'app/people/html/wfm-import-agents.tpl.html',
			controller: ['$translate', 'importAgentsService', 'PeopleService', WfmImportAgentsCtrl]
		});
})(angular);
