
define([
    'knockout',
    'vm'
], function (
    ko,
    ViewModel
	) {

    var viewModel = new ViewModel();

    ko.applyBindings(viewModel);

});

