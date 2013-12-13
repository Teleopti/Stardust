
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
		'shared/current-state',
		'ajax',
		'lazy'
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
		currentState,
		ajax,
		lazy
	) {

	var viewModel;

	var setSelectedLayer = function (shifts) {
		var layers = lazy(shifts).map(function(x) { return x.Layers(); }).flatten();
		var selectedStart = currentState.SelectedLayer().StartMinutes();
		var selectedLayer = lazy(layers).select(function(x) { return x.StartMinutes() == selectedStart; }).first();
		if (!selectedLayer) {
			selectedStart += 1440;
			selectedLayer = lazy(layers).select(function(x) { return x.StartMinutes() == selectedStart; }).first();
		}
		if (selectedLayer)
			selectedLayer.Selected(true);
	};

	var loadSchedules = function (options) {
		subscriptions.subscribeGroupSchedule(
			viewModel.SelectedGroup(),
			helpers.Date.ToServer(viewModel.SelectedDate()),
			function (schedules) {
				var currentPersons = viewModel.Persons();

				var dateClone = viewModel.SelectedDate().clone();

				for (var i = 0; i < currentPersons.length; i++) {
					currentPersons[i].ClearData();

					for (var j = 0; j < schedules.length; j++) {
						if (currentPersons[i].Id == schedules[j].PersonId) {
							schedules[j].Date = dateClone;
							currentPersons[i].AddData(schedules[j], viewModel.TimeLine, viewModel.SelectedGroup());
						}
					}
				}

				currentPersons.sort(function (first, second) {
					first = first.OrderBy();
					second = second.OrderBy();
					return first == second ? 0 : (first < second ? -1 : 1);
				});

				viewModel.Persons.valueHasMutated();

				options.success();
				
				resize.notify();

				for (var k = 0; k < currentPersons.length; k++) {
					if (currentState.SelectedPersonId() == currentPersons[k].Id) {
						if (currentState.SelectedLayer()) {
							setSelectedLayer(currentPersons[k].Shifts());
						}
					}
				}
			},
			function (notification) {
				for (var i = 0; i < viewModel.Persons().length; i++) {
					if (notification.DomainReferenceId == viewModel.Persons()[i].Id) {
						return true;
					}
				}
				return false;
			}
		);
	};

	var loadPersons = function (options) {
		var groupid = viewModel.SelectedGroup();
		
		ajax.ajax({
			url: 'Person/PeopleInGroup',
			data: {
				date: helpers.Date.ToServer(viewModel.SelectedDate()),
				groupId: groupid
			},
			success: function (people, textStatus, jqXHR) {
				var newItems = ko.utils.arrayMap(people, function (s) {
					s["GroupId"] = groupid;
					s["Date"] = viewModel.SelectedDate().format("YYYYMMDD");
					return new personViewModel(s);
				});
				viewModel.SetPersons(newItems);
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
				date: viewModel.SelectedDate().toDate().toJSON()
			},
			success: function (data, textStatus, jqXHR) {
				viewModel.SetSkills(data.Skills);
				options.success();
			}
		});
	};

	var loadDailyStaffingMetrics = function (options) {
		if (viewModel.SelectedSkill() == null) return;
		subscriptions.subscribeDailyStaffingMetrics(
			helpers.Date.ToServer(viewModel.SelectedDate()),
			viewModel.SelectedSkill().Id,
			function (data) {
				viewModel.SetDailyMetrics(data);
				options.success();
			});
	};
	var loadGroupPages = function (options) {
		ajax.ajax({
			url: 'GroupPage/AvailableGroupPages',
			data: {
				date: helpers.Date.ToServer(viewModel.SelectedDate()),
			},
			success: function (data, textStatus, jqXHR) {
				viewModel.SetGroupPages(data);
				options.success();
			}
		});
	};

	return {
		initialize: function (options) {

			options.renderHtml(view);

			viewModel = new teamScheduleViewModel();

			resize.onresize(function () {
				viewModel.TimeLine.WidthPixels($('.time-line-for').width());
			});

			viewModel.SelectedGroup.subscribe(function () {
				if (viewModel.Loading())
					return;
				currentState.Clear();
				navigation.GoToTeamSchedule(viewModel.SelectedGroup(), viewModel.SelectedDate(), viewModel.SelectedSkill());
			});

			viewModel.SelectedDate.subscribe(function () {
				if (viewModel.Loading())
					return;
				currentState.Clear();
				navigation.GoToTeamSchedule(viewModel.SelectedGroup(), viewModel.SelectedDate(), viewModel.SelectedSkill());
			});

			viewModel.SelectedSkill.subscribe(function () {
				if (viewModel.Loading())
					return;
				navigation.GoToTeamSchedule(viewModel.SelectedGroup(), viewModel.SelectedDate(), viewModel.SelectedSkill());
			});

			ko.applyBindings(viewModel, options.bindingElement);
		},

		display: function (options) {

			var currentGroupId = function () {
				if (options.id)
					return options.id;
				if (viewModel.SelectedGroup())
					return viewModel.SelectedGroup();
				if (viewModel.GroupPages().length > 0 && viewModel.GroupPages()[0].Groups().length > 0)
					return viewModel.GroupPages()[0].Groups()[0].Id;
				return null;
			};

			var currentSkillId = function () {
				if (options.secondaryId)
					return options.secondaryId;
				var skills = viewModel.Skills();
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
			
			viewModel.Loading(true);

			viewModel.SelectedDate(currentDate());
			
			loadSkills({
				success: function () {
					viewModel.SelectSkillById(currentSkillId());
					viewModel.LoadingStaffingMetrics(true);
					loadDailyStaffingMetrics({
						success: function () {
							viewModel.LoadingStaffingMetrics(false);
						}
					});
				}
			});

			var deferred = $.Deferred();
			var loadPersonsAndSchedules = function () {
				var currentGroup = currentGroupId();
				if (!currentGroup) {
					viewModel.Loading(false);
					deferred.resolve();
					return;
				}

				viewModel.SelectedGroup(currentGroup);
				loadPersons({
					success: function () {
						loadSchedules({
							success: function () {
								viewModel.Loading(false);
								if (currentState.SelectedPersonId()) {
									$('html, body').animate({
										scrollTop: $("[data-person-id='" + currentState.SelectedPersonId() + "']").offset().top
									}, 20);
								}
								deferred.resolve();
							}
						});
					}
				});
			};

			if (viewModel.GroupPages().length != 0 && viewModel.GroupPages()[0].Groups().length != 0) {
				loadPersonsAndSchedules();
			} else {
				loadGroupPages({
					success: loadPersonsAndSchedules
				});
			}

			return deferred.promise();
		},

		dispose: function (options) {
			subscriptions.unsubscribeGroupSchedule();
			subscriptions.unsubscribeDailyStaffingMetrics();
			$(".datepicker.dropdown-menu").remove();
		},

		setDateFromTest: function (date) {
			viewModel.SelectedDate(moment(date));
		}

	};
});

