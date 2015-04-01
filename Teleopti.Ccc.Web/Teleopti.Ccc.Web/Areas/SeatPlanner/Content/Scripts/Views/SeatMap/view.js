
define([
		'knockout',
		'jquery',
		'navigation',
		'moment',
		'Views/SeatMap/vm',
		'text!templates/SeatMap/view.html',
		'ajax'
], function (
		ko,
		$,
		navigation,
		momentX,
		viewModel,
		view,
		ajax
	) {

	var seatMapViewModel = new viewModel();

	var loadSeatMap = function (callback, id) {
		ajax.ajax({
			url: "SeatPlanner/SeatMap/GetOld?id=" + id,
			success: callback
		});
	};


	return {
		initialize: function (options) {
			options.renderHtml(view);
			seatMapViewModel.SetViewOptions(options);
			ko.cleanNode(options.bindingElement);
			ko.applyBindings(seatMapViewModel, options.bindingElement);
		},

		display: function (options) {

			//seatMapViewModel.Loading(true); // -- RobTODO implement loading dialog, including for refreshes...

			seatMapViewModel.SetupCanvas();

			var loadedSeatMap = $.Deferred();
			loadSeatMap(function (data) {
				seatMapViewModel.LoadSeatMap(data);
				loadedSeatMap.resolve();
			});

			return $.when(loadedSeatMap)
					.done(function () {
						//seatPlanViewModel.Loading(false);
						
					});
			
		},
		dispose: function (options) {
		},

		
	};
});

