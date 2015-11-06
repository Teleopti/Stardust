define([
		'knockout',
		'text!templates/manageadherence/view.html',
		'views/manageadherence/vm',
		'subscriptions.unsubscriber',
		'errorview'
], function (
		ko,
		view,
		manageAdherenceViewModel,
		unsubscriber,
		errorview
	) {
	var viewModel;
	return {
		initialize: function (options) {
			errorview.remove();
			options.renderHtml(view);
		},

		display: function (options) {
			viewModel = manageAdherenceViewModel();
			ko.cleanNode(options.bindingElement);
			ko.applyBindings(viewModel, options.bindingElement);
			viewModel.setViewOptions(options);
			viewModel.load();
		},
		dispose: function (options) {
			unsubscriber.unsubscribeAdherence();
		}
	};
});

