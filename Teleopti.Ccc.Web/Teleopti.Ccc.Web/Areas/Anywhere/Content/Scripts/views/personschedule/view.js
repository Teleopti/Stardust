
define([
		'knockout',
		'views/personschedule/vm',
		'subscriptions',
		'helpers',
		'text!templates/personschedule/view.html',
		'resizeevent',
		'views/personschedule/person',
		'ajax',
		'navigation'
], function (
		ko,
		personScheduleViewModel,
		subscriptions,
		helpers,
		view,
	    resize,
		personViewModel,
		ajax,
		navigation
	) {

	var personSchedule;

	var loadSchedules = function (options) {
		var date = moment(options.date, "YYYYMMDD");
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
							currentPersons[i].AddData(schedules[j], personSchedule.TimeLine); //personSchedule.SelectedGroup());
						}
					}
				}

				currentPersons.sort(function (first, second) {
					first = first.OrderBy();
					second = second.OrderBy();
					return first == second ? 0 : (first < second ? -1 : 1);
				});

				personSchedule.PersonsInGroup.valueHasMutated();

				options.success();

				resize.notify();
			},
			function (notification) {
				for (var i = 0; i < personSchedule.PersonsInGroup().length; i++) {
					if (notification.DomainReferenceId == personSchedule.PersonsInGroup()[i].Id) {
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
				var newItems = ko.utils.arrayMap(people, function (s) {
					return new personViewModel(s);
				});
				personSchedule.SetPersonsInGroup(newItems);
				options.success();
			}
		});
	};

	return {
		initialize: function (options) {

			options.renderHtml(view);

			personSchedule = new personScheduleViewModel();

			resize.onresize(function () {
				personSchedule.TimeLine.WidthPixels($('.time-line-for').width());

			});
			
			personSchedule.SelectedGroup.subscribe(function () {
				if (personSchedule.Loading())
					return;
				if (!options.action)
					navigation.GotoPersonSchedule(personSchedule.SelectedGroup(), options.personid, options.date);
				else
					navigation.GotoPersonScheduleWithAction(personSchedule.SelectedGroup(), options.personid, options.date, options.action);
			});

			ko.applyBindings(personSchedule, options.bindingElement);

		},

		display: function (options) {
			var date = moment(options.date, 'YYYYMMDD');

			personSchedule.Loading(true);
			personSchedule.Id(options.personid != undefined ? options.personid : options.id);
			personSchedule.Date(date);

			var deferred = $.Deferred();
			var loadPersonsAndSchedules = function () {
				var currentGroup = options.groupid;
				if (!currentGroup) {
					personSchedule.Loading(false);
					deferred.resolve();
					return;
				}

				loadPersons({
					groupid: options.groupid,
					date: options.date,
					success: function () {
						loadSchedules({
							groupid: options.groupid,
							date: options.date,
							success: function () {
								personSchedule.Loading(false);
								deferred.resolve();
							}
						});
					}
				});
			};

			loadPersonsAndSchedules();

			subscriptions.subscribePersonSchedule(
				    options.personid,
				    helpers.Date.ToServer(personSchedule.Date()),
				    function (data) {
				    	resize.notify();
				    	personSchedule.ClearData();
				    	personSchedule.SetData(data, options.groupid);
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

