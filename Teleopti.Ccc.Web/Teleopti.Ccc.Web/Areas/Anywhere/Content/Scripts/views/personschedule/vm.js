define([
        'knockout',
        'navigation',
        'views/personschedule/layer',
        'shared/timeline',
        'views/personschedule/addfulldayabsenceform',
        'views/personschedule/absencelistitem',
        'helpers',
        'resources!r'
    ], function(
        ko,
        navigation,
        layerViewModel,
        timeLineViewModel,
        addFullDayAbsenceFormViewModel,
        absenceListItemViewModel,
        helpers,
        resources
    ) {

        return function() {

            var self = this;

            this.Loading = ko.observable(false);
            
            this.Layers = ko.observableArray([]);
	        
            this.IsDayOff = ko.observable(false);
	        
            this.IsShift = ko.computed(function () {
            	return !self.IsDayOff();
            });

            this.Absences = ko.observableArray();

            this.TimeLine = new timeLineViewModel(this.Layers);

            this.Resources = resources;

            this.Id = ko.observable("");
            this.Date = ko.observable();

            this.Name = ko.observable("");
            this.Site = ko.observable("");
            this.Team = ko.observable("");

            this.AddFullDayAbsenceForm = new addFullDayAbsenceFormViewModel();

            this.AddingFullDayAbsence = ko.observable(false);

            this.DisplayDescriptions = ko.observable(false);
            this.ToggleDisplayDescriptions = function () {
                self.DisplayDescriptions(!self.DisplayDescriptions());
            };

            this.SetData = function(data) {
                data.Date = self.Date();
                data.PersonId = self.Id();

                self.Name(data.Name);
                self.Site(data.Site);
                self.Team(data.Team);
                self.IsDayOff(data.IsDayOff);

                self.Layers([]);
                var layers = ko.utils.arrayMap(data.Layers, function(l) {
                    l.Date = self.Date();
                    l.IsFullDayAbsence = data.IsFullDayAbsence;
                    return new layerViewModel(self.TimeLine, l);
                });
                self.Layers.push.apply(self.Layers, layers);

                self.Absences([]);
                var absences = ko.utils.arrayMap(data.PersonAbsences, function(a) {
                    a.PersonId = self.Id();
                    a.Date = self.Date();
                    return new absenceListItemViewModel(a);
                });
                self.Absences.push.apply(self.Absences, absences);

                self.AddFullDayAbsenceForm.SetData(data);
            };

            this.AddFullDayAbsence = function() {
                navigation.GotoPersonScheduleAddFullDayAbsenceFormWithoutHistory(self.Id(), self.Date());
            };

        };
    });
