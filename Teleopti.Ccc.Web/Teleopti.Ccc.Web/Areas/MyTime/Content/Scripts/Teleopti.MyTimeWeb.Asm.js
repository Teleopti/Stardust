/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Asm = (function () {
	function _start(serverMsSince1970, pixelsPerHour) {
		var refreshSeconds = 3;
		var observableInfo = ko.observable();
		var timeLineArray = _timeLineArray();

		ko.applyBindings({
			timeLines: timeLineArray,
			activityInfo: observableInfo
		});

		_refresh(serverMsSince1970, pixelsPerHour, observableInfo);
		$('.asm-outer-canvas').show();

		setInterval(function () {
			_refresh(serverMsSince1970, pixelsPerHour, observableInfo);
		}, refreshSeconds * 1000);
	}

	function _timeLineArray() {
		var timelineArray = new Array();
		for (var day = 0; day <= 2; day++) {
			for (var hour = 0; hour < 24; hour++) {
				timelineArray.push(hour);
			}
		}
		return timelineArray;
	}

	function _refresh(serverMsSince1970, pixelsPerHour, observableInfo) {
		_moveTimeLineToNow(serverMsSince1970, pixelsPerHour);
		_updateInfoCanvas(observableInfo);
	}

	function _updateInfoCanvas(observableInfo) {
		var model = new Array();

		var timelinePosition = parseFloat($('.asm-time-marker').css('width')) - parseFloat($(".asm-sliding-schedules").css('left'));
		$('.asm-layer')
			.each(function () {
				var startPos = parseFloat($(this).css('left'));
				var endPos = startPos + parseFloat($(this).css('padding-left'));
				if (endPos > timelinePosition) {
					var active = false;
					if (startPos <= timelinePosition) {
						active = true;
					}
					model.push({ 'payload': $(this).data('asm-activity'), 'time': $(this).data('asm-start-time'), 'active': active });
				}
			});
		observableInfo(model);
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
