
define([
		'knockout',
		'text!templates/agentschedule/view.html'
	], function (
		ko,
		view
	) {

		return {
			display: function (options) {

				options.renderHtml(view);

			}
		};
	});

