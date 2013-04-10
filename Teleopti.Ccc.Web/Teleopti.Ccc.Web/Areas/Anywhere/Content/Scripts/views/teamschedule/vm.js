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
		            self.SelectedSkill(skills[0]);
	            } else {
		            self.SelectedSkill(undefined);
	            }
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
            this.ScheduledHours = ko.observable();
            this.DiffHours = ko.observable();
            this.DiffPercentage = ko.observable();
            this.ESL = ko.observable();
        };
    });
