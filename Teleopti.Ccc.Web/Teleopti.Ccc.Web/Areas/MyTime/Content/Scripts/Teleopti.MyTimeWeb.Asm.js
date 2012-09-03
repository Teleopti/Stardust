/// <reference path="Teleopti.MyTimeWeb.Common.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Asm = (function () {
	function _start(serverMsSince1970, pixelsPerHour) {
		var refreshSeconds = 20;

		_refresh(serverMsSince1970, pixelsPerHour);
		$('.asm-outer-canvas').show();

		setInterval(function () {
			_refresh(serverMsSince1970, pixelsPerHour);
		}, refreshSeconds * 1000);
	}

	function _refresh(serverMsSince1970, pixelsPerHour) {
		_moveTimeLineToNow(serverMsSince1970, pixelsPerHour);
		_updateInfoCanvas();
	}

	function _updateInfoCanvas() {
		var currentStart;
		var currentEnd;
		var currentActivity = '';
		var nu = new Date().getTeleoptiTime();
		$('.asm-layer')
			.each(function () {
				var start = parseFloat($(this).data('asm-start-milliseconds'));
				var end = parseFloat($(this).data('asm-end-milliseconds'));
				if (nu >= start && nu < end) {
					currentStart = $(this).data('asm-start-time');
					currentEnd = $(this).data('asm-end-time');
					currentActivity = $(this).data('asm-activity');
					return false;
				}
			});
		if (currentActivity != '') {
			$('#asm-info-current-activity').text(currentActivity);
			$('#asm-info-current-starttime').text(currentStart);
			$('#asm-info-current-endtime').text(currentEnd);
		}
		$('#asm-info-debug').text(new Date().getTeleoptiTime());
	}

	function _moveTimeLineToNow(serverMsSince1970, pixelsPerHour) {
		var slidingSchedules = $('.asm-sliding-schedules');
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
