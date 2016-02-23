
define([
    'knockout',
    'vm'
], function (
    ko,
    viewmodel
	) {

    var vm = new viewmodel();
    ko.applyBindings(vm);

});

