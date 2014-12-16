define([
		'knockout',
		'knockout.justgagebinding',
		'text!templates/realtimeadherencesites/view.html',
		'views/realtimeadherencesites/vm',
		'subscriptions.adherencesites',
		'errorview',
		'ajax',
		'resources',
		'syncToggleQuerier',
		'polling/adherencesites'
], function (
		ko,
		justGageBinding,
		view,
		realTimeAdherenceViewModel,
		subscriptions,
		errorview,
		ajax,
		resources,
		syncToggleQuerier,
		poller
	) {

	var viewModel;
	var toggledStateGetter = function() {
		var subscription;
		syncToggleQuerier('RTA_NoBroker_31237', {
			enabled: function () { subscription = poller; },
			disabled: function () { subscription = subscriptions; }
		});
		return subscription;
	}
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
			viewModel = realTimeAdherenceViewModel(toggledStateGetter());
			viewModel.BusinessUnitId(options.buid);
			ko.cleanNode(options.bindingElement);
			ko.applyBindings(viewModel, options.bindingElement);

			viewModel.load();
		},
		dispose: function (options) {
			toggledStateGetter().unsubscribeAdherence();
		}
	};
});

