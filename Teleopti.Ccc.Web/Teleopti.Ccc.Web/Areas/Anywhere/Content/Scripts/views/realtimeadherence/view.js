
define([
		'knockout',
		'text!templates/realtimeadherence/view.html',
		'views/realtimeadherence/vm',
		'errorview',
		'ajax'
], function (
		ko,
		view,
		realTimeAdherenceViewModel,
		errorview,
		ajax
	) {

	var viewModel;
	
	return {
		initialize: function (options) {
			errorview.remove();
			
			var menu = ko.contextFor($('nav')[0]).$data;
			if (!menu.RealTimeAdherenceVisible()) {
				errorview.display('No permission for real time adherence overview!');
				return;
			}

			options.renderHtml(view);
		},

		display: function (options) {
			
			viewModel = realTimeAdherenceViewModel();
			ko.applyBindings(viewModel, options.bindingElement);

			ajax.ajax({
				url: "Sites",
				success: function (data) {
					viewModel.fill(data);
				}
			});

		},
	};
});

