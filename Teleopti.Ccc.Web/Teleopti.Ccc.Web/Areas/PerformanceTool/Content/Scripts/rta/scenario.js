
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

		this.Iterations = ko.computed(function () {

			var configuration = self.ConfigurationObject();
			if (!configuration)
				return;

			var iterations = [];
			
			for (var numberOfStates = 0; numberOfStates < configuration.StatesToSend;) {
				for (var stateCode = 0; stateCode < configuration.States.length; stateCode++) {
					for (var externalLogOn = 0; externalLogOn < configuration.ExternalLogOns.length; externalLogOn++) {
						iterations.push(new Iteration({
							Url: configuration.Url,
							PlatformTypeId: configuration.PlatformTypeId,
							SourceId: configuration.SourceId,

							ExternalLogOn: configuration.ExternalLogOns[externalLogOn],
							StateCode: configuration.States[stateCode],
							Success: function() {
								progressItemReadModel.Success();
								calculateRunDone();
							},
							Failure: function() {
								progressItemReadModel.Failure();
								calculateRunDone();
							}
						}));
						numberOfStates++;
						if (numberOfStates == configuration.StatesToSend)
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
			PlatformTypeId: "00000000-0000-0000-0000-000000000000",
			ExternalLogOns: ["2001", "2002", "0063", "2000", "0019", "0068", "0085", "0202", "0238", "2003"],
			States: ["Ready", "OFF"],
			SourceId: 1,
			StatesToSend: 4,
			ExpectedPersonsInAlarm: 4
		}, null, 4));


		var calculateRunDone = function () {
			var calculatedInterationsDone = progressItemReadModel.Count();
			if (calculatedInterationsDone > result.IterationsDone()) {
				result.IterationsDone(calculatedInterationsDone);
				if (result.IterationsDone() >= self.IterationsExpected()) {
					result.RunDone(true);
					result = null;
					messagebroker.unsubscribe(agentsAdherenceSubscription);
					agentsAdherenceSubscription = null;
				}
			}
		};

		this.Run = function () {
			var iterations = self.Iterations();
			progressItemReadModel.Reset();
			result = new ResultViewModel();

			var expectedPersonsInAlarm = self.ConfigurationObject().ExpectedPersonsInAlarm;
			startPromise.done(function() {
				agentsAdherenceSubscription = messagebroker.subscribe({
					domainType: 'SiteAdherenceMessage',
					callback: function (notification) {
						var outOfAdherence = JSON.parse(notification.BinaryData).OutOfAdherence;
						console.log(outOfAdherence);
						console.log(expectedPersonsInAlarm);
						if (outOfAdherence === expectedPersonsInAlarm)
							result.RunDone(true);
					}
				});


				$.when(
					agentsAdherenceSubscription.promise
				).done(function() {

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