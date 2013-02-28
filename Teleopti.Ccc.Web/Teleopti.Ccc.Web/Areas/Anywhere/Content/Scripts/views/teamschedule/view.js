
define([
		'knockout',
		'jquery',
		'navigation',
		'swipeListener',
		'moment',
		'subscriptions',
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
			navigation.GotoPersonSchedule(agentId, teamSchedule.SelectedDate().format('YYYYMMDD'));
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

				var initialLoad = true;
				var loadSchedules = function () {
					var queryDate = teamSchedule.SelectedDate().clone();
					queryDate.utc();

					teamSchedule.isLoading(true);

					subscriptions.subscribeTeamSchedule(
						teamSchedule.SelectedTeam().Id,
						queryDate.toDate(),
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

							if (initialLoad) {
								teamSchedule.SelectedTeam.subscribe(function () {
									loadPeople();
								});

								teamSchedule.SelectedDate.subscribe(function () {
									loadAvailableTeams();
								});
								initialLoad = false;
							}

							resize();
						});

				};

				var arrayIndexOf = function (a, fnc, b) {
					if (!fnc || typeof (fnc) != 'function') {
						return -1;
					}
					if (!a || !a.length || a.length < 1) return -1;
					for (var i = 0; i < a.length; i++) {
						if (fnc(a[i], b)) return i;
					}
					return -1;
				};

				var arrayExcept = function (sourceArray, exceptArray, predicate) {
					for (var i = 0; i < exceptArray.length; i++) {
						var index = arrayIndexOf(sourceArray, predicate, exceptArray[i]);
						if (index > -1) {
							sourceArray.splice(index, 1);
						}
					}
				};

				var loadAvailableTeams = function () {
					$.getJSON('Person/AvailableTeams?' + $.now(), { date: teamSchedule.SelectedDate().toDate().toJSON() }).success(function (details, textStatus, jqXHR) {
						var teams = teamSchedule.Teams();
						var teamsToRemove = teams.slice(0);
						var teamsToAdd = details.Teams;

						var teamEqualityComparer = function (a, b) {
							return a.Id == b.Id;
						};

						arrayExcept(teamsToRemove, teamsToAdd, teamEqualityComparer);
						arrayExcept(teamsToAdd, teams, teamEqualityComparer);
						arrayExcept(teams, teamsToRemove, teamEqualityComparer);

						$.merge(teams, teamsToAdd);

						teamSchedule.Teams.valueHasMutated();

						loadPeople();
					}).error(function () {
						window.location.href = 'authentication/signout';
					});
				};

				var loadPeople = function () {
					$.ajax({
						url: 'Person/PeopleInTeam',
						cache: false,
						dataType: 'json',
						data: {
							date: teamSchedule.SelectedDate().toDate().toJSON(),
							teamId: teamSchedule.SelectedTeam().Id
						},
						success: function (people, textStatus, jqXHR) {

							var newItems = ko.utils.arrayMap(people, function (s) {
								return new agentViewModel(s, events);
							});
							agents.SetAgents(newItems);

							loadSchedules();
						}
					}
					);
				};

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

