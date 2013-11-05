define([
		'knockout',
		'moment',
		'navigation',
        'lazy',
		'views/teamschedule/layer',
		'resources!r'
], function (
	ko,
	moment,
	navigation,
    lazy,
	layer,
	resources
	) {

	return function (timeline) {

		var self = this;
		
		this.Layers = ko.observableArray();
		var date, personId;
	    
		this.AddLayers = function (data) {
		    personId = data.PersonId;
			var layers = data.Projection;
			var newItems = ko.utils.arrayMap(layers, function (l) {
			    date = date || moment(l.Start).startOf('day');
				l.Date = data.Date;
				l.IsFullDayAbsence = data.IsFullDayAbsence;
				return new layer(timeline, l, self);
			});
			self.Layers.push.apply(self.Layers, newItems);
		};

		this.AnyLayerSelected = ko.computed(function () {
		    return lazy(self.Layers()).some(function (x) { return x.Selected(); });
		});

	    this.MakeSpaceForDrop = ko.computed(function() {
	        if (!self.AnyLayerSelected())
	            return false;
	        var aDropWillBeDisplayed = lazy(self.Layers()).some(function (x) { return x.DisplayDrop(); });
	        return aDropWillBeDisplayed;
	    });

		this.ShowDetails = function () {
		    navigation.GotoPersonSchedule(personId, date);
		};

	    this.AddFullDayAbsence = function() {
	        navigation.GotoPersonScheduleAddFullDayAbsenceForm(personId, date);
	    };

	    this.AddActivity = function () {
	        navigation.GotoPersonSchedule(personId, date);
	    };

		this.DistinctLayers = ko.computed(function() {
			var array = self.Layers();
			var result = [];
			var names = [];
			for (var i = 0, j = array.length; i < j; i++) {
				if (ko.utils.arrayIndexOf(names, array[i].Description) < 0) {
					result.push(array[i]);
					names.push(array[i].Description);
				}
			}
			return result;
		});

	};
});
