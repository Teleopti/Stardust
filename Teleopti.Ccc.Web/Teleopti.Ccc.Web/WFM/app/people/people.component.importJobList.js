(function (angular) {
	'use strict';

	function Artifact(id, filename) {
		this.id = id;
		this.filename = filename;
	}

	function Job(id, owner, filename, startedTime, inProgress, error, succeeded, failed, warned) {
		this.id = id;
		this.owner = owner;
		this.filename = filename;
		this.startedTime = startedTime ? new Date(startedTime) : null;
		this.inProgress = inProgress;
		this.error = error;
		this.succeeded = succeeded;
		this.failed = failed;
		this.warned = warned;
		this.artifacts = {
			input: null,
			errors: null,
			warnings: null
		};
	}

	function Ctrl(NoticeService, importAgentsService, $translate) {
		this._notice = NoticeService;
		this._svc = importAgentsService;
		this._translate = $translate;

		this.jobs = [];
	}

	Ctrl.prototype.refreshing = false;

	Ctrl.prototype.$onInit = function () {
		if (angular.isString(this.parent.message) && this.parent.message.length > 0) {
			this._notice.success(this.parent.message, 5000, true);
			this.parent.message = '';
		}

		this._svc.fetchJobs()
			.then(this.onJobsFetched.bind(this));
	};

	Ctrl.prototype.onJobsFetched = function (data) {
		this.jobs = data.map(function (j) {
			var filename = j.InputArtifact ? j.InputArtifact.Name : 'No filename';
			var job = new Job(j.JobResultId, j.Owner, filename, j.Timestamp, j.IsWorking, j.ErrorMessage, j.SuccessCount + j.WarningCount, j.FailedCount, j.WarningCount);

			if (j.InputArtifact) {
				job.artifacts.input = new Artifact(j.InputArtifact.Id, j.InputArtifact.Name);
			}

			if (job.failed > 0 && j.FailedArtifact) {
				job.artifacts.errors = new Artifact(j.FailedArtifact.Id, j.FailedArtifact.Name);
			}

			if (job.warned > 0 && j.WarningArtifact) {
				job.artifacts.warnings = new Artifact(j.WarningArtifact.Id, j.WarningArtifact.Name);
			}

			return job;
		});
	};

	Ctrl.prototype.refreshList = function () {
		this.refreshing = true;
		this._svc.fetchJobs()
			.then(function (data) {
				this.refreshing = false;
				this.onJobsFetched(data);
			}.bind(this), function () {
				this.refreshing = false;
			}.bind(this));
	};

	Ctrl.prototype.makeSummary = function (job) {
		if (job.failed === 0) {
			return this._translate.instant('ImportAgentsJobResultSummaryForFullSuccess').replace('{0}', job.succeeded);
		}
		return this._translate.instant('ImportAgentsJobResultSummaryForNonfullSuccess').replace('{0}', job.succeeded).replace('{1}', job.failed);
	};

	Ctrl.prototype.newImport = function () {
		this.parent.page = 'new';
	};

	var component = {
		require: {
			parent: '^wfmImportAgents'
		},
		templateUrl: 'app/people/html/import-job-list.tpl.html',
		controller: ['NoticeService', 'importAgentsService', '$translate', Ctrl]
	};

	angular.module('wfm.people')
		.component('importJobList', component);
})(angular);