define([
	'knockout',
	'text!templates/realtimeadherenceagents/view.html',
	'views/realtimeadherenceagents/vm',
	'views/realtimeadherenceagents/subscriptions.adherenceagents',
	'errorview',
	'ajax',
	'resources',
	'amplify',
	'permissions',
	'knockoutBindings'
], function (
	ko,
	view,
	realTimeAdherenceViewModel,
	subscriptions,
	errorview,
	ajax,
	resources,
	amplify,
	permissions,
	knockoutBindings // view dependency
) {
	var viewModel;

	return {
		initialize: function (options) {
			errorview.remove();
			options.renderHtml(view);
		},

		display: function (options) {
			viewModel = realTimeAdherenceViewModel();
			viewModel.SetViewOptions(options);
			ko.cleanNode(options.bindingElement);
			ko.applyBindings(viewModel, options.bindingElement);
			setInterval(function () {
				viewModel.refreshAlarmTime();
			}, 1000);

			var loadStates = function (teamId) {
				ajax.ajax({
					url: "Agents/GetStates?teamId=" + teamId,
					error: function (jqXHR, textStatus, errorThrown) {
						if (jqXHR.status == 403) {
							errorview.display(resources.InsufficientPermission);
						}
					},
					success: function (data) {
						viewModel.fillAgentsStates(data);
					}
				});
			};

			var populateViewModel = function(teamId) {
				ajax.ajax({
						url: "Agents/ForTeam?teamId=" + teamId,
						error: function(jqXHR, textStatus, errorThrown) {
							if (jqXHR.status == 403) {
								errorview.display(resources.InsufficientPermission);
							}
						},
						success: function(data) {
							viewModel.fillAgents(data);
							if (!resources.RTA_NoBroker_31237) {
								loadStates(teamId);
							}
						}
					})
					.done(function() {
						ajax.ajax({
							url: "Teams/GetBusinessUnitId?teamId=" + teamId,
							success: function(businessUnitId) {
								subscriptions.subscribeAdherence(function(notification) {
										viewModel.updateFromNotification(notification);
									},
									businessUnitId,
									teamId,
									function() {
										$('.realtimeadherenceagents').attr("data-subscription-done", " ");
									},
									true);
							}
						});
					});
			};

			if (options.id === 'MultipleTeams') {
				subscriptions.unsubscribeAdherence();
				if (resources.RTA_ViewAgentsForMultipleTeams_28967) {
					var teams = amplify.store('MultipleTeams');
					for (var i = 0; i < teams.length; i++) {
						populateViewModel(teams[i]);
					}
				}
			} else if (options.id === 'MultipleSites') {
				if (resources.RTA_ViewAgentsForMultipleTeams_28967) {
					var sites = amplify.store('MultipleSites');
					for (var site = 0; site < sites.length; site++) {
						ajax.ajax({
							url: "Teams/ForSite?siteId=" + sites[site],
							success: function (data) {
								for (var teamInSite = 0; teamInSite < data.length; teamInSite++) {
									populateViewModel(data[teamInSite].Id);
								}
							}
						});
					}

				}
			} else {
				populateViewModel(options.id);
			}

			permissions.get().done(function (data) {
				viewModel.permissionAddActivity(data.IsAddActivityAvailable);
			});

			viewModel.changeScheduleAvailable(resources.RTA_ChangeScheduleInAgentStateView_29934);
			viewModel.agentAdherenceEnabled(resources.RTA_SeePercentageAdherenceForOneAgent_30783);
			viewModel.agentAdherenceDetailsEnabled(resources.RTA_SeeAdherenceDetailsForOneAgent_31285);
			viewModel.notifyViaSMSEnabled(resources.RTA_NotifyViaSMS_31567);
		},
		dispose: function (options) {
			subscriptions.unsubscribeAdherence();
		}
	};
});