
define([
		'knockout',
		'views/personschedule/vm',
		'subscriptions',
		'helpers',
		'text!templates/personschedule/view.html',
		'resizeevent',
		'views/personschedule/person',
		'ajax',
		'lazy'
], function (
		ko,
		personScheduleViewModel,
		subscriptions,
		helpers,
		view,
		resize,
		personViewModel,
		ajax,
		lazy
	) {

	var viewModel;

	var loadSchedules = function (options) {
		var date = moment(options.date, "YYYYMMDD");
		subscriptions.subscribeTeamSchedule(
			options.groupid,
			helpers.Date.ToServer(date),
			function (schedules) {
				var currentPersons = viewModel.Persons();

				for (var i = 0; i < currentPersons.length; i++) {
					currentPersons[i].ClearData();

					for (var j = 0; j < schedules.length; j++) {
						if (currentPersons[i].Id == schedules[j].PersonId) {
							schedules[j].Date = date;
							currentPersons[i].AddData(schedules[j], viewModel.TimeLine);
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
			},
			function (notification) {
				var persons = viewModel.Persons();
				for (var i = 0; i < persons.length; i++) {
					if (notification.DomainReferenceId == persons[i].Id) {
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
				date: helpers.Date.ToServer(moment(options.date, "YYYYMMDD")),
				groupId: options.groupid
			},
			success: function (data, textStatus, jqXHR) {
				if (!viewModel.DisplayGroupMates()) {
					data = [lazy(data)
						.select(function(x) { return x.Id == viewModel.PersonId(); }).first()];
				}
				viewModel.AddPersons(data);
				options.success();
			}
		});
	};

	return {
		initialize: function (options) {

			options.renderHtml(view);

			viewModel = new personScheduleViewModel();

			resize.onresize(function () {
				viewModel.TimeLine.WidthPixels($('.time-line-for').width());

			});

			ko.applyBindings(viewModel, options.bindingElement);
		},

		display: function (options) {
			var date = moment(options.date, 'YYYYMMDD');

			viewModel.Loading(true);

			viewModel.PersonId(options.personid != undefined ? options.personid : options.id);
			viewModel.GroupId(options.groupid);
			viewModel.Date(date);

			var deferred = $.Deferred();

			var loadPersonsAndSchedules = function () {
				var currentGroup = options.groupid;
				if (!currentGroup) {
					viewModel.Loading(false);
					deferred.resolve();
					return;
				}

				loadPersons({
					groupid: currentGroup,
					date: options.date,
					personid: options.personid,
					success: function () {
						loadSchedules({
							groupid: currentGroup,
							date: options.date,
							personid: options.personid,
							success: function () {
								viewModel.Loading(false);
								deferred.resolve();
							}
						});
					}
				});
			};

			subscriptions.subscribePersonSchedule(
				viewModel.PersonId(),
				helpers.Date.ToServer(viewModel.Date()),
				function (data) {
					viewModel.SetData(data, viewModel.TimeLine);
					loadPersonsAndSchedules();
				}
			);

			//subscriptions.subscribeGroupSchedules(
			//	viewModel.GroupId(),
			//	helpers.Date.ToServer(viewModel.Date()),
			//	function (data) {
			//		viewModel.SetSchedules(data);
			//	});

			return deferred.promise();
		},

		dispose: function (options) {
			subscriptions.unsubscribePersonSchedule();
			$(".datepicker.dropdown-menu").remove();
		},

		clearaction: function (options) {
			viewModel.AddingFullDayAbsence(false);
			viewModel.AddingActivity(false);
			viewModel.AddingIntradayAbsence(false);
		},

		addfulldayabsence: function (options) {
			viewModel.AddingFullDayAbsence(true);
		},

		addactivity: function (options) {
			viewModel.AddingActivity(true);
		},

		addintradayabsence: function (options) {
			viewModel.AddingIntradayAbsence(true);
		},

		setDateFromTest: function (date) {
			viewModel.AddFullDayAbsenceForm.EndDate(moment(date));
		}
	};
});

