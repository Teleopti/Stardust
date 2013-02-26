
define([
		'knockout',
		'text!templates/personschedule/view.html'
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

