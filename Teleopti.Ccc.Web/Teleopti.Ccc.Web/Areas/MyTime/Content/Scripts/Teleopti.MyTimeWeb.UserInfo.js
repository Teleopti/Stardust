
if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.UserInfo = (function ($) {
	
	var ajax = new Teleopti.MyTimeWeb.Ajax();

	var promise = ajax.Ajax({
		url: 'UserInfo/Culture',
		dataType: "json",
		type: 'GET'
	});

	return {
		WhenLoaded: function(action) {
			promise.done(function(data) {
				action(data);
			});
			return promise;
		}
	};

})(jQuery);
