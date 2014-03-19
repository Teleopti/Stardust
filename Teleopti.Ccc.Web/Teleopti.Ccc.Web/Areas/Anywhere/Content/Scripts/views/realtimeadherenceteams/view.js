define([
		'knockout',
		'knockout.adherencebindings',
		'text!templates/realtimeadherenceteams/view.html',
		'views/realtimeadherenceteams/vm',
		'subscriptions.adherenceteams',
		'ajax'
], function (
		ko,
		gageBinding,
		view,
		realTimeAdherenceViewModel,
		subscriptions,
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
				url: "Teams/ForSite?siteId=" + options.id,
				success: function (data) {
					viewModel.fill(data);
				}
			});

			subscriptions.subscribeAdherence(function (notification) {
				viewModel.updateFromNotification(notification);
			});
		},
	};
});

