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
            this.ForcastedHours = ko.observable();
            this.ForcastedHoursDisplay = ko.computed(function () {
            	return self.Resources.Forecasted + self.ForcastedHours();
            });
            this.ScheduledHours = ko.observable();
	        this.ScheduledHoursDisplay = ko.computed(function() {
	        	return self.Resources.Scheduled + self.ScheduledHours();
	        });
            this.DiffHours = ko.observable();
            this.DiffPercentage = ko.observable();
            this.DifferenceDisplay = ko.computed(function () {
            	return self.Resources.Difference + self.DiffHours() + self.DiffPercentage();
            });
            this.ESL = ko.observable();
            this.ESLDisplay = ko.computed(function () {
            	return self.Resources.ESL + self.ESL();
            });
        };
    });
