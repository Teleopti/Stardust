define([
		'knockout',
		'moment',
		'navigation',
		'lazy',
		'shared/layer',
		'views/teamschedule/shift-menu'
], function (
	ko,
	moment,
	navigation,
	lazy,
	layer,
	shiftMenu
	) {

	return function(data, timeline) {

		var self = this;

		this.Layers = ko.observableArray();

		this.IsAnyLayerSelected = function () {
			// refact
			return $(self.Layers()).is(function (index) {
				return this.Selected();
			});
		};

		this.ShiftMenu = new shiftMenu();

		this.AddLayers = function (data) {
			var newItems = ko.utils.arrayMap(data.Projection, function (l) {
				l.Date = data.Date;
				l.IsFullDayAbsence = data.IsFullDayAbsence;
				return new layer(timeline, l);
			});
			self.Layers.push.apply(self.Layers, newItems);
			
			// refact
			var resultDate = undefined;
			ko.utils.arrayForEach(self.Layers(), function (l) {
				var startDate = l.StartDate();
				if (resultDate === undefined)
					resultDate = startDate;
				if (resultDate.diff(startDate, 'days') > 0)
					resultDate = startDate;
			});
			self.ShiftMenu.GroupId = data.GroupId;
			self.ShiftMenu.PersonId = data.PersonId;
			self.ShiftMenu.Date = resultDate;
		};

		this.ShiftStartPixels = ko.computed(function () {
			if (self.Layers().length > 0)
				return self.Layers()[0].StartPixels();
			return 0;
		});

		this.DistinctLayers = ko.computed(function () {
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
