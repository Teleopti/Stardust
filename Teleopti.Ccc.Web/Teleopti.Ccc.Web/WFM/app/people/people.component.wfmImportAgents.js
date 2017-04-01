(function (angular) {
	'use strict';

	function WfmImportAgentsCtrl($translate, importAgentsService, PeopleService, Toggle) {
		this._svc = importAgentsService;
		this._peopleSvc = PeopleService;
		this._translate = $translate;
		this._toggles = Toggle;
		this.runImportJobInBackground = this._toggles.Wfm_People_MoveImportJobToBackgroundService_43582;
		this.fallbacks = {};
		this.now = new Date();
	}

	WfmImportAgentsCtrl.prototype.fetchingFieldOptions = true;
	WfmImportAgentsCtrl.prototype.started = false;
	WfmImportAgentsCtrl.prototype.done = false;
	WfmImportAgentsCtrl.prototype.setFallbacks = false;


	WfmImportAgentsCtrl.prototype.reset = function () {
		this.done = false;
		this.started = false;
		this.fallbacks = {};
		this.result = null;
		this.fileError = null;
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
		var fields = this.setFallbacks ? this.fallbacks : undefined;

		if (!this.runImportJobInBackground) {
			this._peopleSvc.uploadAgentFromFile(this.file, fields)
				.then(this.handleImportResult.bind(this), this.handleImportError.bind(this));
		} else {}
	};

	WfmImportAgentsCtrl.prototype.getTemplate = function () {
		this._peopleSvc.downloadFileTemplateAgent()
			.then(function (response) {
				this.saveFile(response, 'agent_template.xls');
			}.bind(this));
	};

	WfmImportAgentsCtrl.prototype.handleImportError = function(response) {
		this.done = true;
		var fileError = response.headers()['message'].match(/^format errors: (.+)$/);

		if (fileError) {
			this.fileError = fileError[1];
		}
	}

	WfmImportAgentsCtrl.prototype.handleImportResult = function (response) {
		this.done = true;
		
		var isXlsx = response.headers()['content-type'] === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';

		var processResult = response.headers()['message'].match(/success count:(\d+), failed count:(\d+), warning count:(\d+)/);

		if (processResult) {
			var isAllSuccess = !(processResult[2] > 0 || processResult[3] > 0);
			this.result = {
				success: processResult[1],
				failure: processResult[2],
				warning: processResult[3],
				isAllSuccess: isAllSuccess
			};

			if (!isAllSuccess) {
				var ext = isXlsx ? '.xlsx' : '.xls';
				this.saveFile(response, 'invalid_agents' + ext);
			}
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
			controller: ['$translate', 'importAgentsService', 'PeopleService', 'Toggle', WfmImportAgentsCtrl]
		});
})(angular);
