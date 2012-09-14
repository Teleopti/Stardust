/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Asm = (function () {
	var refreshSeconds = 1;
	var pixelPerHours = 50;
	var timeLineMarkerWidth = 100;

	function _start() {
		_setFixedElementAttributes();

		var yesterdayTemp = new Date(new Date().getTeleoptiTime());
		yesterdayTemp.setDate(yesterdayTemp.getDate() - 1);
		var yesterday = new Date(yesterdayTemp.getFullYear(), yesterdayTemp.getMonth(), yesterdayTemp.getDate());

		var observableInfo = ko.observable();
		var observableHours = ko.observableArray();
		var observableLayers = ko.observableArray();
		var vm = {
			timeLines: observableHours,
			activityInfo: observableInfo,
			layers: observableLayers
		};
		ko.applyBindings(vm);
		_loadViewModel(vm, yesterday);
	}

	function _setFixedElementAttributes() {
		$('body').css('overflow', 'hidden');
		$('.asm-time-marker').css('width', timeLineMarkerWidth);
		$('.asm-sliding-schedules').css('width', (3 * 24 * pixelPerHours));
		$('.asm-timeline-line').css('width', (pixelPerHours - 1)); //"1" due to border size
	}

	function _loadViewModel(asmViewModel, yesterday) {
		Teleopti.MyTimeWeb.Ajax.Ajax({
			url: '/MyTime/Asm/Today', //todo: fix!
			dataType: "json",
			type: 'GET',
			data: { asmZero: yesterday.toJSON() }, //todo: fix!
			success: function (data) {
				asmViewModel.timeLines(data.Hours);
				var layers = _updateLayers(data.Layers);
				asmViewModel.layers(layers);
				_refresh(yesterday, asmViewModel, layers);
				$('.asm-outer-canvas').show();
				setInterval(function () {
					_refresh(yesterday, asmViewModel, layers);
				}, 1000 * refreshSeconds);
			},
			error: function () {
				alert('nope');
			}
		});
	}

	function _refresh(yesterday, asmViewModel, layers) {
		_moveTimeLineToNow(yesterday);
		_updateInfoCanvas(asmViewModel.activityInfo, layers);
	}

	function _updateLayers(layers) {
		var newLayers = new Array();
		$.each(layers, function (key, layer) {
			newLayers.push({ 'leftPx': ((layer.StartMinutesSinceAsmZero) * pixelPerHours / 60 + timeLineMarkerWidth) + 'px',
				'payload': layer.Payload,
				'backgroundColor': layer.Color,
				'paddingLeft': (layer.LengthInMinutes * pixelPerHours) / 60 + 'px',
				'startTimeText': layer.StartTimeText,
				'title': layer.StartTimeText + '-' + layer.EndTimeText + ' ' + layer.Payload
			});
		});
		return newLayers;
	}

	function _updateInfoCanvas(observableInfo, layers) {
		var model = new Array();
		var timeLineFixedPos = parseFloat($('.asm-time-marker').css('width'));
		var timelinePosition = timeLineFixedPos - parseFloat($(".asm-sliding-schedules").css('left'));
		$.each(layers, function (key, layer) {
			var startPos = parseInt(layer.leftPx);
			var endPos = startPos + parseInt(layer.paddingLeft);
			if (endPos > timelinePosition) {
				var active = false;
				if (startPos <= timelinePosition) {
					active = true;
				}
				var startText = layer.startTimeText;

				if (startPos - timelinePosition >= 24 * pixelPerHours) {
					startText += '+1';
				}

				model.push({ 'payload': layer.payload, 'time': startText, 'active': active });
			}
		});
		observableInfo(model);
	}

	function _moveTimeLineToNow(yesterday) {
		var slidingSchedules = $('.asm-sliding-schedules');
		var clientMsSince1970 = new Date().getTeleoptiTime();
		var msSinceStart = clientMsSince1970 - yesterday.getTime();
		var hoursSinceStart = msSinceStart / 1000 / 60 / 60;
		var startPixel = -(pixelPerHours * hoursSinceStart);
		slidingSchedules.css('left', (startPixel) + 'px');
	}

	return {
		Init: function () {
			_start();
		}
	};
})(jQuery);
