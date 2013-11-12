define([
        'knockout',
        'navigation',
        'lazy',
		'shared/timeline',
		'shared/group-page',
        'resources!r',
        'moment',
		'select2',
		'knockoutBindings'
    ], function(
        ko,
        navigation,
        lazy,
        timeLineViewModel,
	    groupPageViewModel,
        resources,
        moment,
	    select2,
	    knockoutBindings
    ) {

        return function() {

            var self = this;
            
            this.Loading = ko.observable(false);
            
            this.Persons = ko.observableArray();

            this.TimeLine = new timeLineViewModel(this.Persons);

            this.Resources = resources;

            this.GroupPages = ko.observableArray();
	        this.SelectedGroup = ko.observable();
            this.SelectedDate = ko.observable(moment());

            this.DisplayDescriptions = ko.observable(false);
            this.ToggleDisplayDescriptions = function() {
                self.DisplayDescriptions(!self.DisplayDescriptions());
            };
            
            this.SetPersons = function (persons) {
                self.Persons([]);
                self.Persons.push.apply(self.Persons, persons);
            };
	        
            this.SetGroupPages = function (data) {
            	self.GroupPages([]);
	            
            	var groupPages = data.GroupPages;
            	self.SelectedGroup(data.SelectedGroupId);
	            
            	var newItems = ko.utils.arrayMap(groupPages, function (d) {
            		return new groupPageViewModel(d);
            	});
            	self.GroupPages.push.apply(self.GroupPages, newItems);
            };
	        
            this.NextDay = function() {
                self.SelectedDate(self.SelectedDate().add('d', 1));
            };

            this.PreviousDay = function() {
                self.SelectedDate(self.SelectedDate().add('d', -1));
            };

            this.SelectPerson = function(person) {
                navigation.GotoPersonSchedule(self.SelectedGroup(), person.Id, self.SelectedDate());
            };
            
            this.SelectLayer = function (layer) {
                var selectedLayers = lazy(self.Persons())
                    .map(function(x) { return x.Shifts(); })
                    .flatten()
                    .map(function(x) { return x.Layers(); })
                    .flatten()
                    .filter(function (x) {
                        if (x === layer)
                            return false;
                        return x.Selected();
                    });
                selectedLayers.each(function (x) {
                    x.Selected(false);
                });
                layer.Selected(!layer.Selected());
            };

















	        
            this.Skills = ko.observableArray();
            this.SetSkills = function (skills) {
            	self.Skills([]);
	            if (skills.length > 0) {
		            self.Skills.push.apply(self.Skills, skills);
	            }
            };
	        this.SelectSkillById = function(id) {
	        	var skills = self.Skills();
		        var foundItem = ko.utils.arrayFirst(skills, function(item) {
			        return item.Id == id;
		        });
		        self.SelectedSkill(foundItem);
	        };
	        this.DisplayStaffingMetrics = ko.computed(function() {
		        return self.Skills().length > 0;
	        });
            this.SelectedSkill = ko.observable();
            this.SelectSkill = function (skill) {
            	self.SelectedSkill(skill);
            };
            this.SelectedSkillName = ko.computed(function () {
            	var skill = self.SelectedSkill();
	            if (!skill)
		            return undefined;
	            else
		            return skill.Name;
            });
	        

            this.LoadingStaffingMetrics = ko.observable(false);
	        
            this.ForecastedHours = ko.observable();
            this.ForecastedHoursDisplay = ko.computed(function () {
            	var forecastedHours = self.ForecastedHours();
            	if (forecastedHours != undefined) {
            		return self.Resources.Forecasted + ': ' + forecastedHours.toFixed(2) + ' ' + self.Resources.HourShort;
            	}
            	return self.Resources.Forecasted + ': ' + '__' + ' ' + self.Resources.HourShort;
            });
            this.ScheduledHours = ko.observable();
            this.ScheduledHoursDisplay = ko.computed(function () {
            	var scheduledHours = self.ScheduledHours();
            	if (scheduledHours != undefined) {
            		return self.Resources.Scheduled + ': ' + scheduledHours.toFixed(2) + ' ' + self.Resources.HourShort;
            	}
            	return self.Resources.Scheduled + ': ' + '__' + ' ' + self.Resources.HourShort;
	        });
            this.DiffHours = ko.observable();
            this.DiffPercentage = ko.observable();
            this.DifferenceDisplay = ko.computed(function () {
	            var diffHours = self.DiffHours();
	            var diffPercentage = self.DiffPercentage();
	            if (diffHours!=undefined && diffPercentage!=undefined)
	            	return self.Resources.Difference + ': ' + diffHours.toFixed(2) + ' ' + self.Resources.HourShort + ', ' + (diffPercentage * 100).toFixed(2) + ' %';

	            return self.Resources.Difference + ': ' + '__' + ' ' + self.Resources.HourShort + ', ' + '__' + ' %';
            });
            this.ESL = ko.observable();
            this.ESLDisplay = ko.computed(function () {
            	var esl = self.ESL();
            	if (esl != undefined)
            		return self.Resources.ESL + ': ' + (esl * 100).toFixed(2) + ' %';
	            return '';
            });
            this.SetDailyMetrics = function (data) {
	            self.ForecastedHours(data.ForecastedHours);
	            self.ESL(data.ESL);
	            self.ScheduledHours(data.ScheduledHours);
	            self.DiffHours(data.AbsoluteDifferenceHours);
	            self.DiffPercentage(data.RelativeDifference);
            };
        };
    });
