
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
        'ajax',
        'pagelog'
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
	    ajax,
	    pagelog
	) {

		var teamSchedule;

		var events = new ko.subscribable();

		events.subscribe(function (personId) {
		    navigation.GotoPersonSchedule(personId, teamSchedule.SelectedDate());
		}, null, "gotoperson");

		var loadSchedules = function(options) {
		    pagelog.log("loadSchedules");
		    subscriptions.subscribeTeamSchedule(
				teamSchedule.SelectedTeam(),
				helpers.Date.ToServer(teamSchedule.SelectedDate()),
				function(schedules) {
				    pagelog.log("loadSchedules.got");
				    var currentPersons = teamSchedule.Persons();

					var dateClone = teamSchedule.SelectedDate().clone();
					for (var i = 0; i < schedules.length; i++) {
						for (var j = 0; j < currentPersons.length; j++) {
						    if (currentPersons[j].Id == schedules[i].Id) {
						        currentPersons[j].SetLayers(schedules[i].Projection, teamSchedule.TimeLine, dateClone);
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
					pagelog.log("loadSchedules./got");
				});
		};

		var loadPersons = function (options) {
		    pagelog.log("loadPersons");
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

		var loadTeams = function (options) {
		    pagelog.log("loadTeams");
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

			    pagelog.log("initialize");
			    
				options.renderHtml(view);

				teamSchedule = new teamScheduleViewModel();

				resize.onresize(function () {
				    teamSchedule.TimeLine.WidthPixels($('.shift').width());
				});

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

			    pagelog.log("display");

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
		                return moment().startOf('day');
					} else {
						return moment(date, 'YYYYMMDD');
					}
				};

				teamSchedule.Loading(true);

				teamSchedule.SelectedDate(currentDate());

				var deferred = $.Deferred();
			    
				var loadPersonsAndSchedules = function() {
				    pagelog.log("loadPersonsAndSchedules");
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
				
				pagelog.log("load stuff");
			    
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

