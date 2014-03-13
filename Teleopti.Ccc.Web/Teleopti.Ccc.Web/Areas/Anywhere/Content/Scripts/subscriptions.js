define([
				'jquery',
				'messagebroker'
], function (
			$,
			messagebroker
	) {

	var startPromise;
	
	var start = function () {
		startPromise = messagebroker.start();
		return startPromise;
	};

	return {
		start: start
	};

});
