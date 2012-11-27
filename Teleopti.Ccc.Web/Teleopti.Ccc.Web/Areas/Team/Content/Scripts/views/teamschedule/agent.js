define([
		'knockout',
		'helpers'
	], function (
		ko,
		helpers
		) {

	    return function (name, layers, contractTime, workTime) {
	        var self = this;
	        this.Id = helpers.Guid.Create();

	        this.Name = name;
	        this.Layers = ko.observableArray(layers);
	        this.ContractTime = ko.observable(contractTime);
	        this.WorkTime = ko.observable(workTime);

	        this.FirstStartMinute = ko.computed(function () {
	            var start = undefined;
	            ko.utils.arrayForEach(self.Layers(), function (l) {
	                var startMinutes = l.StartMinutes();
	                if (!start)
	                    start = startMinutes;
	                if (startMinutes < start)
	                    start = startMinutes;
	            });
	            return start;
	        });

	        this.LastEndMinute = ko.computed(function () {
	            var end = undefined;
	            ko.utils.arrayForEach(self.Layers(), function (l) {
	                var endMinutes = l.EndMinutes();
	                if (!end)
	                    end = endMinutes;
	                if (endMinutes > end)
	                    end = endMinutes;
	            });
	            return end;
	        });
	    };
	});
