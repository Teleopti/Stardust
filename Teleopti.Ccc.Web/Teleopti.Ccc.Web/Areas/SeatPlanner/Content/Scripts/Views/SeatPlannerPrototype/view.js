
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
			url: "SeatPlanner/TeamHierarchy/GetOld",
			success: callback
		});
	};


	var loadLocations = function(callback) {
		ajax.ajax({
			url: "SeatPlanner/LocationHierarchy/GetOld",
			success: callback
		});
	};

	var seatPlanViewModel = new viewModel();

	return {
		initialize: function (options) {
			options.renderHtml(view);
			seatPlanViewModel.SetViewOptions(options);
			ko.cleanNode(options.bindingElement);
			ko.applyBindings(seatPlanViewModel, options.bindingElement);
		},

		display: function (options) {

			seatPlanViewModel.Loading(true);
			var loadedTeamsDeferred = $.Deferred();
			loadSitesAndTeams(function (data) {
				seatPlanViewModel.LoadTeams(data);
				loadedTeamsDeferred.resolve();
			});

			var loadedLocationsDeferred = $.Deferred();
			loadLocations(function (data) {
				seatPlanViewModel.LoadLocations(data);
				loadedLocationsDeferred.resolve();
			});

			
			return $.when(loadedTeamsDeferred && loadedLocationsDeferred)
					.done(function () {
						seatPlanViewModel.Loading(false);
						//resize.notify();
					});

		},


		/*	permissions.get().done(function (data) {
					viewModel.permissionAddFullDayAbsence(data.IsAddFullDayAbsenceAvailable);
					});*/

		dispose: function (options) {
		},

		setDateFromTest: function (date) {
			seatPlanViewModel.Date(moment(date));
		}

	};
});

