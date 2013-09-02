
define([
        'knockout',
        'moment',
        'scenario-addremovefulldayabsence',
        'scenario-resourcecalculationaddremovefulldayabsence'
], function (
    ko,
    moment,
    AddRemoveFullDayAbsenceScenarioViewModel,
    ResourceCalculationAddRemoveFullDayAbsenceScenarioViewModel
	) {


    return function() {

        var self = this;
        
        this.Scenarios = [new AddRemoveFullDayAbsenceScenarioViewModel(), new ResourceCalculationAddRemoveFullDayAbsenceScenarioViewModel()];
        this.ScenarioName = ko.observable();
        this.Scenario = ko.observable();
        this.Configuration = ko.observable();
        this.ConfigurationLoading = ko.observable(false);

        var currentTime = ko.observable();
        var runResult = ko.observable();

        this.Running = ko.computed(function () {
            var values = runResult();
            if (!values)
                return false;
            return !values.RunDone();
        });

        this.EnableForm = ko.computed(function () {
            if (self.Running())
                return false;
            if (self.ConfigurationLoading())
                return false;
            return true;
        });
        
        this.ScenarioName.subscribe(function () {
            self.ConfigurationLoading(true);
            
            for (var i = 0; i < self.Scenarios.length; i++) {
                if (self.ScenarioName() == self.Scenarios[i].Name) {
                    self.Scenario(self.Scenarios[i]);
                    self.Configuration(''); //just for now
                    self.Scenario().LoadDefaultConfiguration(function (data) {
                        self.Configuration(JSON.stringify(data, null, 4));
                        self.ConfigurationLoading(false);
                    });
                    break;
                }
            }
            
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

