define(
	[
	], function(
	) {

		var pageLog = null;
		
		function _initialize() {
			if (pageLog)
				return;
			
			pageLog = $("<section />")
				.insertAfter($('#content-placeholder'))
				;
		}
		
		function _displayError(message) {
			_initialize();
			pageLog
				.append(message)
				.append("<br />");
		}

		return {
		    log: function (message) {
				_displayError(message);
			}
		};

	});
