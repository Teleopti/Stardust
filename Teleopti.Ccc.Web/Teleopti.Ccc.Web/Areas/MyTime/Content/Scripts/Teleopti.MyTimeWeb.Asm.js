/// <reference path="Teleopti.MyTimeWeb.Common.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Asm = (function () {
	function _start(serverMsSince1970, pixelsPerHour) {
		var refreshMilliSeconds = 200;
		var slidingSchedules = $('.asm-sliding-schedules');

		_moveTimeLineToNow(slidingSchedules, serverMsSince1970, pixelsPerHour);
		slidingSchedules.show();


		setInterval(function () {
			_moveTimeLineToNow(slidingSchedules, serverMsSince1970, pixelsPerHour);
			_updateInfoCanvas();
		}, refreshMilliSeconds);
	}

	function _updateInfoCanvas() {
		$('#asm-info-debug').text(new Date().getTeleoptiTime());
	}

	function _moveTimeLineToNow(slidingSchedules, serverMsSince1970, pixelsPerHour) {
		var clientMsSince1970 = new Date().getTeleoptiTime();
		var msSinceStart = clientMsSince1970 - serverMsSince1970;

		var hoursSinceStart = msSinceStart / 1000 / 60 / 60;
		var startPixel = -(pixelsPerHour * hoursSinceStart);
		slidingSchedules.css('left', (startPixel) + 'px');
	}

	return {
		Init: function (serverMsSince1970, pixelsPerHour) {
			_start(serverMsSince1970, pixelsPerHour);
		}
	};

})(jQuery);
