
define([
		'knockout',
		'jquery',
		'navigation',
		'moment',
		'Views/Dashboard/vm',
		'text!templates/Dashboard/view.html',
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

	var dashboardViewModel = new viewModel();

	return {
		initialize: function (options) {
			options.renderHtml(view);
			dashboardViewModel.SetViewOptions(options);
			ko.cleanNode(options.bindingElement);
			ko.applyBindings(dashboardViewModel, options.bindingElement);
		},

		display: function (options) {

			dashboardViewModel.Loading(true); 

			
			dashboardViewModel.Loading(false);

		},
		dispose: function (options) {
		},

		
	};
});

