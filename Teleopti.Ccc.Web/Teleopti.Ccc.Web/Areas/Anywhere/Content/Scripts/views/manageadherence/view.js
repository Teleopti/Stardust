define([
		'knockout',
		'text!templates/manageadherence/view.html',
		'errorview'
], function (
		ko,
		view,
		errorview
	) {
	
	return {
		initialize: function (options) {
			errorview.remove();
			options.renderHtml(view);
		},

		display: function (options) {
		}
	};
});

