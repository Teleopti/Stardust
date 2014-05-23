define(
    [
		'ajax'
    ], function (
        ajax
    ) {

        return {
        	checkToggle: function (featureName) {
		        ajax.ajax({
		        	dataType: "text",
		        	url: "ToggleHandler/IsEnabled?toggle="+featureName,
		        	success: function (data) {
						return data=="True";
			        }
		        });
	        }
	}
});
