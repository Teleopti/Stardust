define([
		'knockout',
		'knockout.justgagebinding',
		'text!templates/realtimeadherencesites/view.html',
		'views/realtimeadherencesites/vm',
		'subscriptions.adherencesites',
		'errorview',
		'ajax',
		'resources',
		'toggleQuerier'
], function (
		ko,
		justGageBinding,
		view,
		realTimeAdherenceViewModel,
		subscriptions,
		errorview,
		ajax,
		resources,
		toggleQuerier
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

			viewModel.load();

			/*ajax.ajax({
				url: "Sites",
				success: function(data) {
					viewModel.fill(data);
					checkFeature();
					checkAgentsForMultipleTeamsFeature();
					checkBusinessUnitsFeature();
				}
			});

			var checkFeature = function () {
				toggleQuerier('RTA_RtaLastStatesOverview_27789', { enabled: loadLastStates });
			}
			
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

			var checkAgentsForMultipleTeamsFeature = function () {
				toggleQuerier('RTA_ViewAgentsForMultipleTeams_28967', { enabled: function () { viewModel.agentStatesForMultipleSites(true); } });
			}
			
			var checkBusinessUnitsFeature = function () {
				toggleQuerier('RTA_MonitorMultipleBusinessUnits_28348', { enabled: function() {
					ajax.ajax({
						url: "BusinessUnit",
						success: function (data) {
							viewModel.fillBusinessUnits(data);
						}
					});
				} });
			}

			subscriptions.subscribeAdherence(function (notification) {
				viewModel.updateFromNotification(notification);
			}, function() {
				$('.realtimeadherencesites').attr("data-subscription-done"," ");
			});*/
		}
	};
});

