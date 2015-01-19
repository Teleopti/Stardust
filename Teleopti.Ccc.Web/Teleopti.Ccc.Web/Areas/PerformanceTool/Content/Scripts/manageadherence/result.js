
define([
        'knockout'
    ], function(
        ko
    ) {

        return function () {

            var self = this;
            
            this.IterationsDone = ko.observable(0);
            this.CommandsDone = ko.observable(false);
            this.RunDone = ko.observable(false);

            this.StartTime = ko.observable();
            this.EndTime = ko.observable();

            this.CommandEndTime = ko.computed(function () {
            	if (self.CommandsDone())
            		return moment();
            	else
            		return null;
            });
        };
    });