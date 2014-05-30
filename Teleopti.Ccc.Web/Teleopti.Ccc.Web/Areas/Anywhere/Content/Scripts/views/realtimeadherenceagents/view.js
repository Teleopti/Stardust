define([
	'knockout',
	'text!templates/realtimeadherenceagents/view.html',
	'views/realtimeadherenceagents/vm',
	'subscriptions.adherenceagents',
	'errorview',
	'ajax',
	'resources'
], function (
	ko,
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
			options.renderHtml(view);
		},

		display: function (options) {
			viewModel = realTimeAdherenceViewModel();
			ko.applyBindings(viewModel, options.bindingElement);
			var teamId = options.id;
			

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

			setInterval(function () {
				viewModel.refreshTimeInState();
				viewModel.refreshAlarmTime();
			}, 1000);

			subscriptions.subscribeAdherence(function (notification) {
				viewModel.updateFromNotification(notification);
			},
			teamId,
			function () {
				$('.realtimeadherenceagents').attr("data-subscription-done", " ");
			});
		}

	};
});