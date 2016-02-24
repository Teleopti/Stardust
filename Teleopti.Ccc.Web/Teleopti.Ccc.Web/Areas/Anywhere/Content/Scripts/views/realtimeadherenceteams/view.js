define([
		'knockout',
		'knockout.justgagebinding',
		'text!templates/realtimeadherenceteams/view.html',
		'views/realtimeadherenceteams/vm',
		'views/realtimeadherenceteams/subscriptions.adherenceteams',
		'subscriptions.unsubscriber',
		'ajax',
		'resources'
], function (
		ko,
		justGageBinding,
		view,
		realTimeAdherenceViewModel,
		subscriptions,
		unsubscriber,
		ajax,
		resources
	) {
	var viewModel;

	return {
		initialize: function (options) {
			options.renderHtml(view);
		},

		display: function (options) {
			var siteId = options.id;
			viewModel = realTimeAdherenceViewModel();
			viewModel.SetViewOptions(options);
			ko.cleanNode(options.bindingElement);
			ko.applyBindings(viewModel, options.bindingElement);

			ajax.ajax({
				url: "api/Teams/ForSite?siteId=" + siteId,
				success: function (data) {
					viewModel.fill(data);
				}
			});

			ajax.ajax({
				url: "api/Sites/Get?siteId=" + siteId,
				success: function (data) {
					viewModel.setSiteName(data);
				}
			});

			ajax.ajax({
				url: "api/Sites/GetBusinessUnitId?siteId=" + siteId,
				success: function (businessUnitId) {
					subscriptions.subscribeAdherence(function (notification) {
						viewModel.updateFromNotification(notification);
					},
					businessUnitId,
					siteId,
					function () {
						$('.realtimeadherenceteams').attr("data-subscription-done", " ");
					});
				}
			});
		},
		dispose : function(options) {
			unsubscriber.unsubscribeAdherence();
		}
	};
});

