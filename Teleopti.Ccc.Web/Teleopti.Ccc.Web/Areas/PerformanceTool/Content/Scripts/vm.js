
define([
        'knockout',
        'scenario-addremovefulldayabsence'
], function (
    ko,
    AddRemoveFullDayAbsenceScenarioViewModel
	) {


    return function() {

        var self = this;
        
        this.Scenarios = [new AddRemoveFullDayAbsenceScenarioViewModel()];
        this.Scenario = ko.observable();
        this.Configuration = ko.observable();
        
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
            return "Run " + scenario.Count() + " scenarios";
        });

        this.Scenario.subscribe(function() {
            self.Configuration(JSON.stringify(self.Scenario().LoadDefaultConfiguration(), null, 4));
        });
    };

});

