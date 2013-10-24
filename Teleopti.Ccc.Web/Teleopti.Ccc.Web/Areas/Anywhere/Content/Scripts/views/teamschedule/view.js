
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
				teamSchedule.SelectedGroup(),
				helpers.Date.ToServer(teamSchedule.SelectedDate()),
				function (schedules) {
					var currentPersons = teamSchedule.Persons();
					
					var dateClone = teamSchedule.SelectedDate().clone();
					
					for (var i = 0; i < currentPersons.length; i++) {
						currentPersons[i].ClearData();

						for (var j = 0; j < schedules.length; j++) {
							if (currentPersons[i].Id == schedules[j].PersonId) {
								currentPersons[i].AddData(schedules[j], teamSchedule.TimeLine, dateClone);
							}
						}
					}

					currentPersons.sort(function (first, second) {
						first = first.OrderBy();
						second = second.OrderBy();
						return first == second ? 0 : (first < second ? -1 : 1);
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
				url: 'Person/PeopleInGroup',
				data: {
					date: helpers.Date.ToServer(teamSchedule.SelectedDate()),
					groupId: teamSchedule.SelectedGroup()
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
		var loadGroupPages = function (options) {
			ajax.ajax({
				url: 'GroupPage/AvailableGroupPages',
				data: {
					date: helpers.Date.ToServer(teamSchedule.SelectedDate()),
				},
				success: function (data, textStatus, jqXHR) {
					teamSchedule.SetGroupPages(data);
					options.success();
				}
			});
		};

		return {
			initialize: function (options) {

				options.renderHtml(view);

				teamSchedule = new teamScheduleViewModel();

				resize.onresize(function () {
					teamSchedule.TimeLine.WidthPixels($('.time-line-for').width());
				});

				teamSchedule.SelectedGroup.subscribe(function () {
					if (teamSchedule.Loading())
						return;
					navigation.GoToTeamSchedule(teamSchedule.SelectedGroup(), teamSchedule.SelectedDate(), teamSchedule.SelectedSkill());
				});

				teamSchedule.SelectedDate.subscribe(function() {
					if (teamSchedule.Loading())
						return;
					navigation.GoToTeamSchedule(teamSchedule.SelectedGroup(), teamSchedule.SelectedDate(), teamSchedule.SelectedSkill());
				});

				teamSchedule.SelectedSkill.subscribe(function () {
					if (teamSchedule.Loading())
						return;
					navigation.GoToTeamSchedule(teamSchedule.SelectedGroup(), teamSchedule.SelectedDate(), teamSchedule.SelectedSkill());
				});
				
				ko.applyBindings(teamSchedule, options.bindingElement);
			},
			
			display: function (options) {

				var currentGroupId = function () {
					if (options.id)
						return options.id;
					
					if (!teamSchedule.SelectedGroup()) {
						if (teamSchedule.GroupPages().length > 0 && teamSchedule.GroupPages()[0].Groups().length > 0)
							return teamSchedule.GroupPages()[0].Groups()[0].Id;
					}
					
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
					var currentGroup = currentGroupId();
					if (!currentGroup) {
						teamSchedule.Loading(false);
						deferred.resolve();
						return;
					}
					
					teamSchedule.SelectedGroup(currentGroup);
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

				if (teamSchedule.GroupPages().length != 0 && teamSchedule.GroupPages()[0].Groups().length != 0) {
					loadPersonsAndSchedules();
				} else {
					loadGroupPages({
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

