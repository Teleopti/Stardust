define([
        'knockout',
        'navigation',
		'views/personschedule/timeline',
        'noext!application/resources',
        'moment'
    ], function(
        ko,
        navigation,
        timeLineViewModel,
        resources,
        moment
    ) {

        return function() {

            var self = this;
            
            this.Loading = ko.observable(false);
            
            this.Persons = ko.observableArray();

            this.TimeLine = new timeLineViewModel(this.Persons);

            this.Resources = resources;

            this.Teams = ko.observableArray();
            this.SelectedTeam = ko.observable();
            this.SelectedDate = ko.observable(moment());

            this.SetPersons = function (persons) {
                self.Persons([]);
                self.Persons.push.apply(self.Persons, persons);
            };
            
            this.SetTeams = function (teams) {
                self.Teams([]);
                self.Teams.push.apply(self.Teams, teams);
            };

            this.NextDay = function() {
                self.SelectedDate(self.SelectedDate().add('d', 1));
            };

            this.PreviousDay = function() {
                self.SelectedDate(self.SelectedDate().add('d', -1));
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
            		return self.Resources.Forecasted + ': ' + forecastedHours.toFixed(2);
            	}
            	return '';
            });
            this.ScheduledHours = ko.observable();
            this.ScheduledHoursDisplay = ko.computed(function () {
            	var scheduledHours = self.ScheduledHours();
            	if (scheduledHours != undefined) {
            		return self.Resources.Scheduled + ': ' + scheduledHours.toFixed(2);
            	}
            	return '';
	        });
            this.DiffHours = ko.observable();
            this.DiffPercentage = ko.observable();
            this.DifferenceDisplay = ko.computed(function () {
	            var diffHours = self.DiffHours();
	            var diffPercentage = self.DiffPercentage();
	            if (diffHours!=undefined && diffPercentage!=undefined)
	            	return self.Resources.Difference + ': ' + diffPercentage.toFixed(2) + ', ' + (diffHours * 100).toFixed(2) + ' %';

	            return '';
            });
            this.ESL = ko.observable();
            this.ESLDisplay = ko.computed(function () {
            	var esl = self.ESL();
            	if (esl != undefined)
	            	return self.Resources.ESL + ': ' + esl;
	            return '';
            });
            this.SetDailyMetrics = function (data) {
	            self.ForecastedHours(data.ForecastedHours);
	            self.ESL(data.ESL);
	            self.ScheduledHours(data.ScheduledHours);
	            self.DiffHours(data.RelativeDifference);
	            self.DiffPercentage(data.AbsoluteDifferenceHours);
            };
        };
    });
