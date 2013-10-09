define([
        'knockout',
        'moment',
        'views/teamschedule/layer',
        'resources!r'
    ], function(
        ko,
        moment,
        layer,
        resources
    ) {

        return function(data, events) {
            var self = this;

            this.Id = data.Id;
            this.Name = ko.observable(data.FirstName + ' ' + data.LastName);

            this.Layers = ko.observableArray();
            this.WorkTimeMinutes = ko.observable(0);
            this.ContractTimeMinutes = ko.observable(0);
	        
            this.IsDayOff = ko.observable(false);

	        this.IsShift = ko.computed(function() {
		        return !self.IsDayOff();
	        });

	        this.IsFullDayAbsence = false;
	        
            this.ContractTime = ko.computed(function() {
                var time = moment().startOf('day').add('minutes', self.ContractTimeMinutes());
                return time.format("H:mm");
            });

            this.WorkTime = ko.computed(function() {
                var time = moment().startOf('day').add('minutes', self.WorkTimeMinutes());
                return time.format("H:mm");
            });

	        this.ClearData = function() {
	        	self.Layers([]);
	        	self.WorkTimeMinutes(0);
		        self.ContractTimeMinutes(0);
	        };
	        
	        this.AddData = function (data, timeline, date) {
	        	var layers = data.Projection;
	        	var newItems = ko.utils.arrayMap(layers, function (p) {
	        		return new layer(timeline, p, date);
	        	});
	        	self.Layers.push.apply(self.Layers, newItems);

	        	self.IsFullDayAbsence = data.IsFullDayAbsence;
	        	self.IsDayOff(data.DayOffStartTime != undefined && data.DayOffEndTime != undefined);
		        
	        	self.ContractTimeMinutes(self.ContractTimeMinutes() + data.ContractTimeMinutes);
	        	self.WorkTimeMinutes(self.WorkTimeMinutes() + data.WorkTimeMinutes);
            };
			
            this.TimeLineAffectingStartMinute = ko.computed(function() {
                var start = undefined;
                ko.utils.arrayForEach(self.Layers(), function(l) {
                    var startMinutes = l.StartMinutes();
                    if (start === undefined)
                        start = startMinutes;
                    if (startMinutes < start)
                        start = startMinutes;
                });
                return start;
            });

            this.TimeLineAffectingEndMinute = ko.computed(function() {
                var end = undefined;
                ko.utils.arrayForEach(self.Layers(), function(l) {
                    var endMinutes = l.EndMinutes();
                    if (end === undefined)
                        end = endMinutes;
                    if (endMinutes > end)
                        end = endMinutes;
                });
                return end;
            });

            this.Select = function() {
                events.notifySubscribers(self.Id, "gotoperson");
            };

	        this.CompareTo = function(other) {

	        	var first = self;
	        	var second = other;
		        
	        	/*
				return a negative value if the first argument is smaller, 
				a positive value is the second is smaller, 
				or zero to treat them as equal.
				*/

	        	var firstNoShift = first.Layers().length == 0 && !first.IsDayOff();
	        	var secondNoShift = second.Layers().length == 0 && !second.IsDayOff();
	        	if (firstNoShift && second.IsDayOff())
	        		return 1;
	        	if (secondNoShift && first.IsDayOff())
	        		return -1;

	        	if (first.IsDayOff() && second.IsFullDayAbsence)
	        		return 1;
	        	if (second.IsDayOff() && first.IsFullDayAbsence)
	        		return -1;

	        	if (first.IsFullDayAbsence && !second.IsFullDayAbsence)
	        		return 1;
	        	if (second.IsFullDayAbsence && !first.IsFullDayAbsence)
	        		return -1;

	        	var firstStartMinutes = first.TimeLineAffectingStartMinute();
	        	var secondStartMinutes = second.TimeLineAffectingStartMinute();
	        	if (firstStartMinutes > secondStartMinutes)
	        		return 1;
	        	if (firstStartMinutes < secondStartMinutes)
	        		return -1;

	        	var firstEndMinutes = first.TimeLineAffectingEndMinute();
	        	var secondEndMinutes = second.TimeLineAffectingEndMinute();
	        	if (firstEndMinutes > secondEndMinutes)
	        		return 1;
	        	if (firstEndMinutes < secondEndMinutes)
	        		return -1;

	        	return 0;
	        };
        };
    });
