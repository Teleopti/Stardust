/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />

Teleopti.MyTimeWeb.Request.AddShiftTradeRequest = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;
	var layerCanvasPixelWidth = 700;
	var pixelPerMinute;

	function shiftTradeViewModel() {
		var self = this;

		self.hasWorkflowControlSet = ko.observable(false);
		self.timeLineLengthInMinutes = ko.observable(0);
		self.myScheduleLayers = ko.observableArray();

		self._createMyScheduleLayers = function (layers) {
			var newLayers = new Array();
			$.each(layers, function (key, layer) {
				newLayers.push(new shiftTradeLayerViewModel(layer));
			});
			self.myScheduleLayers(newLayers);
		};

		self.loadViewModel = function (date) {
			//console.log(date);
			ajax.Ajax({
				url: "Requests/ShiftTradeRequest",
				dataType: "json",
				type: 'GET',
				//beforeSend: _loading,
				data: {
					SelectedDate: date
				},
				success: function (data, textStatus, jqXHR) {
					self.hasWorkflowControlSet(!data.HasWorkflowControlSet);
					self.timeLineLengthInMinutes(data.TimeLineLengthInMinutes);
					self._createMyScheduleLayers(data.MySchedulelayers);
					//console.log(data);
				},
				error: function (err) {
					//console.log('Something went wrong here...');
					alert(err);
					//console.log(err);
				}
			});
		};

		self.timeLineLengthInMinutes.subscribe(function () {
			pixelPerMinute = layerCanvasPixelWidth / self.timeLineLengthInMinutes();
		});
	}

	function shiftTradeLayerViewModel(layer) {
		var self = this;

		self.payload = layer.Payload;
		self.backgroundColor = layer.Color;
		self.startTime = layer.StartTimeText;
		self.endTime = layer.EndTimeText;
		self.lengthInMinutes = layer.LengthInMinutes;
		self.leftPx = (layer.ElapsedMinutesSinceShiftStart * pixelPerMinute) + 'px';
		self.paddingLeft = (self.lengthInMinutes * pixelPerMinute) + 'px';
		self.title = ko.computed(function () {
			return self.startTime + '-' + self.endTime + ' ' + self.payload;
		});
	}

	function _init() {
		setShiftTradeRequestDate('2030-01-01'); //Just temporary
		vm = new shiftTradeViewModel();
		var elementToBind = $('#Request-add-shift-trade').get(0);
		ko.applyBindings(vm, elementToBind);
		_showContent();
	}

	function _showContent() {
		$('#Request-add-shift-trade-button')
			.click(function () {
				$('#Request-add-shift-trade')
					.show();

				vm.loadViewModel($('#Request-add-selected-date').val());
			})
			;
	}

	function setShiftTradeRequestDate(date) {
		$('#Request-add-selected-date')
			.val(date)
			;
	}

	return {
		Init: function () {
			_init();
		},
		SetShiftTradeRequestDate: function (date) {
			setShiftTradeRequestDate(date);
		}
	};

})(jQuery);