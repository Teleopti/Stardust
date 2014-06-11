
define([
		'knockout',
		'progressitem-count',
		'rta/iteration',
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
		this.Text = "";

		this.Configuration = ko.observable();

		this.ConfigurationObject = ko.computed(function () {
			try {
				return JSON.parse(self.Configuration());
			} catch (e) {
				return undefined;
			}
		});

		var updateResult = function () {
			var calculatedInterationsDone = progressItemReadModel.Count();
			if (calculatedInterationsDone > result.IterationsDone()) {
				result.IterationsDone(calculatedInterationsDone);
				if (result.IterationsDone() >= self.IterationsExpected()) {
					result.RunDone(true);
					result = null;
				}
			}
		};

		this.Iterations = ko.computed(function () {

			var configuration = self.ConfigurationObject();
			if (!configuration)
				return;

			var iterations = [];
			
			for (var i = 0; i < configuration.StatesToSend;) {
				for (var k = 0; k < configuration.States.length; k++) {
					for (var j = 0; j < configuration.ExternalLogOns.length; j++) {
						iterations.push(new Iteration({
							Url: configuration.Url,
							PlatformTypeId: configuration.PlatformTypeId,
							SourceId: configuration.SourceId,

							ExternalLogOn: configuration.ExternalLogOns[j],
							StateCode: configuration.States[k],

							Sent: function() {
								updateResult();
							},
							Success: function() {
								progressItemReadModel.Success();
								updateResult();
							},
							Failure: function() {
								progressItemReadModel.Failure();
								updateResult();
							}
						}));
						i++;
						if (i == configuration.StatesToSend)
							return iterations;
					}
				}
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
			"Rta",
			ko.computed(function() {
				return self.IterationsExpected();
			}) 
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
			Url: "http://localhost:52858/TeleoptiRtaService.svc",
			PlatformTypeId: "00000000-0000-0000-0000-000000000000",
			ExternalLogOns: [2001],
			States: ["Ready", "OFF"],
			SourceId: 1,
			StatesToSend: 100
		}, null, 4));

		this.Run = function () {
			var iterations = self.Iterations();
			progressItemReadModel.Reset();
			result = new ResultViewModel();

			$.each(iterations, function (i, e) {
				e.Start();
			});

			result.CommandsDone(true);

			return result;
		};
	};
});