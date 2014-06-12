define(
    [
		'ajax'
    ], function (
        ajax
    ) {

        return {
        	checkToggle: function (featureName) {
		        ajax.ajax({
		        	url: "ToggleHandler/IsEnabled?toggle="+featureName,
		        	success: function (data) {
								return data.IsEnabled;
			        }
		        });
	        }
	}
});
