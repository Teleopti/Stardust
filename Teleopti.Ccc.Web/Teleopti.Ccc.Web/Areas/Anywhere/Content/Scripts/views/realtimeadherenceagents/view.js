﻿define([
	'knockout',
	'text!templates/realtimeadherenceagents/view.html',
	'views/realtimeadherenceagents/vm',
	'errorview',
	'ajax',
	'resources'
], function (
	ko,
	view,
	realTimeAdherenceViewModel,
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
			ajax.ajax({
				url: "Agents/GetStates?teamId=" + options.id,
				error: function (jqXHR, textStatus, errorThrown) {
					if (jqXHR.status == 403) {
						errorview.display(resources.InsufficientPermission);
					}
				},
				success: function(data) {
					viewModel.fill(data);
				}
			});
		}
	};
});