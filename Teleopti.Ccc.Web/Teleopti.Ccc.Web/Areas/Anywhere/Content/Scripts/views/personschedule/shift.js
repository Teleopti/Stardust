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

		this.AddLayers = function (data) {
			var newItems = ko.utils.arrayMap(data.Projection, function (l) {
				l.Offset = data.Offset;
				l.IsFullDayAbsence = data.IsFullDayAbsence;
				var affectTimeLine = data.Date.diff(data.Offset) == 0;
				return new layer(timeline, l, affectTimeLine);
			});
			self.Layers.push.apply(self.Layers, newItems);
		};

		this.ShiftStartPixels = ko.computed(function () {
			if (self.Layers().length > 0)
				return self.Layers()[0].StartPixels();
			return 0;
		});
	};
});
