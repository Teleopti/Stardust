/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />
/// <reference path="~/Content/Scripts/date.js" />

Teleopti.MyTimeWeb.Request.AddShiftTradeRequest = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;

	function shiftTradeViewModel() {
		var self = this;
		var layerCanvasPixelWidth = 700;

		self.now = new Date(new Date().getTeleoptiTime()).clearTime();
		self.openPeriodStartDate = null;
		self.openPeriodEndDate = null;
		self.selectedDate = ko.observable();
		self.hasWorkflowControlSet = ko.observable(false);
		self.timeLineLengthInMinutes = ko.observable(0);
		self.myScheduleLayers = ko.observableArray();

		self._createMyScheduleLayers = function (layers) {
			var arrayMap = ko.utils.arrayMap(layers, function (layer) {
				return new shiftTradeLayerViewModel(layer, self);
			});
			self.myScheduleLayers(arrayMap);
		};

		self.pixelPerMinute = ko.computed(function () {
			return layerCanvasPixelWidth / self.timeLineLengthInMinutes();
		});

		self.selectedDate.subscribe(function () {
			if (Date.compare(self.selectedDate(), self.openPeriodStartDate) == -1) {
				self.selectedDate(new Date(self.openPeriodStartDate));
				return;
			}
			if (Date.compare(self.openPeriodEndDate, self.selectedDate()) == -1) {
				self.selectedDate(new Date(self.openPeriodEndDate));
				return;
			}
			self.loadSchedule(self.selectedDate());
		});

		self.loadPeriod = function () {
			ajax.Ajax({
				url: "Requests/ShiftTradeRequestPeriod",
				dataType: "json",
				type: 'GET',
				//beforeSend: _loading,
				success: function (data, textStatus, jqXHR) {
					self.hasWorkflowControlSet(!data.HasWorkflowControlSet);
					setDatePickerRange(data.OpenPeriodRelativeStart, data.OpenPeriodRelativeEnd);
					self.selectedDate(new Date(self.now).addDays(data.OpenPeriodRelativeStart));
				},
				error: function (err) {
					alert("error!");
					//console.log(err);
				}
			});
		};

		self.loadSchedule = function (date) {
			ajax.Ajax({
				url: "Requests/ShiftTradeRequestSchedule",
				dataType: "json",
				type: 'GET',
				//beforeSend: _loading,
				data: { selectedDate: vm.selectedDate().toJSON() },
				success: function (data, textStatus, jqXHR) {
					self.timeLineLengthInMinutes(data.TimeLineLengthInMinutes);
					self._createMyScheduleLayers(data.MyScheduleLayers);
				},
				error: function (err) {
					alert("error!");
					//console.log(err);
				}
			});
		};

		self.nextDate = function () {
			self.selectedDate(self.selectedDate().addDays(1));
		};

		self.previousDate = function () {
			self.selectedDate(self.selectedDate().addDays(-1));
		};
	}

	function shiftTradeLayerViewModel(layer, parentViewModel) {
		var self = this;

		self.payload = layer.Payload;
		self.backgroundColor = layer.Color;
		self.startTime = layer.StartTimeText;
		self.endTime = layer.EndTimeText;
		self.lengthInMinutes = layer.LengthInMinutes;
		self.leftPx = ko.computed(function () {
			return layer.ElapsedMinutesSinceShiftStart * parentViewModel.pixelPerMinute() + 'px';
		});
		self.paddingLeft = ko.computed(function () {
			return self.lengthInMinutes * parentViewModel.pixelPerMinute() + 'px';
		});
		self.title = ko.computed(function () {
			return self.startTime + '-' + self.endTime + ' ' + self.payload;
		});
	}

	function _init() {
		vm = new shiftTradeViewModel();
		var elementToBind = $('#Request-add-shift-trade').get(0);
		ko.applyBindings(vm, elementToBind);
		bindClickToOpenShiftTrade();
	}

	function setDatePickerRange(relativeStart, relativeEnd) {
		vm.openPeriodStartDate = new Date(vm.now).addDays(relativeStart);
		vm.openPeriodEndDate = new Date(vm.now).addDays(relativeEnd);

		var element = $('#Request-add-shift-trade-datepicker');
		element.datepicker("option", "minDate", vm.openPeriodStartDate);
		element.datepicker("option", "maxDate", vm.openPeriodEndDate);
	}

	function bindClickToOpenShiftTrade() {
		$('#Request-add-shift-trade-button')
			.click(function () {
				$('#Request-add-shift-trade')
					.show();

				vm.loadPeriod();
			});
	}

	function setShiftTradeRequestDate(date) {
		vm.selectedDate(Date.parse(date));
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

ko.bindingHandlers.datepicker = {
	init: function (element, valueAccessor, allBindingsAccessor) {
		//initialize datepicker with some optional options
		var options = allBindingsAccessor().datepickerOptions || { showAnim: 'slideDown' };
		$(element).datepicker(options);

		//handle the field changing
		ko.utils.registerEventHandler(element, "change", function () {
			var observable = valueAccessor();
			observable($(element).datepicker("getDate"));
		});

		//handle disposal (if KO removes by the template binding)
		ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
			$(element).datepicker("destroy");
		});

	},
	update: function (element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor()),
            current = $(element).datepicker("getDate");
		if (value - current !== 0) {
			$(element).datepicker("setDate", value);
		}
	}
};