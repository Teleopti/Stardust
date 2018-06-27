(function (angular) {
	'use strict';

	function WfmImportAgentsCtrl($translate, $q, importAgentsService, PeopleService, Toggle) {
		this._q = $q;
		this._svc = importAgentsService;
		this._peopleSvc = PeopleService;
		this._translate = $translate;
		this._toggles = Toggle;
		this.runImportJobInBackground = this._toggles.Wfm_People_MoveImportJobToBackgroundService_43582;

		this.fallbacks = {};
		Object.defineProperty(this.fallbacks, 'team', { value: [] });

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
		this.optionsPromise = this._svc.fetchOptions();
		this.optionsPromise.then(this.onFieldOptionsFetched.bind(this));
		this.getSitesAndTeams();
	};

	WfmImportAgentsCtrl.prototype.onFieldOptionsFetched = function (options) {
		this.fieldOptions = options;
		this.fetchingFieldOptions = false;
	};

	WfmImportAgentsCtrl.prototype.noFile = function () {
		return !(!!this.file);
	};

	WfmImportAgentsCtrl.prototype.clickImport = function () {
		this.started = true;
		var fields = this.setFallbacks ? this.fallbacks : undefined;

		this._peopleSvc.uploadAgentFromFile(this.file, fields)
			.then(this.handleImportResult.bind(this), this.handleImportError.bind(this));
	};

	WfmImportAgentsCtrl.prototype.getTemplate = function () {
		this._peopleSvc.downloadFileTemplateAgent()
			.then(function (response) {
				this.saveFile(response, 'agent_template.xls');
			}.bind(this));
	};

	WfmImportAgentsCtrl.prototype.handleImportError = function (response) {
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
	WfmImportAgentsCtrl.prototype.sitesAndTeams = [];

	WfmImportAgentsCtrl.prototype.getSitesAndTeams = function () {
		var self = this;
		return (self._sitesAndTeamsPromise ||
			(self._sitesAndTeamsPromise = self._q(function (resolve, reject) {
				self._svc.fetchHierarchy(moment().format('YYYY-MM-DD')).then(function (data) {
					resolve(data);
					self.sitesAndTeams = data.Children;
				});
			})));
	};

	angular.module('wfm.peopleold')
		.component('wfmImportAgents',
		{
			templateUrl: 'app/peopleold/html/wfm-import-agents.tpl.html',
			controller: ['$translate', '$q', 'importAgentsService', 'PeopleService', 'Toggle',  WfmImportAgentsCtrl]
		});
})(angular);
