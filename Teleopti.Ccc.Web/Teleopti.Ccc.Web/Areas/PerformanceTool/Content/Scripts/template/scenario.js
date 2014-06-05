
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

		this.Name = "Input -> Measurement (Template)";
		this.Text = "Use this scenario code as a template for new ones";

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

		self.Configuration(JSON.stringify({
			NumberOfIterations: 100
		}, null, 4));

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