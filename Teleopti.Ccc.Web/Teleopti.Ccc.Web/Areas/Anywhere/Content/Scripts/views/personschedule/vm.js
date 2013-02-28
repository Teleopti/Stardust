define([
		'knockout',
		'views/personschedule/layer',
		'views/personschedule/timeline',
		'helpers'
	], function (
		ko,
		layerViewModel,
		timeLineViewModel,
		helpers
		) {

		return function () {

			var self = this;

			this.Layers = ko.observableArray();

			this.TimeLine = new timeLineViewModel(this.Layers);

			this.Id = ko.observable("");
			this.Date = ko.observable();

			this.Name = ko.observable("");
			this.Site = ko.observable("");
			this.Team = ko.observable("");

			this.SetData = function (data) {
				self.Name(data.Name);
				self.Site(data.Site);
				self.Team(data.Team);

				self.Layers([]);
				var layers = ko.utils.arrayMap(data.layers, function (l) {
					return new layerViewModel(timeLine, l);
				});
				self.Layers.push.apply(self.Layers, layers);

			};

		};
	});
