define([
	'knockout',
	'text!templates/realtimeadherenceagents/view.html',
	'views/realtimeadherenceagents/vm',
	'subscriptions.adherenceagents',
	'errorview',
	'ajax',
	'resources',
	'amplify',
	'toggleQuerier'
], function (
	ko,
	view,
	realTimeAdherenceViewModel,
	subscriptions,
	errorview,
	ajax,
	resources,
	amplify,
	toggleQuerier
) {
	var viewModel;
	return {
		initialize: function (options) {
			errorview.remove();
			options.renderHtml(view);
		},

		display: function (options) {
			viewModel = realTimeAdherenceViewModel();
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
						viewModel.fillAgentsStates();
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


				subscriptions.subscribeAdherence(function (notification) {
					viewModel.updateFromNotification(notification);
				},
				teamId,
				function () {
					$('.realtimeadherenceagents').attr("data-subscription-done", " ");
				});
			}

			if (options.id === 'MultipleTeams') {
				toggleQuerier('RTA_ViewAgentsForMultipleTeams_28967', {
					enabled: function() {
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
								success: function(data) {
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
		}
	};
});