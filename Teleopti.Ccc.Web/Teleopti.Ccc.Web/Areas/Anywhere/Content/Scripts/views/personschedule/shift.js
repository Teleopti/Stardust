define([
		'knockout',
		'moment',
		'navigation',
		'lazy',
		'shared/layer'
], function (
	ko,
	moment,
	navigation,
	lazy,
	layer
	) {

	return function(data, timeline) {

		var self = this;

		this.Layers = ko.observableArray();

		this.IsAnyLayerSelected = function () {
			return $(self.Layers()).is(function (index) {
				return this.Selected();
			});
		};
		
		// refact
		this.IsFullDayAbsence = ko.observable();
		
		this.AddLayers = function (data) {
			var newItems = ko.utils.arrayMap(data.Projection, function (l) {
				l.Date = data.Date;
				l.IsFullDayAbsence = data.IsFullDayAbsence;
				// refact
				self.IsFullDayAbsence(data.IsFullDayAbsence);
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
	};
});
