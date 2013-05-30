
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
            self.Configuration(JSON.stringify(self.Scenario().LoadDefaultConfiguration(), null, 4));
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
        var runStartTime;
        var runEndTime;
        var commandsEndTime;
        
        this.Run = function () {
            var scenario = self.Scenario();

            var commandsDoneSub = scenario.CommandsDone.subscribe(function () {
                commandsEndTime = moment();
                commandsDoneSub.dispose();
            });
            
            var runDoneSub = scenario.RunDone.subscribe(function() {
                runEndTime = moment();
                runDoneSub.dispose();
            });
            
            runStartTime = moment();
            
            self.Scenario().Run();
        };

        var formatTimeDiff = function(first, second) {
            var seconds = second.diff(first, 'seconds');
            return seconds + " seconds";
        };
        
        this.TotalRunTime = ko.computed(function () {
            var clock = currentTime();
            if (runStartTime)
                if (runEndTime)
                    return formatTimeDiff(runStartTime, runEndTime);
                else
                    return formatTimeDiff(runStartTime, clock);
            return null;
        });
        
        this.ScenariosPerSecond = ko.computed(function () {
            if (self.Scenario() && self.Scenario().IterationsDone()) {
                if (runEndTime) {
                    return (self.Scenario().IterationsDone() / (runEndTime.diff(runStartTime, 'seconds'))).toFixed(2);
                } else {
                    return (self.Scenario().IterationsDone() / (currentTime().diff(runStartTime, 'seconds'))).toFixed(2);
                }
            }
            return null;
        });
        
        this.TotalTimeToSendCommands = ko.computed(function () {
            var clock = currentTime();
            if (runStartTime)
                if (commandsEndTime)
                    return formatTimeDiff(runStartTime, commandsEndTime);
                else
                    return formatTimeDiff(runStartTime, clock);
            return null;
        });
       
        setInterval(function() {
            currentTime(moment());
        }, 100);
    };

});

