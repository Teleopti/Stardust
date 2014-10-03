
define([
    'knockout',
    'vm'
], function (
    ko,
    viewmodel
	) {


	console.log('hej');
    var vm = new viewmodel();
    ko.applyBindings(vm);
	

});

