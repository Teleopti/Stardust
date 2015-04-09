
define([
    'knockout',
	'resources',
    'vm',
	'ajax'
], function (
    ko,
	resources,
    viewmodel,
	ajax
	) {

    var vm = new viewmodel();
    ko.applyBindings(vm);

	var loadNavigationContent = function(callback) {
		ajax.ajax({
			url: 'Messages/Application/NavigationContent',
			success: callback,
			error: function(data) {
				vm.ErrorMessage(data.statusText);
			}
		});
	};

	loadNavigationContent(function (responseData) {
    	vm.MyTimeVisible(responseData.IsMyTimeAvailable === true);
    	vm.AnywhereVisible(responseData.IsAnywhereAvailable === true);
    	vm.UserName(responseData.UserName);

    });

	ajax.ajax({
		url: 'Messages/Application/GetPersons?ids=' + window.location.hash.substring(1),
		success: function(data) {
			vm.Receivers.removeAll();
			ko.utils.arrayForEach(data.People, function(item) {
				vm.Receivers.push({ Name: item.Name, Id: item.Id });
			});
		},
		error: function (data) {
			if (data.status === 403) {
				vm.ErrorMessage(resources.InsufficientPermission);
			} else {
				vm.ErrorMessage(data.statusText);
			}
		}
	});
});

