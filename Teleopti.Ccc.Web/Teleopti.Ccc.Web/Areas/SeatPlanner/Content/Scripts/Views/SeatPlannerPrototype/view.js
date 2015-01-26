
define([
		'knockout',
		'jquery',
		'navigation',
		'moment',
		'Views/SeatPlannerPrototype/vm',
		'text!templates/SeatPlannerPrototype/view.html',
		//'shared/current-state',
		'ajax'
		//'toggleQuerier',
		//'select2',
		//'permissions'
], function (
		ko,
		$,
		navigation,
		momentX,
		viewModel,
		view,

		//currentState,
		ajax
	//toggleQuerier,
	//select2, // not a direct dependency, but still a view dependency
	//permissions
	) {


	var loadSitesAndTeams = function (callback) {
		ajax.ajax({
			url: "SeatPlanner/TeamHierarchy/Get",
			success: callback

		});
	};

	return {
		initialize: function (options) {

			options.renderHtml(view);
			viewModel = new viewModel();
			viewModel.SetViewOptions(options);
			ko.cleanNode(options.bindingElement);
			ko.applyBindings(viewModel, options.bindingElement);
		},

		display: function (options) {

			viewModel.Loading(true);
			var loadedTeamsDeferred = $.Deferred();
			loadSitesAndTeams(function (data) {
				viewModel.LoadTeams(data);
				loadedTeamsDeferred.resolve();
			});


			return $.when(loadedTeamsDeferred)
					.done(function () {
						viewModel.Loading(false);
						//resize.notify();
					});

		},


		/*			permissions.get().done(function (data) {
						viewModel.permissionAddFullDayAbsence(data.IsAddFullDayAbsenceAvailable);
						viewModel.permissionAddIntradayAbsence(data.IsAddIntradayAbsenceAvailable);
						viewModel.permissionRemoveAbsence(data.IsRemoveAbsenceAvailable);
						viewModel.permissionAddActivity(data.IsAddActivityAvailable);
						viewModel.permissionMoveActivity(data.IsMoveActivityAvailable);
					});*/

		//return $.when(groupPagesDeferred, groupScheduleDeferred, skillsDeferred)
		//		.done(function () {
		//			viewModel.Loading(false);
		//			resize.notify();
		//		});

		dispose: function (options) {
		},

		setDateFromTest: function (date) {
			viewModel.Date(moment(date));
		}

	};
});

