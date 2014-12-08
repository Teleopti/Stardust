
define([
		'knockout',
		'progressitem-count',
		'manageadherence/iteration',
		'result',
		'messagebroker'
], function (
		ko,
		ProgressItemCountViewModel,
		Iteration,
		ResultViewModel,
		messagebroker
	) {
	return function () {

		var self = this;
		var startPromise = messagebroker.start();
		var manageAdherenceSubscription;
		var result;

		this.Name = "Manage Adherence Load Test";

		var text =
		"\
		Before running this scenario make sure that:<br /> \
		- All external logons exists in the system as persons.<br /> \
		- No agent is currently in expected state group.<br /> \
		- Too few agents or states. <br/> \
		Default is only to show you how to set it up, not an actual test<br />\
		";
		this.Text = text;

		this.Configuration = ko.observable();

		this.ConfigurationObject = ko.computed(function () {
			try {
				return JSON.parse(self.Configuration());
			} catch (e) {
				return undefined;
			}
		});

		var expectedReadModelUpdates = ko.observable(0);
		var forceReevaluateOfIterations = ko.observable();
		this.Iterations = ko.computed(function() {
			var configuration = self.ConfigurationObject();
			if (!configuration)
				return undefined;

			var iterations = [];
			forceReevaluateOfIterations();
			for (var s = 0; s < configuration.States.length; s++) {
				for (var p = 0; p < configuration.Persons.length; p++) {

					iterations.push(new Iteration({
						PlatformTypeId: configuration.PlatformTypeId,
						SourceId: configuration.SourceId,
						Person: configuration.Persons[p],
						StateCode: configuration.States[s],
						IsEndingIteration: s === configuration.States.length - 1,
						Timestamp: configuration.Timestamp,
						ExpectedUpdates: configuration.ExpectedUpdates,
						ReadModelUpdatedDone: function () {
							progressItemReadModel.Success();
							calculateRunDone();
						},
					}));

					if (iterations.length > 2000)
						return undefined;
				}
			}

			expectedReadModelUpdates(iterations.length * configuration.ExpectedUpdates);

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
			"Successful Read Model Updates",
			expectedReadModelUpdates
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
			PlatformTypeId: "00000000-0000-0000-0000-000000000000",
			SourceId: 1,
			Persons: [
				{
					ExternalLogOn: "2001",
					PersonId: "B46A2588-8861-42E3-AB03-9B5E015B257C"
				}
			],
			States: ["Ready", "OFF"],
			ExpectedUpdates: 1,
		}, null, 4));


		var calculateRunDone = function () {
			var calculatedInterationsDone = progressItemReadModel.Count();
			if (calculatedInterationsDone > result.IterationsDone()) {
				result.IterationsDone(calculatedInterationsDone);
				if (result.IterationsDone() >= self.IterationsExpected()) {
					result.RunDone(true);
					result = null;
					messagebroker.unsubscribe(manageAdherenceSubscription);
					manageAdherenceSubscription = null;
				}
			}
		};

		this.Run = function () {

			var iterations = self.Iterations();

			progressItemReadModel.Reset();
			result = new ResultViewModel();

			startPromise.done(function () {

				manageAdherenceSubscription = messagebroker.subscribe({
					domainType: 'ReadModelUpdatedMessage',
					callback: function () {
						for (var i = 0; i < iterations.length; i++) {
							var iteration = iterations[i];
							if (!iteration.Done()) {
								iteration.ReadModelUpdatedCount++;
								iteration.NotifyReadModelUpdatedDone();
								break;
							}
						}
					}
				});

				$.when(
					manageAdherenceSubscription.promise
				).done(function () {

					$.each(iterations, function (i, e) {
						e.Start();
					});

					var commandsSentPromises = $.map(iterations, function (e) {
						return e.AllCommandsCompletedPromise;
					});
					$.when.apply($, commandsSentPromises).then(function () {
						result.CommandsDone(true);
					});

				});

			});

			return result;
		};


	};
});