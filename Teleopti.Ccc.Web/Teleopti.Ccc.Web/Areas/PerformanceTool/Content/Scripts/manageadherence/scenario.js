
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
		var intervalFunc;

		this.Name = "Manage Adherence Load Test";
		this.GenerateForm =
			"<form action='" + document.URL + "/../api/PerformanceTool/ManageAdherenceLoadTest" + "' method='get' target='_blank' class='navbar-form navbar-left'>\
					<div class='form-group'>\
						<input type='number' class='form-control' name='iterationCount' value='1000'>\
					</div>\
					<button class='btn btn-primary'>Generate</button>\
				</form>";
		this.ClearForm =
			"<form action='" + document.URL + "/../api/PerformanceTool/ClearManageAdherenceLoadTest" + "' method='get' target='_blank' class='navbar-form navbar-left'>\
					<div class='form-group'>\
						<input type='number' class='form-control' name='iterationCount' value='1000'>\
					</div>\
					<button class='btn btn-primary'>clear</button>\
				</form>";
		var text =
			"\
		Before running this scenario make sure that:<br /> \
		- All external logons exists in the system as persons.<br /> \
		- No agent is currently in expected state group.<br /> \
		- Too few agents or states. <br/> \
		Default is only to show you how to set it up, not an actual test<br /><br />\
		You can generate/clear a number of persons and states as below \
		" + self.GenerateForm + self.ClearForm +
				"";
		this.Text = text;

		this.Configuration = ko.observable();
		this.PollingPerSecond = ko.observable();
		this.TeamId = ko.observable();
		this.ClearAgentState = ko.observable(false);

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
			self.PollingPerSecond(configuration.PollingPerSecond);
			self.TeamId(configuration.TeamId);
			self.ClearAgentState(configuration.ClearAgentState);
			var iterations = [];
			for (var p = 0; p < configuration.Persons.length; p++) {

				iterations.push(new Iteration({
					PlatformTypeId: configuration.PlatformTypeId,
					SourceId: configuration.SourceId,
					Person: configuration.Persons[p],
					StateCode: configuration.States[p],
				}));
			}
			return iterations;

		});
		
		this.ClearIterations = ko.computed(function() {
			var configuration = self.ConfigurationObject();
			if (!configuration)
				return undefined;
			var iterations = [];
			for (var p = 0; p < configuration.Persons.length; p++) {

				iterations.push(new Iteration({
					PlatformTypeId: configuration.PlatformTypeId,
					SourceId: configuration.SourceId,
					Person: configuration.Persons[p],
					StateCode: configuration.ClearAgentStateCode,
				}));
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
					ExternalLogOn: "2001"
				}
			],
			States: ["Ready", "OFF"],
			TeamId: "34590A63-6331-4921-BC9F-9B5E015AB495",
			PollingPerSecond: 0,
			ClearAgentState: true,
			ClearAgentStateCode: "OFF"
		}, null, 4));

		this.Run = function () {
			result = new ResultViewModel();

			$.ajax({
				url: "Anywhere/BusinessUnit/Current",
				success: function (bu) {
					intervalFunc = setInterval(function () {
						for (var i = 0; i < self.PollingPerSecond() ; i++) {
							$.ajax({
								headers: { 'X-Business-Unit-Filter': bu },
								url: "Anywhere/Agents/GetStates?teamId=" + self.TeamId(),
							});
						}
					}, 1000);
				}
			});
			$.ajax({
			    url: "/api/PerformanceTool/resetperformancecounter?iterationCount=" + self.IterationsExpected(),
				success: function () {
					self.runIterations();
				}
			});
			return result;
		};
		
		self.runIterations = function() {
			var iterations = self.Iterations();
			startPromise.done(function() {
				manageAdherenceSubscription = messagebroker.subscribe({
					domainType: 'PerformanceCountDone',
					callback: function (notification) {
						clearInterval(intervalFunc);

						result.RunDone(true);
						result.IterationsDone(self.IterationsExpected());
						var data = JSON.parse(notification.BinaryData);
						result.StartTime(moment(data.StartTime));
						result.EndTime(moment(data.EndTime));

						if (self.ClearAgentState())
							self.runClearIterations();
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

		self.runClearIterations = function() {
			$.each(self.ClearIterations(), function(i, e) {
				e.Start();
			});
		};

	};
});