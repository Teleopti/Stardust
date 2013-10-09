
define([
		'knockout',
		'jquery',
		'navigation',
		'swipeListener',
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
		swipeListener,
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
				function (schedules) {
					var currentPersons = teamSchedule.Persons();
					
				    var dateClone = teamSchedule.SelectedDate().clone();
					
				    for (var i = 0; i < currentPersons.length; i++) {
					    currentPersons[i].ClearData();

				    	for (var j = 0; j < schedules.length; j++) {
				    		if (currentPersons[i].Id == schedules[j].Id) {
				    			currentPersons[i].AddData(schedules[j], teamSchedule.TimeLine, dateClone);
							}
						}
					}

				    currentPersons.sort(function (first, second) {
				    	return first.CompareTo(second);
					});

					teamSchedule.Persons.valueHasMutated();

					options.success();
				    
					resize.notify();
				},
			    function (notification) {
				    for (var i = 0; i < teamSchedule.Persons().length; i++) {
					    if (notification.DomainReferenceId == teamSchedule.Persons()[i].Id) {
						    return true;
					    }
				    }
				    return false;
			    }
		    );
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
			subscriptions.subscribeDailyStaffingMetrics(
				helpers.Date.ToServer(teamSchedule.SelectedDate()),
				teamSchedule.SelectedSkill().Id,
				function (data) {
					teamSchedule.SetDailyMetrics(data);
					options.success();
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
				
				var previousOffset;
				var teamScheduleContainer = $('.team-schedule');
				teamScheduleContainer.swipeListener({
					swipeLeft: function () {
						teamSchedule.NextDay();
					},
					swipeRight: function () {
						teamSchedule.PreviousDay();
					},
					swipeEnd: function () {
						teamScheduleContainer.offset({ left: previousOffset });
					},
					swipeStart: function () {
						previousOffset = teamScheduleContainer.offset().left;
					},
					swipeMove: function (movementX, movementY) {
						teamScheduleContainer.offset({ left: -movementX });
					}
				});
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
				subscriptions.unsubscribeTeamSchedule();
				subscriptions.unsubscribeDailyStaffingMetrics();
			    $(".datepicker.dropdown-menu").remove();
			},
			
			setDateFromTest: function (date) {
			    teamSchedule.SelectedDate(moment(date));
			}
			
		};
	});

