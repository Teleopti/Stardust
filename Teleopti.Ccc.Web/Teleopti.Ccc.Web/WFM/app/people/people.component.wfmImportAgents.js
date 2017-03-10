(function (angular) {
	'use strict';

	function WfmImportAgentsCtrl(svc, peopleSvc) {
		this._svc = svc;
		this._peopleSvc = peopleSvc;
		this.fallbacks = {};

		this.now = new Date();
	}

	WfmImportAgentsCtrl.prototype.fetchingFieldOptions = true;
	WfmImportAgentsCtrl.prototype.started = false;
	WfmImportAgentsCtrl.prototype.done = false;

	WfmImportAgentsCtrl.prototype.reset = function() {
		this.done = false;
		this.started = false;
	};

	WfmImportAgentsCtrl.prototype.$onInit = function () {
		this._svc.fetchOptions()
			.then(this.onFieldOptionsFetched.bind(this));
	};

	WfmImportAgentsCtrl.prototype.onFieldOptionsFetched = function (options) {
		this.fieldOptions = options;
		this.fetchingFieldOptions = false;
	};

	WfmImportAgentsCtrl.prototype.clickImport = function () {
		this.started = true;
		this._peopleSvc.uploadAgentFromFile(this.file, this.fallbacks)
			.then(this.handleImportResult.bind(this));
	};

	WfmImportAgentsCtrl.prototype.getTemplate = function() {
		this._peopleSvc.downloadFileTemplateAgent()
			.then(function(response) {
				this.saveFile(response, 'agent_template.xls');
			}.bind(this));
	};

	WfmImportAgentsCtrl.prototype.handleImportResult = function (response) {
		this.done = true;

		var isXlsx = response.headers()['content-type'] === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
		var processResult = response.headers()['message'].match(/[0-9]+/g);
		if (processResult[1] > 0) {
			var ext = isXlsx ? '.xlsx' : '.xls';
			this.saveFile(response, 'invalid_agents' + ext);
		}
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
			controller: ['importAgentsService', 'PeopleService', WfmImportAgentsCtrl]
		});
})(angular);
