(function (angular) {
	'use strict';

	function Artifact(id, filename) {
		this.id = id;
		this.filename = filename;
	}

	function Job(id, filename, startedTime, inProgress, succeeded, failed, warned) {
		this.id = id;
		this.filename = filename;
		this.startedTime = startedTime;
		this.inProgress = inProgress;
		this.succeeded = succeeded;
		this.failed = failed;
		this.warned = warned;
		this.artifacts = {
			input: null,
			errors: null,
			warnings: null
		};
	}

	Job.prototype.summary = function () {
		if (this.failed === 0) {
			return ('{0} imported. All succeeded. ').replace('{0}', this.succeeded);
		}
		return ('{0} imported, {1} failed. ').replace('{0}', this.succeeded).replace('{1}', this.failed);
	};

	function Ctrl(NoticeService, importAgentsService) {
		this._notice = NoticeService;
		this._svc = importAgentsService;

		this.jobs = [];
	}

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
			var job = new Job(j.JobResultId, filename, j.Timestamp, j.IsWorking, j.SuccessCount + j.WarningCount, j.FaildCount, j.WarningCount);

			if (j.InputArtifact) {
				job.artifacts.input = new Artifact(j.InputArtifact.Id, j.InputArtifact.Name);
			}

			if (job.failed > 0) {
				job.artifacts.errors = new Artifact(j.FaildArtifact.Id, j.FaildArtifact.Name);
			}

			if (job.warned > 0) {
				job.artifacts.warnings = new Artifact(j.WarningArtifact.Id, j.WarningArtifact.Name);
			}

			return job;
		});
	};

	Ctrl.prototype.refreshList = function () {
		this._svc.fetchJobs()
			.then(this.onJobsFetched.bind(this));
	};

	Ctrl.prototype.newImport = function () {
		this.parent.page = 'new';
	};

	var component = {
		require: {
			parent: '^wfmImportAgents'
		},
		templateUrl: 'app/people/html/import-job-list.tpl.html',
		controller: ['NoticeService', 'importAgentsService', Ctrl]
	};

	angular.module('wfm.people')
		.component('importJobList', component);
})(angular);