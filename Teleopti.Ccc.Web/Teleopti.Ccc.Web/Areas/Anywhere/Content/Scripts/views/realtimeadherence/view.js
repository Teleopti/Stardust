
define([
		'knockout',
		'text!templates/realtimeadherence/view.html',
		'errorview'
], function (
		ko,
		view,
		errorview
	) {
	return {
		initialize: function (options) {
			errorview.remove();
			
			var menu = ko.contextFor($('nav')[0]).$data;
			if (!menu.RealTimeAdherenceVisible()) {
				errorview.display('No permission for real time adherence overview!');
				return;
			}

			options.renderHtml(view);
		},

		display: function (options) {
			
		},
	};
});

