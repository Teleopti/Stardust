define([
		'knockout',
		'moment',
		'navigation',
		'lazy',
		'shared/layer',
		'views/teamschedule/shift-menu',
		'resources'
], function (
	ko,
	moment,
	navigation,
	lazy,
	layer,
	shiftMenu,
	resources
	) {

	return function(data, timeline) {

		var self = this;

		this.Layers = ko.observableArray();

		this.ShiftMenu = new shiftMenu();

		this.AddLayers = function (data) {
			var newItems = ko.utils.arrayMap(data.Projection, function (l) {
				l.Offset = data.Offset;
				l.IsFullDayAbsence = data.IsFullDayAbsence;
				return new layer(timeline, l);
			});
			self.Layers.push.apply(self.Layers, newItems);
			
			self.ShiftMenu.GroupId = data.GroupId;
			self.ShiftMenu.PersonId = data.PersonId;
			self.ShiftMenu.Date = data.Date;
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
