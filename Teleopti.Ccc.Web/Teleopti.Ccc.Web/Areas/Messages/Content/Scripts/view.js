
define([
    'knockout',
    'vm',
	'http'
], function (
    ko,
    viewmodel,
	http
	) {

    var vm = new viewmodel();
    ko.applyBindings(vm);

    function getUrlParameter(sParam) {
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


	http.get('Messages/Application/GetPersons', { ids: getUrlParameter("ids") })
		.done(function(data) {
			vm.Receivers.removeAll();
			ko.utils.arrayForEach(data.People, function(item) {
				vm.Receivers.push({ Name: item.Name });
			});
		}).fail(function(data) {
			vm.ErrorMessage(data.statusText);
		});

});

