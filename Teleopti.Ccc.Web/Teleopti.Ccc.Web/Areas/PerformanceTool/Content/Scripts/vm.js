
define([
        'knockout',
        'moment',
        'scenario-addremovefulldayabsence'
], function (
    ko,
    moment,
    AddRemoveFullDayAbsenceScenarioViewModel
	) {


    return function() {

        var self = this;
        
        this.Scenarios = [new AddRemoveFullDayAbsenceScenarioViewModel()];
        this.Scenario = ko.observable();
        this.Configuration = ko.observable();
        
        this.Scenario.subscribe(function () {
            self.Scenario().LoadDefaultConfiguration(function (data) {
                self.Configuration(JSON.stringify(data, null, 4));
            });
        });

        this.Configuration.subscribe(function () {
            
            try {
                var configuration = JSON.parse(self.Configuration());
            } catch (e) {
                self.ConfigurationError("Invalid configuration");
                return;
            }

            var scenario = self.Scenario();
            try {
                scenario.ConfigurationChanged(configuration);
            } catch (e) {
                self.ConfigurationError(e);
                return;
            }
            
            self.ConfigurationError(null);
        });
        
        this.ConfigurationError = ko.observable();


        var currentTime = ko.observable();
        var runResult = ko.observable();

        this.IsRunning = ko.computed(function () {
            var values = runResult();
            if (!values)
                return false;
            return !values.RunDone();
        });

        this.RunButtonEnabled = ko.computed(function () {
            if (self.ConfigurationError())
                return false;
            return !self.IsRunning();
        });
        
        this.RunButtonText = ko.computed(function () {
            var configurationError = self.ConfigurationError();
            if (configurationError)
                return configurationError;
            var scenario = self.Scenario();
            if (scenario && scenario.IterationsExpected())
                return "Run " + scenario.IterationsExpected() + " scenarios";
            return "Select scenario";
        });
        
        this.Run = function () {
            var result = self.Scenario().Run();
            result.StartTime(moment());
            runResult(result);
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

