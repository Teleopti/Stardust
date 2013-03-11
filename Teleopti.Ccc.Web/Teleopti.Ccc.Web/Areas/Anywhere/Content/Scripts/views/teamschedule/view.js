
define([
		'knockout',
		'jquery',
		'navigation',
		'swipeListener',
		'moment',
		'subscriptions',
		'helpers',
		'views/teamschedule/vm',
		'views/teamschedule/timeline',
		'views/teamschedule/agent',
		'views/teamschedule/agents',
		'text!templates/teamschedule/view.html',
		'noext!application/resources'
	], function (
		ko,
		$,
		navigation,
		swipeListener,
		momentX,
		subscriptions,
		helpers,
		teamScheduleViewModel,
		timeLineViewModel,
		agentViewModel,
		agentsViewModel,
		view,
		resources
	) {

		var agents;
		var timeLine;
		var teamSchedule;

		var events = new ko.subscribable();

		events.subscribe(function (agentId) {
			navigation.GotoPersonSchedule(agentId, teamSchedule.SelectedDate());
		}, null, "gotoagent");


		return {
		    display: function (options) {
		        
				options.renderHtml(view);

				date = options.date;
				if (date == undefined) {
					date = moment().sod();
				} else {
					date = moment(date, 'YYYYMMDD');
				}

				agents = new agentsViewModel();
				timeLine = new timeLineViewModel(agents.Agents, resources.ShortTimePattern);
				teamSchedule = new teamScheduleViewModel(date);

				var previousOffset;
				var teamScheduleContainer = $('.team-schedule');

				var resize = function () {
					timeLine.WidthPixels($('.shift').width());
				};

				$(window)
					.resize(resize)
					.bind('orientationchange', resize)
					.ready(resize);

				var loadSchedules = function () {
					
					teamSchedule.isLoading(true);

					subscriptions.subscribeTeamSchedule(
						teamSchedule.SelectedTeam(),
						helpers.Date.AsUTCDate(teamSchedule.SelectedDate().toDate()),
						function (schedules) {
							var currentAgents = agents.Agents();

							var dateClone = teamSchedule.SelectedDate().clone();
							for (var i = 0; i < schedules.length; i++) {
								for (var j = 0; j < currentAgents.length; j++) {
									if (currentAgents[j].Id == schedules[i].Id) {
										currentAgents[j].SetLayers(schedules[i].Projection, timeLine, dateClone);
										currentAgents[j].AddContractTime(schedules[i].ContractTimeMinutes);
										currentAgents[j].AddWorkTime(schedules[i].WorkTimeMinutes);
										break;
									}
								}
							}

							currentAgents.sort(function (a, b) {
								var firstStartMinutes = a.TimeLineAffectingStartMinute();
								var secondStartMinutes = b.TimeLineAffectingStartMinute();
								return firstStartMinutes == secondStartMinutes ? (a.LastEndMinute() == b.LastEndMinute() ? 0 : a.LastEndMinute() < b.LastEndMinute() ? -1 : 1) : firstStartMinutes < secondStartMinutes ? -1 : 1;
							});

							agents.Agents.valueHasMutated();

							teamSchedule.isLoading(false);

							resize();
						});

				};

				var loadAvailableTeams = function () {
				    
				    $.ajax({
				        url: 'Person/AvailableTeams',
				        cache: false,
				        dataType: 'json',
				        data: {
				            date: teamSchedule.SelectedDate().toDate().toJSON()
				        },
				        success: function (data, textStatus, jqXHR) {
				            teamSchedule.Teams(data.Teams);
				        }
				    });

				};

				var loadPeople = function () {
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
				            agents.SetAgents(newItems);

				            loadSchedules();
				        }
				    });
				};

				teamSchedule.SelectedTeam.subscribe(function () {
			        loadPeople();
			        //navigation.GoToTeamSchedule(teamSchedule.SelectedTeam(), teamSchedule.SelectedDate());
			    });

			    teamSchedule.SelectedDate.subscribe(function() {
			        loadAvailableTeams();
			        navigation.GoToTeamSchedule(teamSchedule.SelectedTeam(), teamSchedule.SelectedDate());
			    });
			    
			    $(window).ready(function () {

				    ko.applyBindings({
						TeamSchedule: teamSchedule,
						Resources: resources,
						Timeline: timeLine,
						Agents: agents
					}, options.bindingElement);

				    loadAvailableTeams();
				});

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
			}
		};
	});

