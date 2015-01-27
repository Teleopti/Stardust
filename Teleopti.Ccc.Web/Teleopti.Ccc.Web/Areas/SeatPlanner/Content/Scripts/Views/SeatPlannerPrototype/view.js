
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


	var loadLocations = function(callback) {
		ajax.ajax({
			url: "SeatPlanner/LocationHierarchy/Get"
		});
	}

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

			//Robtodo: loaded locations defer/resolve

			return $.when(loadedTeamsDeferred)
					.done(function () {
						viewModel.Loading(false);
						//resize.notify();
					});

		},


		/*	permissions.get().done(function (data) {
					viewModel.permissionAddFullDayAbsence(data.IsAddFullDayAbsenceAvailable);
					});*/

		dispose: function (options) {
		},

		setDateFromTest: function (date) {
			viewModel.Date(moment(date));
		}

	};
});

