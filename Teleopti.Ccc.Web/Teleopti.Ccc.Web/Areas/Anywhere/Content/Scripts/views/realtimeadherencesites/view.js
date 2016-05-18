define([
		'knockout',
		'knockout.justgagebinding',
		'text!templates/realtimeadherencesites/view.html',
		'views/realtimeadherencesites/vm',
		'views/realtimeadherencesites/subscriptions.adherencesites',
		'subscriptions.unsubscriber',
		'errorview',
		'ajax',
		'resources'
], function (
		ko,
		justGageBinding,
		view,
		realTimeAdherenceViewModel,
		subscriptions,
		unsubscriber,
		errorview,
		ajax,
		resources
	) {

	var viewModel;

	return {
		initialize: function(options) {
			errorview.remove();

			var menu = ko.contextFor($('nav')[0]).$data;
			if (!menu.RealTimeAdherenceVisible()) {
				errorview.display(resources.InsufficientPermission);
				return;
			}

			options.renderHtml(view);
		},

		display: function(options) {
			viewModel = realTimeAdherenceViewModel();
			viewModel.BusinessUnitId(options.buid);
			ko.cleanNode(options.bindingElement);
			ko.applyBindings(viewModel, options.bindingElement);

			viewModel.load();
		},
		dispose: function (options) {
			unsubscriber.unsubscribeAdherence();
		}
	};
});

