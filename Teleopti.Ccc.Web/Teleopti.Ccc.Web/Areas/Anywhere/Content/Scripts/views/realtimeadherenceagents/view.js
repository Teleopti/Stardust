﻿define([
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
				loadStates("api/Agents/GetStates?teamId=" + teamId);
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
				populate("api/Agents/ForTeam?teamId=" + teamId, function() {
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
				populate("api/Agents/ForSites?" + idsToUrl("siteIds", siteIds), function() {
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
				populate("api/Agents/ForTeams?" + idsToUrl("teamIds", teamIds), function () {
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
				populateTeams(teams);
			} else if (options.id === 'MultipleSites') {
				var sites = amplify.store('MultipleSites');
				populateSites(sites);
			} else {
				populateTeam(options.id);
			}

			permissions.get().done(function (data) {
				viewModel.permissionAddActivity(data.IsAddActivityAvailable);
				viewModel.permissionSendMessage(data.IsSmsLinkAvailable);
				viewModel.notifyViaSMSEnabled(viewModel.permissionSendMessage());
			});
		},
		dispose: function (options) {
			unsubscriber.unsubscribeAdherence();
		}
	};
});