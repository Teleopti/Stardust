define([
		'knockout',
		'moment',
		'navigation',
		'lazy',
		'shared/layer',
		'views/newteamschedule/shift-menu'
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

		this.ShiftMenu = new shiftMenu();

		this.AddLayers = function (data) {
			var newItems = ko.utils.arrayMap(data.Projection, function (l) {
				l.Offset = data.Offset;
				l.IsFullDayAbsence = data.IsFullDayAbsence;
				return new layer(timeline, l, true);
			});
			self.Layers.push.apply(self.Layers, newItems);

			self.ShiftMenu.BusinessUnitId = data.BusinessUnitId;
			self.ShiftMenu.GroupId = data.GroupId;
			self.ShiftMenu.PersonId = data.PersonId;
			self.ShiftMenu.Date = data.Date;
		};

		this.ShiftStartPixels = ko.computed(function () {
			var layers = self.Layers();
			if (layers.length > 0)
				return layers[0].StartPixels();
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
