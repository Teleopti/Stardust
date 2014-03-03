
define([
	'knockout',
		'text!templates/realtimeadherencesite/view.html',
		'views/realtimeadherence/vm',
		'ajax'
], function (
	ko,
		view,
		realTimeAdherenceViewModel,
		ajax
	) {
	var viewModel;

	return {
		initialize: function (options) {
			options.renderHtml(view);
		},

		display: function (options) {

			viewModel = realTimeAdherenceViewModel();

			ko.applyBindings(viewModel, options.bindingElement);

			ajax.ajax({
				url: "Teams",
				success: function (data) {
					viewModel.fill(data);
				}
			});
		},
	};
});

