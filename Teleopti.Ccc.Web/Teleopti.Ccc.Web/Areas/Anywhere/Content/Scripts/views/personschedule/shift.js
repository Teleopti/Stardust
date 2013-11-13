define([
	'knockout',
	'moment',
	'navigation',
	'lazy',
	'views/personschedule/layer',
	'resources!r'
], function (
	ko,
	moment,
	navigation,
	lazy,
	layer,
	resources
	) {

	return function (timeline, selectedGroup) {

		var self = this;

		this.Layers = ko.observableArray();
		var date, personId;

		var groupId = selectedGroup;
		
		this.AddLayers = function (data) {
			personId = data.PersonId;
			var layers = data.Projection != undefined ? data.Projection : data.Layers;
			var newItems = ko.utils.arrayMap(layers, function (l) {
				date = date || moment(l.Start).startOf('day');
				l.Date = data.Date;
				l.IsFullDayAbsence = data.IsFullDayAbsence;
				return new layer(timeline, l, self);
			});
			self.Layers.push.apply(self.Layers, newItems);
		};
		
		this.ShiftStartPixels = ko.computed(function () {
			if (self.Layers().length > 0)
				return self.Layers()[0].StartPixels();
			return 0;
		});
		
		this.AddFullDayAbsence = function () {
			navigation.GotoPersonScheduleAddFullDayAbsenceForm(groupId, personId, date);
		};

		this.AddActivity = function () {
			navigation.GotoPersonScheduleAddActivityForm(groupId, personId, date);
		};

		this.AddAbsence = function () {
			navigation.GotoPersonScheduleAddAbsenceForm(groupId, personId, date);
		};
	};
});
