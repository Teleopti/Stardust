
define([
    'knockout',
    'vm'
], function (
    ko,
    ViewModel
	) {

    var viewModel = new ViewModel();

	if (window.location.hash) {
		var scenarioName = window.location.hash.substring(1);

		var replaced = scenarioName.split('+').join(' ');

		viewModel.SelectScenarioByName(replaced);
	};
    ko.applyBindings(viewModel);

});

