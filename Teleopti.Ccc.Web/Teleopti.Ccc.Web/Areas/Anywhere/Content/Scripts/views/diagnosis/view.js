define([
		'knockout',
		'text!templates/diagnosis/view.html',
		'views/diagnosis/vm'
], function (
		ko,
		view,
		diagnosisViewModel
	) {

	var viewModel;

	return {
		initialize: function (options) {

			options.renderHtml(view);
		},

		display: function (options) {

			viewModel = diagnosisViewModel();

			ko.applyBindings(viewModel, options.bindingElement);

		},
	};
});

