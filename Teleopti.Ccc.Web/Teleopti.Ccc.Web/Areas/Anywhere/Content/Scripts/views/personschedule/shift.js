define([
		'knockout',
		'moment',
		'navigation',
		'lazy',
		'shared/layer',
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
			var layers = data.Projection != undefined ? data.Projection : data.Layers;
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
	};
});
