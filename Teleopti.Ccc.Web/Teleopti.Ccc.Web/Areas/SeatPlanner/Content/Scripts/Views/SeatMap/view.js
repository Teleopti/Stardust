
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

	return {
		initialize: function (options) {
			options.renderHtml(view);
			seatMapViewModel.SetViewOptions(options);
			ko.cleanNode(options.bindingElement);
			ko.applyBindings(seatMapViewModel, options.bindingElement);
		},

		display: function (options) {

			//seatMapViewModel.Loading(true); // -- RobTODO causes issues with canvas loading....perhaps float a loading dialog instead.

			seatMapViewModel.SetupCanvas();

			//seatMapViewModel.Loading(false);

		},
		dispose: function (options) {
		},

		
	};
});

