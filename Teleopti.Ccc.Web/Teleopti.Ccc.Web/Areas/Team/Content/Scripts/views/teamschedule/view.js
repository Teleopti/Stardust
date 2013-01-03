
define([
		'knockout',
		'jquery',
		'navigation',
		'signalrHubs',
		'swipeListener',
		'moment',
		'views/teamschedule/vm',
		'views/teamschedule/timeline',
		'views/teamschedule/agent',
		'text!templates/teamschedule/view.html'
	], function (
		ko,
		$,
		swipeListener,
		momentX,
		navigation,
		signalrHubs,
		teamScheduleViewModel,
		timeLineViewModel,
		agentViewModel,
		view
	) {
		return {
			display: function (options) {

				options.renderHtml(view);

				var date = options.date;
				if (date == undefined) {
					date = moment().sod();
				} else {
					date = moment(date, 'YYYYMMDD');
				}

				var timeLine = new timeLineViewModel();
				var teamSchedule = new teamScheduleViewModel(timeLine, date);

				var previousOffset;
				var teamScheduleContainer = $('.team-schedule');

				var schedule = $.connection.scheduleHub;

				var resize = function () {
					timeLine.WidthPixels($('.shift').width());
				};

				$(window)
					.resize(resize)
					.bind('orientationchange', resize)
					.ready(resize);

				var utcFromMoment = function (momentDate) {
					return new Date(Date.UTC(momentDate.year(), momentDate.month(), momentDate.date()));
				};

				var loadSchedules = function () {
					var queryDate = teamSchedule.SelectedDate();
					queryDate.utc();

					teamSchedule.isLoading(true);
					schedule.server.subscribeTeamSchedule(teamSchedule.SelectedTeam().Id, queryDate.toDate()).done(function (schedules) {
						var agents = teamSchedule.Agents();
						for (var i = 0; i < schedules.length; i++) {
							for (var j = 0; j < agents.length; j++) {
								if (agents[j].Id == schedules[i].Id) {
									agents[j].AddLayers(schedules[i].Projection);
									break;
								}
							}
						}

						teamSchedule.Agents.sort(function (a, b) {
							var firstStartMinutes = a.FirstStartMinute();
							var secondStartMinutes = b.FirstStartMinute();
							return firstStartMinutes == secondStartMinutes ? (a.LastEndMinute() == b.LastEndMinute() ? 0 : a.LastEndMinute() < b.LastEndMinute() ? -1 : 1) : firstStartMinutes < secondStartMinutes ? -1 : 1;
						});

						teamSchedule.isLoading(false);
						resize();
					});
				};

				var loadAvailableTeams = function () {
					$.getJSON('../Person/AvailableTeams?' + Math.round(new Date().getTime()), { date: teamSchedule.SelectedDate().toDate().toJSON() }).success(function (details, textStatus, jqXHR) {
						var selectedTeamBefore = teamSchedule.SelectedTeam();

						teamSchedule.AddTeams(details.Teams);

						var teams = teamSchedule.Teams();
						var teamToSelect = teams[0];
						if (selectedTeamBefore) {
							for (var i = 0; i < teams.length; i++) {
								if (selectedTeamBefore.Id == teams[i].Id) {
									teamToSelect = teams[i];
									break;
								}
							}
						}
						teamSchedule.SelectedTeam(teamToSelect);
					});
				};

				var loadPeople = function () {
					$.getJSON('../Person/PeopleInTeam?' + Math.round(new Date().getTime()), { date: teamSchedule.SelectedDate().toDate().toJSON(), teamId: teamSchedule.SelectedTeam().Id }).success(function (people, textStatus, jqXHR) {
						timeLine.Agents.removeAll();
						teamSchedule.Agents.removeAll();

						var newItems = ko.utils.arrayMap(people, function (s) {
							return new agentViewModel(timeLine, s);
						});
						teamSchedule.AddAgents(newItems);

						loadSchedules();
					});
				};

				teamSchedule.SelectedTeam.subscribe(function () {
					loadPeople();
				});

				teamSchedule.SelectedDate.subscribe(function () {
					loadAvailableTeams();
				});

				$.connection.hub.url = 'signalr';
				$.connection.hub.start()
					.done(function () {
						loadAvailableTeams();

						ko.applyBindings({
							TeamSchedule: teamSchedule
						});
					})
					.fail(function (error) {
						$('.container > .row:first').html('<div class="alert"><button type="button" class="close" data-dismiss="alert">&times;</button><strong>Warning!</strong> ' + error + '.</div>');
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

