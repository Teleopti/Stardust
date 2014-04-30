define([
		'knockout',
		'knockout.justgagebinding',
		'text!templates/realtimeadherencesites/view.html',
		'views/realtimeadherencesites/vm',
		'subscriptions.adherencesites',
		'errorview',
		'ajax'
], function (
		ko,
		justGageBinding,
		view,
		realTimeAdherenceViewModel,
		subscriptions,
		errorview,
		ajax
	) {

	var viewModel;

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

			viewModel = realTimeAdherenceViewModel();

			ko.applyBindings(viewModel, options.bindingElement);

			ajax.ajax({
				url: "Sites",
				success: function (data) {
				    viewModel.fill(data);
					for (var i = 0; i < viewModel.sites().length; i++) {
				    	(function (s) {
				    		ajax.ajax({
				    			url: "Sites/GetOutOfAdherence?siteId=" + s.Id,
				    			success: function (d) {
				    				viewModel.update(d);
				    			}
				    		});
				    	})(viewModel.sites()[i]);
				    }
				}
			});


			subscriptions.subscribeAdherence(function (notification) {
				viewModel.updateFromNotification(notification);
			}, function() {
				$('.realtimeadherencesites').attr("data-subscription-done"," ");
			});
		},
	};
});

