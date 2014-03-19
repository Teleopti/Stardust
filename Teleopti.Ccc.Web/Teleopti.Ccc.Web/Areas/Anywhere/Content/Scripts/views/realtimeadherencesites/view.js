//fix - don't have the dependencies here!
define([
		'knockout',
		'knockout.adherencebindings',
		'text!templates/realtimeadherencesites/view.html',
		'views/realtimeadherencesites/vm',
		'subscriptions.adherencesites',
		'errorview',
		'ajax'
], function (
		ko,
		koBinding,
		view,
		realTimeAdherenceViewModel,
		subscriptions,
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

			subscriptions.subscribeAdherence(function (notification) {
				viewModel.updateFromNotification(notification);
			});
		},
	};
});

