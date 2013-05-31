
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

        this.RunButtonEnabled = ko.observable();
        this.RunButtonText = ko.computed(function () {
            
            try {
                var configuration = JSON.parse(self.Configuration());
            } catch (e) {
                self.RunButtonEnabled(false);
                return "Invalid configuration";
            }
            
            var scenario = self.Scenario();
            scenario.ConfigurationChanged(configuration);
            self.RunButtonEnabled(true);
            return "Run " + scenario.IterationsExpected() + " scenarios";
        });

        var currentTime = ko.observable();
        var run = ko.observable();
        
        this.Run = function () {
            var result = self.Scenario().Run();
            result.StartTime(moment());
            run(result);
        };

        var formatTimeDiff = function(first, second) {
            var seconds = second.diff(first, 'seconds');
            return seconds + " seconds";
        };

        this.TotalRunTime = ko.computed(function() {
            var runInfo = run();
            if (!runInfo)
                return null;
            var startTime = runInfo.StartTime();
            if (startTime) {
                var endTime = runInfo.EndTime() || currentTime();
                return formatTimeDiff(startTime, endTime);
            }
            return null;
        });
        
        this.ScenariosPerSecond = ko.computed(function () {
            var runInfo = run();
            if (!runInfo)
                return null;
            var iterations = runInfo.IterationsDone();
            var startTime = runInfo.StartTime();
            var endTime = runInfo.EndTime() || currentTime();
            if (iterations) {
                return (iterations / (endTime.diff(startTime, 'seconds'))).toFixed(2);
            }
            return null;
        });
        
        this.TotalTimeToSendCommands = ko.computed(function () {
            var runInfo = run();
            if (!runInfo)
                return null;
            var startTime = runInfo.StartTime();
            if (startTime) {
                var endTime = runInfo.CommandEndTime() || currentTime();
                return formatTimeDiff(startTime, endTime);
            }
            return null;
        });

        this.RunDone = ko.computed(function () {
            var runInfo = run();
            if (runInfo)
                return runInfo.RunDone();
            return false;
        });

        setInterval(function() {
            currentTime(moment());
        }, 100);
    };

});

