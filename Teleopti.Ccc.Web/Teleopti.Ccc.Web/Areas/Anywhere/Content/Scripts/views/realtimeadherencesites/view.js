﻿define([
		'knockout',
		'knockout.justgagebinding',
		'text!templates/realtimeadherencesites/view.html',
		'views/realtimeadherencesites/vm',
		'subscriptions.adherencesites',
		'errorview',
		'ajax',
		'resources',
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
		poller
	) {

	var viewModel;

	var toggledStateGetter = function () {
		if (resources.RTA_NoBroker_31237)
			return poller;
		return subscriptions;
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

