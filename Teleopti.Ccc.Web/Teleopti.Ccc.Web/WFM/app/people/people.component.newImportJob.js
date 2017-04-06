(function (angular) {
	'use strict';

	function Ctrl(PeopleService) {
		this._peopleService = PeopleService;

		this.fallbacks = {};
		this.now = new Date();
	}

	Ctrl.prototype.started = false;
	Ctrl.prototype.fetchingFieldOptions = true;
	Ctrl.prototype.setFallbacks = false;

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

	Ctrl.prototype.setTeam = function (teamId) {
		this.fallbacks.team = teamId;
	};

	Ctrl.prototype.noFile = function () {
		return !(!!this.file);
	};

	Ctrl.prototype.createImportJob = function () {
		this.started = true;
		var fields = this.setFallbacks ? this.fallbacks : undefined;

		this._peopleService.newImportJob(this.file, fields)
			.then(function (response) {
				this.parent.message = ('A new import job has been created for \'{0}\'').replace('{0}', this.file.name);
				this.gotoJobList();
			}.bind(this), function (response) {
				this.started = false;
			}.bind(this));
	};

	Ctrl.prototype.getTemplate = function () {
		this._peopleService.downloadFileTemplateAgent();
	};

	var component = {
		require: {
			parent: '^wfmImportAgents'
		},
		templateUrl: 'app/people/html/new-import-job.tpl.html',
		controller: ['PeopleService', Ctrl]
	};

	angular.module('wfm.people')
		.component('newImportJob', component);
})(angular);