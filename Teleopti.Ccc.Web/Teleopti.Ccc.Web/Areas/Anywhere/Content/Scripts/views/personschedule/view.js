
define([
		'knockout',
		'views/personschedule/vm',
		'subscriptions',
		'helpers',
		'text!templates/personschedule/view.html',
		'resizeevent',
		'views/personschedule/person',
		'ajax'
], function (
		ko,
		personScheduleViewModel,
		subscriptions,
		helpers,
		view,
	    resize,
		personViewModel,
		ajax
	) {

	var personSchedule;

	return {
		initialize: function (options) {

			options.renderHtml(view);

			personSchedule = new personScheduleViewModel();

			resize.onresize(function () {
				personSchedule.TimeLine.WidthPixels($('.time-line-for').width());

			});

			ko.applyBindings(personSchedule, options.bindingElement);

		},

		display: function (options) {

			var deferred = $.Deferred();

			var date = moment(options.date, 'YYYYMMDD');

			if (personSchedule.Id() == options.personid && personSchedule.Date().diff(date) == 0)
				return;

			personSchedule.Loading(true);

			personSchedule.Id(options.personid);
			personSchedule.Date(date);

			ajax.ajax({
				url: 'Person/PeopleInGroup',
				data: {
					date: helpers.Date.ToServer(date),
					groupId: options.groupid
				},
				success: function(people, textStatus, jqXHR) {
					var newItems = ko.utils.arrayMap(people, function(s) {
						return new personViewModel(s);
					});
					personSchedule.SetPersonsInGroup(newItems);

					subscriptions.subscribeTeamSchedule(
						options.groupid,
						helpers.Date.ToServer(date),						
						function (schedules) {
							var currentPersons = personSchedule.PersonsInGroup();

							for (var i = 0; i < currentPersons.length; i++) {
								currentPersons[i].ClearData();

								for (var j = 0; j < schedules.length; j++) {
									if (currentPersons[i].Id == schedules[j].PersonId) {
										schedules[j].Date = date;
										currentPersons[i].AddData(schedules[j], personSchedule.TimeLine);
									}
								}
							}

							currentPersons.sort(function(first, second) {
								first = first.OrderBy();
								second = second.OrderBy();
								return first == second ? 0 : (first < second ? -1 : 1);
							});

							personSchedule.PersonsInGroup.valueHasMutated();

							resize.notify();
						},
						function(notification) {
							for (var i = 0; i < personSchedule.PersonsInGroup().length; i++) {
								if (notification.DomainReferenceId == personSchedule.PersonsInGroup()[i].Id) {
									return true;
								}
							}
							return false;
						}
					);
				}
			});

			subscriptions.subscribePersonSchedule(
				    options.personid,
				    helpers.Date.ToServer(personSchedule.Date()),
				    function (data) {
				    	resize.notify();
				    	personSchedule.SetData(data);
				    	personSchedule.Loading(false);
				    	deferred.resolve();
				    }
			    );

			return deferred.promise();
		},

		dispose: function (options) {
			subscriptions.unsubscribePersonSchedule();
			$(".datepicker.dropdown-menu").remove();
		},

		clearaction: function (options) {
			personSchedule.AddingFullDayAbsence(false);
			personSchedule.AddingActivity(false);
			personSchedule.AddingAbsence(false);
		},

		addfulldayabsence: function (options) {
			personSchedule.AddingFullDayAbsence(true);
		},

		addactivity: function (options) {
			personSchedule.AddingActivity(true);
		},
		
		addabsence: function (options) {
			personSchedule.AddingAbsence(true);
		},

		setDateFromTest: function (date) {
			personSchedule.AddFullDayAbsenceForm.EndDate(moment(date));
		}
	};
});

