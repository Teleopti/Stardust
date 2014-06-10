
define([
		'knockout',
		'progressitem-count',
		'template/iteration',
		'result'
], function (
		ko,
		ProgressItemCountViewModel,
		Iteration,
		ResultViewModel
	) {
	return function () {

		var self = this;
		var result;

		this.Name = "Rta Load Test";
		this.Text = "\
		Load test RTA configuration options:<br/> \
		Remove SetUpDatabase if you dont want to create testAgents <br/><br/>\
		\
		Instead of ExternalLogOns:[ 1,2 ] you can choose either <br/>\
		Sites : [ \"guid1\",\"guid2\" ] <br/>\
		Teams : [ \"guid1\",\"guid2\" ] <br/>\
		PersonIds : [ \"guid1\",\"guid2\" ] ";

		this.Configuration = ko.observable();

		this.ConfigurationObject = ko.computed(function () {
			try {
				return JSON.parse(self.Configuration());
			} catch (e) {
				return undefined;
			}
		});

		var updateResult = function () {
			result.IterationsDone(progressItemReadModel.Count());
			if (result.IterationsDone() >= self.IterationsExpected()) {
				result.RunDone(true);
				result = null;
			}
		};

		this.Iterations = ko.computed(function () {

			var configuration = self.ConfigurationObject();
			if (!configuration)
				return;

			var iterations = [];
			for (var i = 0; i < configuration.NumberOfIterations; i++) {
				iterations.push(new Iteration({
					Number: i,
					Sent: function () {
						updateResult();
					},
					Success: function () {
						progressItemReadModel.Success();
						updateResult();
					},
					Failure: function () {
						progressItemReadModel.Failure();
						updateResult();
					}
				}));
				if (iterations.length > 2000)
					return undefined;
			}
			return iterations;

		});

		this.IterationsExpected = ko.computed(function () {
			var iterations = self.Iterations();
			if (iterations)
				return iterations.length;
			else
				return 0;
		});

		var progressItemReadModel = new ProgressItemCountViewModel(
			"Count",
			self.IterationsExpected
		);

		this.ProgressItems = [
			progressItemReadModel
		];

		this.ConfigurationError = ko.computed(function () {
			if (!self.ConfigurationObject())
				return "Could not parse configuration";
			if (!self.Iterations())
				return "Too many combinations found";
			if (self.Iterations().length == 0)
				return "No combinations found";
			return undefined;
		});

		var configuration =
		{
			Url: "http://foo.bar/TeleoptiRtaService.svc",
			SendInterval: 1000,
			PlatformTypeId: "00000000-0000-0000-0000-000000000000",
			ExternalLogOns: [],
			States: [],
			DataSourceId: 1,
			SetUpDatabase: {
				NumberOfAgentsToCreate: 1000,
				DataSourceId: 6,
			}

		};

		self.Configuration(JSON.stringify(configuration, null, 4));

		this.Run = function () {

			var iterations = self.Iterations();

			progressItemReadModel.Reset();

			$.each(iterations, function (i, e) {
				e.Start();
			});

			var commandsSentPromises = $.map(iterations, function (e) {
				return e.AllCommandsCompletedPromise;
			});
			$.when.apply($, commandsSentPromises).then(function () {
				result.CommandsDone(true);
			});

			result = new ResultViewModel();

			return result;
		};
	};
});