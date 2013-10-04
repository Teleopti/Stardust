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
	        
            this.IsDayOff = ko.observable(data.IsDayOff);

	        this.IsShift = ko.computed(function() {
		        return !self.IsDayOff();
	        });

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
        };
    });
