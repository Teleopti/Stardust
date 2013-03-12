
define([
		'knockout',
		'jquery',
		'navigation',
		'swipeListener',
		'moment',
		'subscriptions',
		'helpers',
		'views/teamschedule/vm',
		'views/teamschedule/agent',
		'text!templates/teamschedule/view.html'
	], function (
		ko,
		$,
		navigation,
		swipeListener,
		momentX,
		subscriptions,
		helpers,
		teamScheduleViewModel,
		agentViewModel,
		view
	) {

		var teamSchedule;

		var events = new ko.subscribable();

		events.subscribe(function (agentId) {
			navigation.GotoPersonSchedule(agentId, teamSchedule.SelectedDate());
		}, null, "gotoagent");

		var loadSchedules = function(options) {
			subscriptions.subscribeTeamSchedule(
				teamSchedule.SelectedTeam(),
				helpers.Date.AsUTCDate(teamSchedule.SelectedDate().toDate()),
				function(schedules) {
					var currentAgents = teamSchedule.Agents();

					var dateClone = teamSchedule.SelectedDate().clone();
					for (var i = 0; i < schedules.length; i++) {
						for (var j = 0; j < currentAgents.length; j++) {
							if (currentAgents[j].Id == schedules[i].Id) {
								currentAgents[j].SetLayers(schedules[i].Projection, teamSchedule.TimeLine, dateClone);
								currentAgents[j].AddContractTime(schedules[i].ContractTimeMinutes);
								currentAgents[j].AddWorkTime(schedules[i].WorkTimeMinutes);
								break;
							}
						}
					}

					currentAgents.sort(function(a, b) {
						var firstStartMinutes = a.TimeLineAffectingStartMinute();
						var secondStartMinutes = b.TimeLineAffectingStartMinute();
						return firstStartMinutes == secondStartMinutes ? (a.LastEndMinute() == b.LastEndMinute() ? 0 : a.LastEndMinute() < b.LastEndMinute() ? -1 : 1) : firstStartMinutes < secondStartMinutes ? -1 : 1;
					});

					teamSchedule.Agents.valueHasMutated();

					options.success();
				});
		};

		var loadPersons = function (options) {
			$.ajax({
				url: 'Person/PeopleInTeam',
				cache: false,
				dataType: 'json',
				data: {
					date: teamSchedule.SelectedDate().toDate().toJSON(),
					teamId: teamSchedule.SelectedTeam()
				},
				success: function (people, textStatus, jqXHR) {
					var newItems = ko.utils.arrayMap(people, function (s) {
						return new agentViewModel(s, events);
					});
					teamSchedule.SetAgents(newItems);
					options.success();
				}
			});
		};

		var loadTeams = function (options) {
			$.ajax({
				url: 'Person/AvailableTeams',
				cache: false,
				dataType: 'json',
				data: {
					date: teamSchedule.SelectedDate().toDate().toJSON()
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

				var resize = function () {
					teamSchedule.TimeLine.WidthPixels($('.shift').width());
				};

				$(window)
					.resize(resize)
					.bind('orientationchange', resize)
					.ready(resize);

				teamSchedule.SelectedTeam.subscribe(function () {
					if (teamSchedule.Loading())
						return;
					navigation.GoToTeamSchedule(teamSchedule.SelectedTeam(), teamSchedule.SelectedDate());
				});

				teamSchedule.SelectedDate.subscribe(function() {
					if (teamSchedule.Loading())
						return;
					navigation.GoToTeamSchedule(teamSchedule.SelectedTeam(), teamSchedule.SelectedDate());
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

				var currentDate = function () {
					var date = options.date;
					if (date == undefined) {
						return moment().sod();
					} else {
						return moment(date, 'YYYYMMDD');
					}
				};

				teamSchedule.Loading(true);

				teamSchedule.SelectedDate(currentDate());

				var deferred = $.Deferred();
			    
				var loadPersonsAndSchedules = function() {
					teamSchedule.SelectedTeam(currentTeamId());
					loadPersons({
						success: function() {
							loadSchedules({
								success: function() {
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
			
			setDateFromTest: function (date) {
			    teamSchedule.SelectedDate(moment(date));
			}
			
		};
	});

