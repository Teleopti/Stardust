define([
		'knockout',
		'knockout.justgagebinding',
		'text!templates/realtimeadherenceteams/view.html',
		'views/realtimeadherenceteams/vm',
		'subscriptions.adherenceteams',
		'ajax',
		'toggleQuerier'
], function (
		ko,
		justGageBinding,
		view,
		realTimeAdherenceViewModel,
		subscriptions,
		ajax,
		toggleQuerier
	) {
	var viewModel;

	return {
		initialize: function (options) {
			options.renderHtml(view);
		},

		display: function (options) {
			var siteId = options.id;
			viewModel = realTimeAdherenceViewModel();

			ko.applyBindings(viewModel, options.bindingElement);

			ajax.ajax({
				url: "Teams/ForSite?siteId=" + siteId,
				success: function (data) {
					viewModel.fill(data);
					checkFeature();
					checkDetailFeature();
					checkAgentsForMultipleTeamsFeature();
				}
			});

			ajax.ajax({
				url: "Sites/Get?siteId=" + siteId,
				success: function (data) {
					viewModel.setSiteName(data);
				}
			});

			var checkFeature = function() {
				toggleQuerier('RTA_RtaLastStatesOverview_27789', { enabled: loadLastStates });
			}
			
			var checkDetailFeature = function () {
				toggleQuerier('RTA_DrilldownToAllAgentsInOneTeam_25234', {
					enabled: function() {
						for (var i = 0; i < viewModel.teams().length; i++) {
							(function(team) {
								team.canOpenTeam(true);
							})(viewModel.teams()[i]);
						}
					}
				});
			};

			var checkAgentsForMultipleTeamsFeature = function () {
				toggleQuerier('RTA_ViewAgentsForMultipleTeams_28967', { enabled: function () { viewModel.agentStatesForMultipleTeams(true); } });
			}

			var loadLastStates = function () {
				for (var i = 0; i < viewModel.teams().length; i++) {
					(function (team) {
						ajax.ajax({
							url: "Teams/GetOutOfAdherence?teamId=" + team.Id,
							success: function (d) {
								viewModel.update(d);
							}
						});
					})(viewModel.teams()[i]);
				}
			};

			subscriptions.subscribeAdherence(function (notification) {
				viewModel.updateFromNotification(notification);
			},
			siteId,
			function () {
				$('.realtimeadherenceteams').attr("data-subscription-done"," ");
			});
		},
	};
});

