define([
	'knockout',
	'text!templates/realtimeadherenceagents/view.html',
	'views/realtimeadherenceagents/vm',
	'subscriptions.adherenceagents',
	'errorview',
	'ajax',
	'resources',
	'amplify',
	'toggleQuerier',
	'permissions',
	'knockoutBindings',
	'syncToggleQuerier',
	'polling/adherencetagents'
], function (
	ko,
	view,
	realTimeAdherenceViewModel,
	broker,
	errorview,
	ajax,
	resources,
	amplify,
	toggleQuerier,
	permissions,
	knockoutBindings,
	syncToggleQuerier,
	poller
) {
	var viewModel;

	var toggledStateGetter = function () {
		var subscription;
		syncToggleQuerier('RTA_NoBroker_31237', {
			enabled: function () { subscription = poller; },
			disabled: function () { subscription = broker; }
		});
		return subscription;
	}

	return {
		initialize: function (options) {
			errorview.remove();
			options.renderHtml(view);
		},

		display: function (options) {
			var subscriptions = toggledStateGetter();
			viewModel = realTimeAdherenceViewModel();
			viewModel.SetViewOptions(options);
			ko.cleanNode(options.bindingElement);
			ko.applyBindings(viewModel, options.bindingElement);
			setInterval(function () {
				viewModel.refreshAlarmTime();
			}, 1000);

			var populateViewModel = function (teamId) {
				ajax.ajax({
					url: "Agents/ForTeam?teamId=" + teamId,
					error: function (jqXHR, textStatus, errorThrown) {
						if (jqXHR.status == 403) {
							errorview.display(resources.InsufficientPermission);
						}
					},
					success: function (data) {
						viewModel.fillAgents(data);
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
					}
				});

				ajax.ajax({
					url: "Teams/GetBusinessUnitId?teamId=" + teamId,
					success: function (businessUnitId) {
						subscriptions.subscribeAdherence(function (notification) {
							viewModel.updateFromNotification(notification);
						},
							businessUnitId,
							teamId,
							function () {
								$('.realtimeadherenceagents').attr("data-subscription-done", " ");
							},
							true);
					}
				});
			}

			if (options.id === 'MultipleTeams') {
				subscriptions.unsubscribeAdherence();
				toggleQuerier('RTA_ViewAgentsForMultipleTeams_28967', {
					enabled: function () {
						var teams = amplify.store('MultipleTeams');
						for (var i = 0; i < teams.length; i++) {
							populateViewModel(teams[i]);
						}
					}
				});
			} else if (options.id === 'MultipleSites') {
				toggleQuerier('RTA_ViewAgentsForMultipleTeams_28967', {
					enabled: function () {
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
				});
			} else {
				populateViewModel(options.id);
			}

			permissions.get().done(function (data) {
				viewModel.permissionAddActivity(data.IsAddActivityAvailable);
			});

			toggleQuerier('RTA_ChangeScheduleInAgentStateView_29934', {
				enabled: function () {
					viewModel.changeScheduleAvailable(true);
				}
			});

			toggleQuerier('RTA_SeePercentageAdherenceForOneAgent_30783', {
				enabled: function () {
					viewModel.agentAdherenceEnabled(true);
				}
			});

			toggleQuerier('RTA_SeeAdherenceDetailsForOneAgent_31285', {
				enabled: function () {
					viewModel.agentAdherenceDetailsEnabled(true);
				}
			});

			toggleQuerier('RTA_NotifyViaSMS_31567', {
				enabled: function () {
					viewModel.notifyViaSMSEnabled(true);
				}
			});

		},
		dispose: function (options) {
			toggledStateGetter().unsubscribeAdherence();
		}
	};
});