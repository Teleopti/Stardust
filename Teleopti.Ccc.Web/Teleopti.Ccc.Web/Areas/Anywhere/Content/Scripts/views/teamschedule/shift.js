define([
		'knockout',
		'moment',
		'navigation',
		'lazy',
		'views/teamschedule/layer',
		'shared/shiftmenu'
], function (
	ko,
	moment,
	navigation,
	lazy,
	layer,
	shiftMenu
	) {

	return function(timeline, groupid, personid, date) {

		var self = this;

		this.Layers = ko.observableArray();

		this.ShiftMenu = new shiftMenu(groupid, personid, date);

		this.AddLayers = function (data) {
			var layers = data.Projection;
			var newItems = ko.utils.arrayMap(layers, function(l) {
				l.Date = data.Date;
				l.IsFullDayAbsence = data.IsFullDayAbsence;
				return new layer(timeline, l, self);
			});
			self.Layers.push.apply(self.Layers, newItems);
		};

		this.StartsOnSelectedDay = ko.computed(function () {
			return self.Layers().length > 0 && self.Layers()[0].StartMinutes() < 25 * 60;
		});

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
