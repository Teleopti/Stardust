
define([
		'knockout',
		'progressitem-count',
		'rta/iteration',
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
		var agentsAdherenceSubscription;
		var result;

		this.Name = "Real Time Adherence Load Test";

		var text =
		"\
		Before running this scenario make sure that:<br /> \
		- All external logons exists in the system as persons.<br /> \
		- No agent is currently in expected state group.<br /> \
		- Too few agents or states.<br /> \
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

		var personsInConfiguration = ko.observable(0);
		var forceReevaluateOfIterations = ko.observable();
		this.Iterations = ko.computed(function() {
			var configuration = self.ConfigurationObject();
			if (!configuration)
				return;

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
						ExpectedEndingStateGroup: configuration.ExpectedEndingStateGroup,
						Success: function () {
							progressItemReadModel.Success();
							calculateRunDone();
						}
					}));

					if (iterations.length > 2000)
						return undefined;
				}
			}

			personsInConfiguration(configuration.Persons.length);

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
			"Persons in expected ending state group",
			personsInConfiguration 
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
			ExpectedEndingStateGroup: "Logged off",
		}, null, 4));


		var calculateRunDone = function () {
			var calculatedInterationsDone = progressItemReadModel.Count();
			if (calculatedInterationsDone > result.IterationsDone()) {
				result.IterationsDone(calculatedInterationsDone);
				if (result.IterationsDone() >= personsInConfiguration()) {
					result.RunDone(true);
					result = null;
					messagebroker.unsubscribe(agentsAdherenceSubscription);
					agentsAdherenceSubscription = null;
				}
			}
		};

		this.Run = function () {
			forceReevaluateOfIterations.notifySubscribers();
			var iterations = self.Iterations();
			progressItemReadModel.Reset();
			result = new ResultViewModel();
			startPromise.done(function() {
				agentsAdherenceSubscription = messagebroker.subscribe({
					domainType: 'AgentsAdherenceMessage',
					callback: function (notification) {
						var message = JSON.parse(notification.BinaryData);
						var actualAgentStates = message.AgentStates;
						
						$.each(iterations, function (i, iteration) {
							$.each(actualAgentStates, function (ii, state) {
								iteration.IncomingActualAgentState(state);
							});
						});
					}
				});

				$.when(
					agentsAdherenceSubscription.promise
				).done(function () {
					$.each(iterations, function(i, e) {
						e.Start();
					});
					var statesSentPromises = $.map(iterations, function(e) {
						return e.StateSentCompletedPromise;
					});
					$.when.apply($, statesSentPromises).then(function() {
						result.CommandsDone(true);
					});
				});
			});

			return result;
		};


	};
});