define([
		'knockout',
		'knockout.justgagebinding',
		'text!templates/realtimeadherencesites/view.html',
		'views/realtimeadherencesites/vm',
		'subscriptions.adherencesites',
		'errorview',
		'ajax',
		'resources'
], function (
		ko,
		justGageBinding,
		view,
		realTimeAdherenceViewModel,
		subscriptions,
		errorview,
		ajax,
		resources
	) {

	var viewModel;

	return {
		initialize: function (options) {
			errorview.remove();

			var menu = ko.contextFor($('nav')[0]).$data;
			if (!menu.RealTimeAdherenceVisible()) {
				errorview.display(resources.InsufficientPermission);
				return;
			}

			options.renderHtml(view);
		},

		display: function (options) {

			viewModel = realTimeAdherenceViewModel();

			ko.applyBindings(viewModel, options.bindingElement);

			ajax.ajax({
				url: "Sites",
				success: function(data) {
					viewModel.fill(data);
					checkFeature();
				}
			});
			
			var checkFeature = function() {
				ajax.ajax({
					dataType: "text",
					url: "ToggleHandler/IsEnabled?toggle=RTA_RtaLastStatesOverview_27789",
					success: function(data) {
						if (data === "True") {
							loadLastStates();
						}
					}
				});
			};

			var loadLastStates = function () {
				for (var i = 0; i < viewModel.sites().length; i++) {
					(function(s) {
						ajax.ajax({
							url: "Sites/GetOutOfAdherence?siteId=" + s.Id,
							success: function(d) {
								viewModel.update(d);
							}
						});
					})(viewModel.sites()[i]);
				}
			};

			subscriptions.subscribeAdherence(function (notification) {
				viewModel.updateFromNotification(notification);
			}, function() {
				$('.realtimeadherencesites').attr("data-subscription-done"," ");
			});
		},
	};
});

