/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Asm = (function () {
	var pxPerHour;

	function _start(serverMsSince1970, pixelsPerHour, hours) {
		var refreshSeconds = 1;
		pxPerHour = pixelsPerHour;
		var observableInfo = ko.observable();

		ko.applyBindings({
			timeLines: hours,
			activityInfo: observableInfo
		});

		_refresh(serverMsSince1970, observableInfo);
		$('.asm-outer-canvas').show();

		setInterval(function () {
			_refresh(serverMsSince1970, observableInfo);
		}, refreshSeconds * 1000);
	}

	function _refresh(serverMsSince1970, observableInfo) {
		_moveTimeLineToNow(serverMsSince1970);
		_updateInfoCanvas(observableInfo);
	}

	function _updateInfoCanvas(observableInfo) {
		var model = new Array();
		var timeLineFixedPos = parseFloat($('.asm-time-marker').css('width'));
		var timelinePosition = timeLineFixedPos - parseFloat($(".asm-sliding-schedules").css('left'));

		$('.asm-layer')
			.each(function () {
				var startPos = parseFloat($(this).css('left'));
				var endPos = startPos + parseFloat($(this).css('padding-left'));
				if (endPos > timelinePosition) {
					var active = false;
					if (startPos <= timelinePosition) {
						active = true;
					}
					var startText = $(this).data('asm-start-time');

					if (startPos - timelinePosition  >= 24 * pxPerHour) {
						startText += '+1';
					}

					model.push({ 'payload': $(this).data('asm-activity'), 'time': startText, 'active': active });
				}
			});
		observableInfo(model);
	}

	function _moveTimeLineToNow(serverMsSince1970) {
		var slidingSchedules = $('.asm-sliding-schedules');
		var clientMsSince1970 = new Date().getTeleoptiTime();
		var msSinceStart = clientMsSince1970 - serverMsSince1970;
		var hoursSinceStart = msSinceStart / 1000 / 60 / 60;
		var startPixel = -(pxPerHour * hoursSinceStart);
		slidingSchedules.css('left', (startPixel) + 'px');
	}



	return {
		Init: function (serverMsSince1970, pixelsPerHour, hours) {
			$('body').css('overflow', 'hidden');
			_start(serverMsSince1970, pixelsPerHour, hours);
		}
	};

})(jQuery);
