
define([
		'knockout',
		'jquery',
		'navigation',
		'moment',
		'subscriptions.groupschedule',
		'subscriptions.unsubscriber',
		'helpers',
		'views/teamschedule/vm',
		'views/teamschedule/person',
		'text!templates/teamschedule/view.html',
		'resizeevent',
		'shared/current-state',
		'ajax',
		'select2',
		'permissions',
		'resources'
], function (
		ko,
		$,
		navigation,
		momentX,
		groupschedulesubscriptions,
		unsubscriber,
		helpers,
		teamScheduleViewModel,
		personViewModel,
		view,
		resize,
		currentState,
		ajax,
		select2, // not a direct dependency, but still a view dependency
		permissions,
		resources
	) {

	var viewModel;

	var loadGroupPages = function (buid, date, callback) {
		ajax.ajax({
			url: 'api/GroupPage/AvailableGroupPages?date=' + date,
			headers: { 'X-Business-Unit-Filter': buid},
			success: callback
		});
	}

	return {
		initialize: function (options) {

			options.renderHtml(view);

			viewModel = new teamScheduleViewModel();

			resize.onresize(function () {
				var timeLineWidth;
				var timeLine = $('.time-line-for');
				if (timeLine.length == 0) {
					// Since the SortedPerson need be loaded with delay,
					// The element ".time-line-for" may not exists yet.
					var rowWidth = $(".table-hover .person").width();
					var agentNameColumnWidth = $(".table-hover .person .person-name-column").width();
					var contractTimeTitleWidth = $('.table-hover .person .contract-time-title>span').width();
					timeLineWidth = rowWidth - agentNameColumnWidth - contractTimeTitleWidth;
				} else {
					timeLineWidth = timeLine.width();
				}

				viewModel.setTimelineWidth(timeLineWidth);
			});

			viewModel.GroupId.subscribe(function () {
				if (viewModel.Loading())
					return;
				navigation.GoToTeamSchedule(viewModel.BusinessUnitId(), viewModel.GroupId(), viewModel.Date());
			});

			viewModel.Date.subscribe(function () {
				if (viewModel.Loading() || !viewModel.GroupId())
					return;
				navigation.GoToTeamSchedule(viewModel.BusinessUnitId(), viewModel.GroupId(), viewModel.Date());
			});

			ko.cleanNode(options.bindingElement);
			ko.applyBindings(viewModel, options.bindingElement);
		},

		display: function (options) {

			viewModel.Loading(true);

			if (resources.MyTeam_MoveActivity_25206) {
				viewModel.isMoveActivityFeatureEnabled(true);
			}

			viewModel.SetViewOptions(options);

			var groupPagesDeferred = $.Deferred();
			loadGroupPages(
				viewModel.BusinessUnitId(),
				helpers.Date.ToServer(viewModel.Date()),
				function (data) {
					
					var currentGroupId = function () {
						if (options.id)
							return options.id;
						if (options.groupid)
							return options.groupid;
						if (data.DefaultGroupId)
							return data.DefaultGroupId;
						if (viewModel.GroupPages().length > 0 && viewModel.GroupPages()[0].Groups().length > 0)
							return viewModel.GroupPages()[0].Groups()[0].Id;
						return null;
					};

					viewModel.SetGroupPages(data);
					viewModel.GroupId(currentGroupId());
					groupPagesDeferred.resolve();
				});

			var groupScheduleDeferred = $.Deferred();
			groupPagesDeferred.done(function () {
				var receivedSchedules = [];
				groupschedulesubscriptions.subscribeGroupSchedule(
					viewModel.BusinessUnitId(),
					viewModel.GroupId(),
					helpers.Date.ToServer(viewModel.Date()),
					function (personIdToCheck) {
						var found = false;
						ko.utils.arrayForEach(viewModel.Persons(), function(p) {
							if (p.Id == personIdToCheck) {
								found = true;
								return;
							}
						});
						return found;
					},
					function (data) {
						receivedSchedules.push.apply(receivedSchedules, data.Schedules);
						if (receivedSchedules.length === data.TotalCount) {
							viewModel.UpdateSchedules({ BaseDate: data.BaseDate, Schedules: receivedSchedules });
							groupScheduleDeferred.resolve();
							receivedSchedules = [];
						}
					}
				);
			});

			permissions.get().done(function (data) {
				viewModel.permissionAddFullDayAbsence(data.IsAddFullDayAbsenceAvailable);
				viewModel.permissionAddIntradayAbsence(data.IsAddIntradayAbsenceAvailable);
				viewModel.permissionRemoveAbsence(data.IsRemoveAbsenceAvailable);
				viewModel.permissionAddActivity(data.IsAddActivityAvailable);
				viewModel.permissionMoveActivity(data.IsMoveActivityAvailable);
			});

			return $.when(groupPagesDeferred, groupScheduleDeferred)
					.done(function () {
						viewModel.Loading(false);
						resize.notify();
					});
			
		},

		dispose: function (options) {
			unsubscriber.unsubscribeAdherence();
			groupschedulesubscriptions.unsubscribeGroupSchedule();
			$(".datepicker.dropdown-menu").remove();
		},

		setDateFromTest: function (date) {
			viewModel.Date(moment(date));
		}
	};
});

