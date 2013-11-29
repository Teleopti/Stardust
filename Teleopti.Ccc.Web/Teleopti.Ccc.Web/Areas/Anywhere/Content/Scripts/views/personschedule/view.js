
define([
		'knockout',
		'views/personschedule/vm',
		'subscriptions',
		'helpers',
		'text!templates/personschedule/view.html',
		'resizeevent',
		'views/personschedule/person',
		'ajax',
		'navigation',
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
		navigation,
		lazy
	) {

	var viewModel;

	var loadSchedules = function (options) {
		var date = moment(options.date, "YYYYMMDD");
		subscriptions.subscribeTeamSchedule(
			options.groupid,
			helpers.Date.ToServer(date),
			function (schedules) {
				var currentPersons = viewModel.PersonsInGroup();

				for (var i = 0; i < currentPersons.length; i++) {
					if (currentPersons[i].Id != options.personid) {
						currentPersons[i].ClearData();

						for (var j = 0; j < schedules.length; j++) {
							if (currentPersons[i].Id == schedules[j].PersonId) {
								schedules[j].Date = date;
								currentPersons[i].AddData(schedules[j], viewModel.TimeLine);
							}
						}
					}
				}

				currentPersons.sort(function (first, second) {
					first = first.OrderBy();
					second = second.OrderBy();
					return first == second ? 0 : (first < second ? -1 : 1);
				});

				viewModel.PersonsInGroup.valueHasMutated();

				options.success();

				resize.notify();
			},
			function (notification) {
				for (var i = 0; i < viewModel.PersonsInGroup().length; i++) {
					if (notification.DomainReferenceId == viewModel.PersonsInGroup()[i].Id) {
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
			success: function (people, textStatus, jqXHR) {

				var newPeople = lazy(people)
						.filter(function (x) {
							return options.personid != x.Id;
						});

				var newItems = ko.utils.arrayMap(newPeople.toArray(), function (s) {
					return new personViewModel(s);
				});

				viewModel.AddPersonsToGroup(newItems);
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

			viewModel.Id(options.personid != undefined ? options.personid : options.id);
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
					groupid: options.groupid,
					date: options.date,
					personid: options.personid,
					success: function () {
						loadSchedules({
							groupid: options.groupid,
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
				    viewModel.Id(),
				    helpers.Date.ToServer(viewModel.Date()),
				    function (data) {
				    	resize.notify();

				    	data.Id = viewModel.Id();
				    	data.Date = viewModel.Date();

				    	viewModel.PersonsInGroup([]);

				    	var person = new personViewModel(data);
				    	person.AddData(data, viewModel.TimeLine);
				    	viewModel.AddPersonsToGroup([person]);
				    	viewModel.SetData(data, options.groupid);

				    	if (viewModel.AddingActivity()) {
				    		loadPersonsAndSchedules();
				    	} else {
				    		viewModel.Loading(false);
				    		deferred.resolve();
				    	}
				    }
			    );

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

