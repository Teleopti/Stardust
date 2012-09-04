﻿/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />

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
		_bindTimeLine();
		$('.asm-outer-canvas').show();

		setInterval(function () {
			_refresh(serverMsSince1970, pixelsPerHour);
		}, refreshSeconds * 1000);
	}

	function _bindTimeLine() {
		var timelineArray = new Array();
		for (var day = 0; day <= 2; day++) {
			for (var hour = 0; hour < 24; hour++) {
				timelineArray.push(hour);
			}
		}
		ko.applyBindings({
			timeLines: timelineArray
		});
	}

	function _refresh(serverMsSince1970, pixelsPerHour) {
		_moveTimeLineToNow(serverMsSince1970, pixelsPerHour);
		_updateInfoCanvas();
	}

	function _updateInfoCanvas() {
		var nu = new Date().getTeleoptiTime();

		//default-values
		$('#asm-info-current-activity').text('');
		$('#asm-info-current-starttime').text('');
		$('#asm-info-current-endtime').html('');
		$('#asm-info-next-activity').text('');
		$('#asm-info-next-starttime').html('');
		$('#asm-info-next-endtime').html('');

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
