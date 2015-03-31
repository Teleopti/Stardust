define([
		'knockout',
		'knockout.justgagebinding',
		'text!templates/realtimeadherenceteams/view.html',
		'views/realtimeadherenceteams/vm',
		'views/realtimeadherenceteams/subscriptions.adherenceteams',
		'ajax',
		'resources'
], function (
		ko,
		justGageBinding,
		view,
		realTimeAdherenceViewModel,
		subscriptions,
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
				url: "Teams/ForSite?siteId=" + siteId,
				success: function (data) {

					viewModel.fill(data);

					if (!resources.RTA_NoBroker_31237)
						loadCurrentData();

					if (resources.RTA_ViewAgentsForMultipleTeams_28967) {
						viewModel.agentStatesForMultipleTeams(true);
					}
				}
			});

			ajax.ajax({
				url: "Sites/Get?siteId=" + siteId,
				success: function (data) {
					viewModel.setSiteName(data);
				}
			});

			ajax.ajax({
				url: "Sites/GetBusinessUnitId?siteId=" + siteId,
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

			var loadCurrentData = function () {
				for (var i = 0; i < viewModel.teams().length; i++) {
					var team = viewModel.teams()[i];
					ajax.ajax({
						url: "Teams/GetOutOfAdherence?teamId=" + team.Id,
						success: function (d) {
							viewModel.update(d);
						}
					});
				}
			};
		},
		dispose : function(options) {
			subscriptions.unsubscribeAdherence();
		}
	};
});

