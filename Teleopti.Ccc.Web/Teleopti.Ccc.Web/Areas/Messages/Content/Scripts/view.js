
define([
    'knockout',
    'vm',
	'ajax'
], function (
    ko,
    viewmodel,
	ajax
	) {

    var vm = new viewmodel();
    ko.applyBindings(vm);

    var getUrlParameter = function(sParam) {
    	var sPageUrl = window.location.search.substring(1);
    	var sUrlVariables = sPageUrl.split('&');
    	for (var i = 0; i < sUrlVariables.length; i++) {
    		var sParameterName = sUrlVariables[i].split('=');
    		if (sParameterName[0] == sParam) {
    			return sParameterName[1];
    		}
    	}
	    return "";
    };

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
		url: 'Messages/Application/GetPersons?ids=' + getUrlParameter("ids"),
		success: function(data) {
			vm.Receivers.removeAll();
			ko.utils.arrayForEach(data.People, function(item) {
				vm.Receivers.push({ Name: item.Name, Id: item.Id });
			});
		},
		error: function(data) { vm.ErrorMessage(data.statusText); }
	});
});

