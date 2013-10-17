
define([
		'knockout',
		'jquery',
		'navigation',
		'moment',
		'subscriptions',
		'helpers',
		'views/teamschedule/vm',
		'views/teamschedule/person',
		'text!templates/teamschedule/view.html',
        'resizeevent',
        'ajax'
	], function (
		ko,
		$,
		navigation,
		momentX,
		subscriptions,
		helpers,
		teamScheduleViewModel,
		personViewModel,
		view,
	    resize,
	    ajax
	) {

		var teamSchedule;

		var events = new ko.subscribable();

		events.subscribe(function (personId) {
		    navigation.GotoPersonSchedule(personId, teamSchedule.SelectedDate());
		}, null, "gotoperson");

		var loadSchedules = function(options) {
		    subscriptions.subscribeTeamSchedule(
				teamSchedule.SelectedTeam(),
				helpers.Date.ToServer(teamSchedule.SelectedDate()),
				function(schedules) {
				    var currentPersons = teamSchedule.Persons();

				    var dateClone = teamSchedule.SelectedDate().clone();

				    for (var i = 0; i < schedules.length; i++) {
				    	for (var j = 0; j < currentPersons.length; j++) {
				    		if (currentPersons[j].Id == schedules[i].Id) {
				    			currentPersons[j].ClearLayers();
				    			break;
				    		}
				    	}
				    }
					
					for (var i = 0; i < schedules.length; i++) {
						for (var j = 0; j < currentPersons.length; j++) {
						    if (currentPersons[j].Id == schedules[i].Id) {
						        currentPersons[j].AddLayers(schedules[i].Projection, teamSchedule.TimeLine, dateClone);
						        currentPersons[j].AddContractTime(schedules[i].ContractTimeMinutes);
						        currentPersons[j].AddWorkTime(schedules[i].WorkTimeMinutes);
								break;
							}
						}
					}

					currentPersons.sort(function (a, b) {
						var firstStartMinutes = a.TimeLineAffectingStartMinute();
						var secondStartMinutes = b.TimeLineAffectingStartMinute();
	                    return firstStartMinutes == secondStartMinutes ? (a.TimeLineAffectingEndMinute() == b.TimeLineAffectingEndMinute() ? 0 : a.TimeLineAffectingEndMinute() < b.TimeLineAffectingEndMinute() ? -1 : 1) : firstStartMinutes < secondStartMinutes ? -1 : 1;
					});

					teamSchedule.Persons.valueHasMutated();

					options.success();
				    
					resize.notify();
				});
		};

		var loadPersons = function (options) {
		    ajax.ajax({
				url: 'Person/PeopleInTeam',
				data: {
				    date: helpers.Date.ToServer(teamSchedule.SelectedDate()),
					teamId: teamSchedule.SelectedTeam()
				},
				success: function (people, textStatus, jqXHR) {
					var newItems = ko.utils.arrayMap(people, function (s) {
						return new personViewModel(s, events);
					});
					teamSchedule.SetPersons(newItems);
					options.success();
				}
			});
		};
		
		var loadSkills = function (options) {
			$.ajax({
				url: 'StaffingMetrics/AvailableSkills',
				cache: false,
				dataType: 'json',
				data: {
					date: teamSchedule.SelectedDate().toDate().toJSON()
				},
				success: function (data, textStatus, jqXHR) {
					teamSchedule.SetSkills(data.Skills);
					options.success();
				}
			});
		};

		var loadDailyStaffingMetrics = function (options) {
			if (teamSchedule.SelectedSkill() == null) return;
			$.ajax({
				url: 'StaffingMetrics/DailyStaffingMetrics',
				cache: false,
				dataType: 'json',
				data: {
					skillId: teamSchedule.SelectedSkill().Id,
					date: teamSchedule.SelectedDate().toDate().toJSON()
				},
				success: function (data, textStatus, jqXHR) {
					teamSchedule.SetDailyMetrics(data);
					options.success();
				}
			});
		};
		var loadTeams = function (options) {
		    ajax.ajax({
				url: 'Person/AvailableTeams',
				data: {
				    date: helpers.Date.ToServer(teamSchedule.SelectedDate()),
				},
				success: function (data, textStatus, jqXHR) {
					teamSchedule.SetTeams(data.Teams);
					options.success();
				}
			});
		};

		return {
			initialize: function (options) {

				options.renderHtml(view);

				teamSchedule = new teamScheduleViewModel();

				resize.onresize(function () {
				    teamSchedule.TimeLine.WidthPixels($('.shift').width());
				});

				teamSchedule.SelectedTeam.subscribe(function () {
					if (teamSchedule.Loading())
						return;
					navigation.GoToTeamSchedule(teamSchedule.SelectedTeam(), teamSchedule.SelectedDate(), teamSchedule.SelectedSkill());
				});

				teamSchedule.SelectedDate.subscribe(function() {
					if (teamSchedule.Loading())
						return;
					navigation.GoToTeamSchedule(teamSchedule.SelectedTeam(), teamSchedule.SelectedDate(), teamSchedule.SelectedSkill());
				});

				teamSchedule.SelectedSkill.subscribe(function () {
					if (teamSchedule.Loading())
						return;
					navigation.GoToTeamSchedule(teamSchedule.SelectedTeam(), teamSchedule.SelectedDate(), teamSchedule.SelectedSkill());
				});
				
				ko.applyBindings(teamSchedule, options.bindingElement);
			},
			
			display: function (options) {

			    var currentTeamId = function () {
					if (options.id)
						return options.id;
					if (teamSchedule.Teams().length > 0)
						return teamSchedule.Teams()[0].Id;
					return null;
				};
				
				var currentSkillId = function () {
					if (options.secondaryId)
						return options.secondaryId;
					var skills = teamSchedule.Skills();
					if (skills.length > 0)
						return skills[0].Id;
					return null;
				};

				var currentDate = function () {
					var date = options.date;
					if (date == undefined) {
		                return moment().startOf('day');
					} else {
						return moment(date, 'YYYYMMDD');
					}
				};
				teamSchedule.Loading(true);

				teamSchedule.SelectedDate(currentDate());
				loadSkills({
					success: function() {
						teamSchedule.SelectSkillById(currentSkillId());
						teamSchedule.LoadingStaffingMetrics(true);
						loadDailyStaffingMetrics({
							success: function () {
								teamSchedule.LoadingStaffingMetrics(false);
							}
						});
					}
				});

				var deferred = $.Deferred();
				var loadPersonsAndSchedules = function () {
					var currentTeam = currentTeamId();
					if (!currentTeam) {
						teamSchedule.Loading(false);
						deferred.resolve();
						return;
					}
					teamSchedule.SelectedTeam(currentTeam);
					loadPersons({
						success: function() {
							loadSchedules({
								success: function () {
								    teamSchedule.Loading(false);
								    deferred.resolve();
								}
							});
						}
					});
				};
				
				if (teamSchedule.Teams().length != 0) {
					loadPersonsAndSchedules();
				} else {
					loadTeams({
						success: loadPersonsAndSchedules
					});
				}

			    return deferred.promise();
			},
			
			dispose: function (options) {
			    $(".datepicker.dropdown-menu").remove();
			},
			
			setDateFromTest: function (date) {
			    teamSchedule.SelectedDate(moment(date));
			}
			
		};
	});

