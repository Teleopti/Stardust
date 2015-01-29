define([
		'knockout',
		'knockout.justgagebinding',
		'text!templates/realtimeadherenceteams/view.html',
		'views/realtimeadherenceteams/vm',
		'subscriptions.adherenceteams',
		'ajax',
		'polling/adherenceteams',
		'resources'
], function (
		ko,
		justGageBinding,
		view,
		realTimeAdherenceViewModel,
		broker,
		ajax,
		poller,
		resources
	) {
	var viewModel;

	var toggledStateGetter = function () {
		if (resources.RTA_NoBroker_31237)
			return poller;
		return broker;
	}

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

			ajax.ajax({
				url: "Sites/GetBusinessUnitId?siteId=" + siteId,
				success: function (businessUnitId) {
					toggledStateGetter().subscribeAdherence(function (notification) {
						viewModel.updateFromNotification(notification);
					},
					businessUnitId,
					siteId,
					function () {
						$('.realtimeadherenceteams').attr("data-subscription-done", " ");
					});
				}
			});

			var checkFeature = function () {
				if (resources.RTA_RtaLastStatesOverview_27789)
					loadLastStates('Teams/GetOutOfAdherence');
				if (resources.RTA_NoBroker_31237)
					loadLastStates('Teams/GetOutOfAdherenceLite');
			}
			
			var checkDetailFeature = function () {
				if (resources.RTA_DrilldownToAllAgentsInOneTeam_25234) {
					for (var i = 0; i < viewModel.teams().length; i++) {
						(function(team) {
							team.canOpenTeam(true);
						})(viewModel.teams()[i]);
					}
				}
			};

			var checkAgentsForMultipleTeamsFeature = function () {
				if (resources.RTA_ViewAgentsForMultipleTeams_28967) {
					viewModel.agentStatesForMultipleTeams(true);
				}
			}

			var loadLastStates = function (url) {
				for (var i = 0; i < viewModel.teams().length; i++) {
					(function (team) {
						ajax.ajax({
							url: url+"?teamId=" + team.Id,
							success: function (d) {
								viewModel.update(d);
							}
						});
					})(viewModel.teams()[i]);
				}
			};
		},
		dispose : function(options) {
			toggledStateGetter().unsubscribeAdherence();
		}
	};
});

