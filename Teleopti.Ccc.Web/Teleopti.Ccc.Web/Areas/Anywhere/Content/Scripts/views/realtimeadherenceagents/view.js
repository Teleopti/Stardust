define([
	'knockout',
	'text!templates/realtimeadherenceagents/view.html',
	'views/realtimeadherenceagents/vm',
	'views/realtimeadherenceagents/subscriptions.adherenceagents',
	'subscriptions.unsubscriber',
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
	unsubscriber,
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

			var loadStates = function(url) {
				ajax.ajax({
					url: url,
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

			var loadStatesForTeam = function(teamId) {
				loadStates("Agents/GetStates?teamId=" + teamId);
			};

			var idsToUrl = function(idtype, ids) {
				var idsUrl = "";
				ids.forEach(function(id) {
					idsUrl += idtype + "=" + id + "&";
				});
				idsUrl = idsUrl.substring(0, idsUrl.length - 1);
				return idsUrl;
			};

			var subscriptionDone = function () {
				$('.realtimeadherenceagents').attr("data-subscription-done", " ");
			};

			var populate = function (agentsUrl,onSuccess) {
				ajax.ajax({
					url: agentsUrl,
					error: function(jqXHR, textStatus, errorThrown) {
						if (jqXHR.status == 403) {
							errorview.display(resources.InsufficientPermission);
						}
					},
					success: function(data) {
						viewModel.fillAgents(data);
						onSuccess();
					}
				});
			};

			var populateTeam = function(teamId) {
				populate("Agents/ForTeam?teamId=" + teamId, function() {
					if (!resources.RTA_NewEventHangfireRTA_34333) {
						loadStatesForTeam(teamId);
					}
					subscriptions.subscribeAdherence(function(notification) {
							viewModel.updateFromNotification(notification);
						},
						options.buid,
						teamId,
						subscriptionDone,
						true);
				});
			};
			var populateSites = function(siteIds) {
				populate("Agents/ForSites?" + idsToUrl("siteIds", siteIds), function() {
					subscriptions.subscribeForSitesAdherence(function(notification) {
							viewModel.updateFromNotification(notification);
						},
						options.buid,
						siteIds,
						subscriptionDone,
						true);
				});
			};
			var populateTeams = function(teamIds) {
				populate("Agents/ForTeams?" + idsToUrl("teamIds", teamIds), function () {
					subscriptions.subscribeForTeamsAdherence(function(notification) {
							viewModel.updateFromNotification(notification);
						},
						options.buid,
						teamIds,
						subscriptionDone,
						true);
				});
			};

			if (options.id === 'MultipleTeams') {
				var teams = amplify.store('MultipleTeams');
				if (!resources.RTA_NewEventHangfireRTA_34333) {
					for (var i = 0; i < teams.length; i++) {
						populateTeam(teams[i]);
					}
				} else {
					populateTeams(teams);
				}
			} else if (options.id === 'MultipleSites') {
				var sites = amplify.store('MultipleSites');
				if (!resources.RTA_NewEventHangfireRTA_34333) {
					for (var site = 0; site < sites.length; site++) {
						ajax.ajax({
							url: "Teams/ForSite?siteId=" + sites[site],
							success: function(data) {
								for (var teamInSite = 0; teamInSite < data.length; teamInSite++) {
									populateTeam(data[teamInSite].Id);
								}
							}
						});
					}
				} else {
					populateSites(sites);
				}
			} else {
				populateTeam(options.id);
			}

			permissions.get().done(function (data) {
				viewModel.permissionAddActivity(data.IsAddActivityAvailable);
				viewModel.permissionSendMessage(data.IsSmsLinkAvailable);
				viewModel.notifyViaSMSEnabled(resources.RTA_NotifyViaSMS_31567 && viewModel.permissionSendMessage());
			});

			viewModel.agentAdherenceEnabled(resources.RTA_NewEventHangfireRTA_34333);
			viewModel.agentAdherenceDetailsEnabled(resources.RTA_AdherenceDetails_34267);
		},
		dispose: function (options) {
			unsubscriber.unsubscribeAdherence();
		}
	};
});