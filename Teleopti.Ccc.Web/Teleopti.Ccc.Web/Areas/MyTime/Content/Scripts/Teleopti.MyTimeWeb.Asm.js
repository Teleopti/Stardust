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
		var nu = new Date().getTeleoptiTime();

		//default-values
		$('#asm-info-current-activity').text('-');
		$('#asm-info-current-starttime').text(' ');
		$('#asm-info-current-endtime').text(' ');
		$('#asm-info-next-activity').text('-');
		$('#asm-info-next-starttime').text(' ');
		$('#asm-info-next-endtime').text(' ');

		$('.asm-layer')
			.each(function () {
				var startMs = parseFloat($(this).data('asm-start-milliseconds'));
				var endMs = parseFloat($(this).data('asm-end-milliseconds'));
				if (nu >= startMs && nu < endMs) {
					$('#asm-info-current-activity').text($(this).data('asm-activity'));
					$('#asm-info-current-starttime').text($(this).data('asm-start-time'));
					$('#asm-info-current-endtime').text($(this).data('asm-end-time'));

					var nextLayer = $(this).next();
					if (nextLayer.length > 0) {
						$('#asm-info-next-activity').text(nextLayer.data('asm-activity'));
						$('#asm-info-next-starttime').text(nextLayer.data('asm-start-time'));
						$('#asm-info-next-endtime').text(nextLayer.data('asm-end-time'));
					}
					return false;
				}
			});
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
