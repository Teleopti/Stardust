
define([
		'knockout',
		'jquery',
		'navigation',
		'moment',
		'subscriptions.groupschedule',
		'subscriptions.staffingmetrics',
		'helpers',
		'views/teamschedule/vm',
		'views/teamschedule/person',
		'text!templates/teamschedule/view.html',
		'resizeevent',
		'shared/current-state',
		'ajax',
		'toggleQuerier',
		'select2'
], function (
		ko,
		$,
		navigation,
		momentX,
		groupschedulesubscriptions,
		staffingmetricssubscriptions,
		helpers,
		teamScheduleViewModel,
		personViewModel,
		view,
		resize,
		currentState,
		ajax,
		toggleQuerier,
		select2 // not a direct dependency, but still a view dependency
	) {

	var viewModel;

	var loadSkills = function (date, callback) {
		$.ajax({
			url: 'StaffingMetrics/AvailableSkills',
			cache: false,
			dataType: 'json',
			data: { date: date },
			success: callback
		});
	};

	var loadGroupPages = function (date, callback) {
		ajax.ajax({
			url: 'GroupPage/AvailableGroupPages',
			data: { date: date },
			success: callback
		});
	}

	return {
		initialize: function (options) {

			options.renderHtml(view);

			viewModel = new teamScheduleViewModel();

			resize.onresize(function () {
				viewModel.TimeLine.WidthPixels($('.time-line-for').width());
			});

			viewModel.GroupId.subscribe(function () {
				if (viewModel.Loading())
					return;
				navigation.GoToTeamSchedule(viewModel.GroupId(), viewModel.Date(), viewModel.SelectedSkill());
			});

			viewModel.Date.subscribe(function () {
				if (viewModel.Loading())
					return;
				navigation.GoToTeamSchedule(viewModel.GroupId(), viewModel.Date(), viewModel.SelectedSkill());
			});

			viewModel.SelectedSkill.subscribe(function () {
				if (viewModel.Loading())
					return;
				navigation.GoToTeamSchedule(viewModel.GroupId(), viewModel.Date(), viewModel.SelectedSkill());
			});

			ko.applyBindings(viewModel, options.bindingElement);
		},

		display: function (options) {

			viewModel.Loading(true);

			toggleQuerier('MyTeam_MoveActivity_25206', {
				enabled: function() {
					viewModel.moveActivityVisible(true);
				}
			});

			viewModel.SetViewOptions(options);

			var groupPagesDeferred = $.Deferred();
			loadGroupPages(
				helpers.Date.ToServer(viewModel.Date()),
				function (data) {
					
					var currentGroupId = function () {
						if (options.id)
							return options.id;
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
				groupschedulesubscriptions.subscribeGroupSchedule(
					viewModel.GroupId(),
					helpers.Date.ToServer(viewModel.Date()),
					function (data) {
						viewModel.UpdateSchedules(data);
						groupScheduleDeferred.resolve();
					}
				);
			});

			var skillsDeferred = $.Deferred();
			toggleQuerier('MyTeam_StaffingMetrics_25562', {
				enabled: function() {
					viewModel.StaffingMetricsVisible(true);
					loadSkills(
						helpers.Date.ToServer(viewModel.Date()),
						function(data) {

							var currentSkillId = function() {
								if (options.secondaryId)
									return options.secondaryId;
								var skills = viewModel.Skills();
								if (skills.length > 0)
									return skills[0].Id;
								return null;
							};

							viewModel.SetSkills(data.Skills);
							viewModel.SelectSkillById(currentSkillId());
							skillsDeferred.resolve();
						});

					skillsDeferred.done(function() {
						if (!viewModel.SelectedSkill())
							return;
						viewModel.LoadingStaffingMetrics(true);

						staffingmetricssubscriptions.subscribeDailyStaffingMetrics(
							helpers.Date.ToServer(viewModel.Date()),
							viewModel.SelectedSkill().Id,
							function(data) {
								viewModel.SetDailyMetrics(data);
								viewModel.LoadingStaffingMetrics(false);
							});
					});
				},
				disabled: function() {
					skillsDeferred.resolve();
				}
			});

			return $.when(groupPagesDeferred, groupScheduleDeferred, skillsDeferred)
					.done(function () {
						viewModel.Loading(false);
						resize.notify();
					});
			
		},

		dispose: function (options) {
			groupschedulesubscriptions.unsubscribeGroupSchedule();
			staffingmetricssubscriptions.unsubscribeDailyStaffingMetrics();
			$(".datepicker.dropdown-menu").remove();
		},

		setDateFromTest: function (date) {
			viewModel.Date(moment(date));
		}

	};
});

