(function (angular) {
	'use strict';

	function Ctrl(PeopleService, $translate) {
		this._peopleService = PeopleService;
		this._translate = $translate;

		this.fallbacks = {};
		Object.defineProperty(this.fallbacks, 'team', { value: [] });

		this.now = new Date();
	}

	Ctrl.prototype.started = false;
	Ctrl.prototype.fetchingFieldOptions = true;
	Ctrl.prototype.setFallbacks = false;
	Ctrl.prototype.fileSizeLimit = 2097152;

	Ctrl.prototype.$onInit = function () {
		this.parent.optionsPromise
			.then(this.onFieldOptionsFetched.bind(this));
	};

	Ctrl.prototype.onFieldOptionsFetched = function (options) {
		this.fieldOptions = options;
		this.fetchingFieldOptions = false;
	};

	Ctrl.prototype.gotoJobList = function () {
		this.parent.page = '';
	};

	Ctrl.prototype.invalidFile = function () {
		return !(!!this.file) || this.file.size > this.fileSizeLimit;
	};

	Ctrl.prototype.createImportJob = function () {
		this.started = true;
		var fields = this.setFallbacks ? this.fallbacks : undefined;

		this._peopleService.newImportJob(this.file, fields)
			.then(function (response) {
				this.parent.message = this._translate.instant('ANewImportJobHasBeenCreatedFor').replace('{0}', this.file.name);
				this.gotoJobList();
			}.bind(this), function (response) {
				this.started = false;
			}.bind(this));
	};

	Ctrl.prototype.getTemplate = function () {
		this._peopleService.downloadFileTemplateAgent()
			.then(function (response) {
				this.parent.saveFile(response, 'agent_template.xls');
			}.bind(this));
	};

	var component = {
		require: {
			parent: '^wfmImportAgents'
		},
		templateUrl: 'app/people/html/new-import-job.tpl.html',
		controller: ['PeopleService', '$translate', Ctrl]
	};

	angular.module('wfm.people')
		.component('newImportJob', component);
})(angular);