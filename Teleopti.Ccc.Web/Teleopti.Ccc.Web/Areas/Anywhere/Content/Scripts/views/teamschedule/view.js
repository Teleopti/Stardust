
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
        'resizeevent'
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
	    resize
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
						return new personViewModel(s, events);
					});
					teamSchedule.SetPersons(newItems);
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
			
			dispose: function (options) {
			    $(".datepicker.dropdown-menu").remove();
			},
			
			setDateFromTest: function (date) {
			    teamSchedule.SelectedDate(moment(date));
			}
			
		};
	});

