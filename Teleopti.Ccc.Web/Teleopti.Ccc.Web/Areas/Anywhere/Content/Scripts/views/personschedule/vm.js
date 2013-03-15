define([
		'knockout',
		'navigation',
		'views/personschedule/layer',
		'views/personschedule/timeline',
        'views/personschedule/addfulldayabsenceform',
		'helpers',
        'noext!application/resources'
	], function (
		ko,
	    navigation,
		layerViewModel,
		timeLineViewModel,
	    addFullDayAbsenceFormViewModel,
		helpers,
	    resources
		) {

		return function () {

			var self = this;

			this.Loading = ko.observable(false);

			this.Layers = ko.observableArray();

			this.TimeLine = new timeLineViewModel(this.Layers);

			this.Resources = resources;
		    
			this.Id = ko.observable("");
			this.Date = ko.observable();

			this.Name = ko.observable("");
			this.Site = ko.observable("");
			this.Team = ko.observable("");

		    this.AddFullDayAbsenceForm = new addFullDayAbsenceFormViewModel();
		    
			this.AddingFullDayAbsence = ko.observable(false);
		    
			this.SetData = function (data) {
			    data.Date = self.Date();
			    data.PersonId = self.Id();

				self.Name(data.Name);
				self.Site(data.Site);
				self.Team(data.Team);

				self.Layers([]);
				var layers = ko.utils.arrayMap(data.Layers, function (l) {
					l.Date = self.Date();
					return new layerViewModel(self.TimeLine, l);
				});
				self.Layers.push.apply(self.Layers, layers);
			    
				self.AddFullDayAbsenceForm.SetData(data);
			};
		    
			this.AddFullDayAbsence = function () {
			    navigation.GotoPersonScheduleAddFullDayAbsenceForm(self.Id(), self.Date());
			};

		};
	});
