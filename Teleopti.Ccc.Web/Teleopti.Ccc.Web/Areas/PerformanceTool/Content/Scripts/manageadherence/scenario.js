
define([
		'knockout',
		'manageadherence/iteration',
		'manageadherence/result',
		'messagebroker'
], function (
		ko,
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

		this.Iterations = ko.computed(function() {
			var configuration = self.ConfigurationObject();
			if (!configuration)
				return undefined;

			var iterations = [];
			for (var s = 0; s < configuration.States.length; s++) {
				for (var p = 0; p < configuration.Persons.length; p++) {

					iterations.push(new Iteration({
						PlatformTypeId: configuration.PlatformTypeId,
						SourceId: configuration.SourceId,
						Person: configuration.Persons[p],
						StateCode: configuration.States[s],
						IsEndingIteration: s === configuration.States.length - 1,
						Timestamp: configuration.Timestamp
					}));

					if (iterations.length > 2000)
						return undefined;
				}
			}

			return iterations;

		});

		self.IterationsExpected = ko.computed(function () {
			var iterations = self.Iterations();
			if (iterations)
				return iterations.length;
			else
				return 0;
		});


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
					PersonId: "B46A2588-8861-42E3-AB03-9B5E015B257C",
				}
			],
			States: ["Ready", "OFF"],
			TeamId: "34590A63-6331-4921-BC9F-9B5E015AB495",
			PollingPerSecond: 1
		}, null, 4));

		this.Run = function () {
			result = new ResultViewModel();

			$.ajax({
				url: "performancetool/application/adherencetest?limit=" + self.IterationsExpected(),
				success: function () {
					self.runIterations();
				}
			});
			$.ajax({
				url: "Anywhere/BusinessUnit/Current",
				success: function (bu) {
					setInterval(self.Poll(bu.Id), 1000);
				}
			});
			return result;
		};

		self.Poll = function(buId) {
			for (var i = 0; i < self.PollingPerSecond; i++) {
				$.ajax({
					headers: { 'X-Business-Unit-Filter': buId },
					url: "Anywhere/Agents/GetStates?teamId=" + self.TeamId,
				});
			}
		}

		self.runIterations = function() {
			var iterations = self.Iterations();
			startPromise.done(function() {
				manageAdherenceSubscription = messagebroker.subscribe({
					domainType: 'PerformanceCountDone',
					callback: function(notification) {
						result.RunDone(true);
						result.IterationsDone(self.IterationsExpected());
						var data = JSON.parse(notification.BinaryData);
						result.EndTime(moment(data.EndTime));
					}
				});

				$.when(manageAdherenceSubscription.promise).done(function() {
					$.each(iterations, function(i, e) {
						e.Start();
					});

					var commandsSentPromises = $.map(iterations, function(e) {
						return e.AllCommandsCompletedPromise;
					});
					$.when.apply($, commandsSentPromises).then(function() {
						result.CommandsDone(true);
					});
				});
			});
		};

	};
});