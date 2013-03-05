define([
		'knockout',
		'navigation',
		'views/personschedule/layer',
		'views/personschedule/timeline',
		'helpers',
        'noext!application/resources'
	], function (
		ko,
	    navigation,
		layerViewModel,
		timeLineViewModel,
		helpers,
	    resources
		) {

		return function () {

			var self = this;

			this.Layers = ko.observableArray();

			this.TimeLine = new timeLineViewModel(this.Layers);

			this.Resources = resources;
		    
			this.Id = ko.observable("");
			this.Date = ko.observable();

			this.Name = ko.observable("");
			this.Site = ko.observable("");
			this.Team = ko.observable("");

			this.AddingFullDayAbsence = ko.observable(false);
		    
			this.SetData = function (data) {
				self.Name(data.Name);
				self.Site(data.Site);
				self.Team(data.Team);

				self.Layers([]);
				var layers = ko.utils.arrayMap(data.Layers, function (l) {
					l.Date = self.Date();
					return new layerViewModel(self.TimeLine, l);
				});
				self.Layers.push.apply(self.Layers, layers);

			};
		    
			this.AddFullDayAbsence = function () {
			    navigation.GotoPersonScheduleAddFullDayAbsenceForm(self.Id(), self.Date());
			    //self.AddingFullDayAbsence(true);
			};

		};
	});
