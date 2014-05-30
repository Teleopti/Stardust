define([
		'knockout',
		'moment',
		'addfulldayabsence/scenario',
		'jqueryajax'
], function (
	ko,
	moment,
	AddFullDayAbsenceScenario,
	JqueryAjax
	) {

	return function() {

		var self = this;

		this.Scenarios = [
			new AddFullDayAbsenceScenario(
				"PersonScheduleDayReadModel",
				function (notification) {
					if (this.PersonId == notification.DomainReferenceId) { return true; }
					return false;
				}),
			new AddFullDayAbsenceScenario(
				"ScheduledResourcesReadModel (BROKEN)",
				function () { return true; })
		];

		var ajax = new JqueryAjax();
		var currentTime = ko.observable();
		var runResult = ko.observable();

		this.Running = ko.computed(function () {
			var values = runResult();
			if (!values)
				return false;
			return !values.RunDone();
		});

		this.ConfigurationLoading = ko.computed(function () {
			return ajax.Active();
		});

		this.EnableForm = ko.computed(function () {
			if (self.Running())
				return false;
			if (self.ConfigurationLoading())
				return false;
			return true;
		});

		this.Scenario = ko.observable(this.Scenarios[0]);
		this.ScenarioName = ko.computed({
			read: function () {
				return self.Scenario().Name;
			},
			write: function (value) {
				for (var i = 0; i < self.Scenarios.length; i++) {
					var scenario = self.Scenarios[i];
					if (value == scenario.Name) {
						self.Scenario(scenario);
					}
				}
			}
		});

		this.Configuration = ko.computed({
			read: function () {
				return self.Scenario().Configuration();
			},
			write: function(value) {
				self.Scenario().Configuration(value);
			}
		});

		this.ConfigurationError = ko.computed(function() {
			return self.Scenario().ConfigurationError();
		});

		this.ProgressItems = ko.computed(function() {
			return self.Scenario().ProgressItems;
		});
		
		this.RunButtonEnabled = ko.computed(function () {
			if (self.ConfigurationError())
				return false;
			return self.EnableForm();
		});
		
		this.RunButtonText = ko.computed(function () {
			var configurationError = self.ConfigurationError();
			if (configurationError)
				return configurationError;
			var scenario = self.Scenario();
			
			if (scenario && scenario.IterationsExpected())
				return "Run " + scenario.IterationsExpected() + " scenarios";
			return "Dont click me yet please";
		});
		
		this.Run = function () {
		    var result = self.Scenario().Run();
		    runResult(result);
		    result.StartTime(moment());
		};

		var formatTimeDiff = function(first, second) {
			var seconds = second.diff(first, 'seconds');
			return seconds + " seconds";
		};

		this.TotalRunTime = ko.computed(function() {
			var values = runResult();
			if (!values)
				return null;
			var startTime = values.StartTime();
			if (startTime) {
				var endTime = values.EndTime() || currentTime();
				return formatTimeDiff(startTime, endTime);
			}
			return null;
		});
		
		this.ScenariosPerSecond = ko.computed(function () {
			var values = runResult();
			if (!values)
				return null;
			var iterations = values.IterationsDone();
			var startTime = values.StartTime();
			var endTime = values.EndTime() || currentTime();
			if (iterations) {
				var seconds = endTime.diff(startTime, 'seconds');
				return (iterations / seconds).toFixed(2);
			}
			return null;
		});
		
		this.TotalTimeToSendCommands = ko.computed(function () {
			var values = runResult();
			if (!values)
				return null;
			var startTime = values.StartTime();
			if (startTime) {
				var endTime = values.CommandEndTime() || currentTime();
				return formatTimeDiff(startTime, endTime);
			}
			return null;
		});

		this.RunDone = ko.computed(function () {
			var values = runResult();
			if (values)
				return values.RunDone();
			return false;
		});

		setInterval(function() {
			currentTime(moment());
		}, 100);
	};

});

