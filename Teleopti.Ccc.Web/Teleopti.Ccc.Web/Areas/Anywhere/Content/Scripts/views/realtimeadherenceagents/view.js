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
	return {
		initialize: function (options) {
			errorview.remove();
			options.renderHtml(view);
		},

		display: function (options) {
			var teamId = options.id;
			ajax.ajax({
				url: "Agents/GetStates?teamId=" + teamId,
				error: function (jqXHR, textStatus, errorThrown) {
					if (jqXHR.status == 403) {
						errorview.display(resources.InsufficientPermission);
					}
				}
			});

			subscriptions.subscribeAdherence(function (notification) {
			},
			teamId,
			function () {
				$('.realtimeadherenceagents').attr("data-subscription-done", " ");
			});
		}

	};
});