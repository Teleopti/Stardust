﻿/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="~/Content/Scripts/jquery-1.8.3-vsdoc.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.0.js"/>
/// <reference path="~/Content/moment/moment.js" />

Teleopti.MyTimeWeb.Request.AddShiftTradeRequest = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;

	function shiftTradeViewModel() {
		var self = this;
		var layerCanvasPixelWidth = 700;

		self.now = moment(new Date().getTeleoptiTime()).startOf('day');
		self.openPeriodStartDate = null;
		self.openPeriodEndDate = null;
		self.selectedDate = ko.observable();
		self.missingWorkflowControlSet = ko.observable(true);
		self.timeLineLengthInMinutes = ko.observable(0);
		self.hours = ko.observableArray();
		self.mySchedule = ko.observable(new scheduleViewModel());
		self.possibleTradeSchedules = ko.observableArray();
		self.pixelPerMinute = ko.computed(function () {
			return layerCanvasPixelWidth / self.timeLineLengthInMinutes();
		});

		self._createMySchedule = function (myScheduleObject) {
			var mappedlayers = ko.utils.arrayMap(myScheduleObject.ScheduleLayers, function (layer) {
				return new layerViewModel(layer, myScheduleObject.MinutesSinceTimeLineStart, self.pixelPerMinute());
			});
			self.mySchedule(new scheduleViewModel(mappedlayers, myScheduleObject));
		};

		self._createPossibleTradeSchedules = function (possibleTradePersons) {
			var mappedPersonsSchedule = ko.utils.arrayMap(possibleTradePersons, function (personSchedule) {

				var mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function (layer) {
					return new layerViewModel(layer, personSchedule.MinutesSinceTimeLineStart, self.pixelPerMinute());
				});

				return new scheduleViewModel(mappedLayers, personSchedule);
			});

			self.possibleTradeSchedules(mappedPersonsSchedule);
		};

		self._createTimeLine = function (hours) {
			var arrayMap = ko.utils.arrayMap(hours, function (hour) {
				return new timeLineHourViewModel(hour, self);
			});
			self.hours(arrayMap);
		};

		self.selectedDate.subscribe(function () {
			if (self.selectedDate().diff(self.openPeriodStartDate) < 0) {
				self.selectedDate(moment(self.openPeriodStartDate));
				return;
			}
			if (self.openPeriodEndDate.diff(self.selectedDate()) < 0) {
				self.selectedDate(moment(self.openPeriodEndDate));
				return;
			}
			self.loadSchedule();
		});

		self.loadPeriod = function () {
			ajax.Ajax({
				url: "Requests/ShiftTradeRequestPeriod",
				dataType: "json",
				type: 'GET',
				//beforeSend: _loading,
				success: function (data, textStatus, jqXHR) {
					self.missingWorkflowControlSet(!data.HasWorkflowControlSet);
					if (data.HasWorkflowControlSet) {
						setDatePickerRange(data.OpenPeriodRelativeStart, data.OpenPeriodRelativeEnd);
						self.selectedDate(moment(self.now).add('days', data.OpenPeriodRelativeStart));
					}
				},
				error: function (err) {
					alert("error!");
					//console.log(err);
				}
			});
		};

		self.loadSchedule = function () {
			ajax.Ajax({
				url: "Requests/ShiftTradeRequestSchedule",
				dataType: "json",
				type: 'GET',
				//beforeSend: _loading,
				data: { selectedDate: self.selectedDate().toDate().toJSON() },
				success: function (data, textStatus, jqXHR) {
					self.timeLineLengthInMinutes(data.TimeLineLengthInMinutes);
					self._createMySchedule(data.MySchedule);
					self._createPossibleTradeSchedules(data.PossibleTradePersons);
					self._createTimeLine(data.TimeLineHours);
					//console.log(data);
				},
				error: function (err) {
					alert("error!");
					//console.log(err);
				}
			});
		};

		self.nextDate = function () {
			self.selectedDate(moment(self.selectedDate()).add('days', 1));
		};

		self.previousDate = function () {
			self.selectedDate(moment(self.selectedDate()).add('days', -1));
		};
	}

	function scheduleViewModel(layers, scheduleObject) {
		var self = this;
		var minutesSinceTimeLineStart = 0;
		var agentName = '';
		if (scheduleObject) {
			agentName = scheduleObject.Name;
			minutesSinceTimeLineStart = scheduleObject.MinutesSinceTimeLineStart;
		}

		self.agentName = agentName;
		self.layers = layers;
		self.minutesSinceTimeLineStart = minutesSinceTimeLineStart;

	}

	function layerViewModel(layer, minutesSinceTimeLineStart, pixelPerMinute) {
		var self = this;

		self.payload = layer.Payload;
		self.backgroundColor = layer.Color;
		self.startTime = layer.StartTimeText;
		self.endTime = layer.EndTimeText;
		self.lengthInMinutes = layer.LengthInMinutes;
		self.leftPx = ko.computed(function () {
			var timeLineoffset = minutesSinceTimeLineStart;
			return (layer.ElapsedMinutesSinceShiftStart + timeLineoffset) * pixelPerMinute + 'px';
		});
		self.paddingLeft = ko.computed(function () {
			return self.lengthInMinutes * pixelPerMinute + 'px';
		});
		self.title = ko.computed(function () {
			return self.startTime + '-' + self.endTime + ' ' + self.payload;
		});
	}

	function timeLineHourViewModel(hour, parentViewModel) {
		var self = this;
		var borderSize = 1;

		self.hourText = hour.HourText;
		self.lengthInMinutes = hour.LengthInMinutesToDisplay;
		self.width = ko.computed(function () {
			return self.lengthInMinutes * parentViewModel.pixelPerMinute() - borderSize + 'px';
		});
	}

	function _init() {
		vm = new shiftTradeViewModel();
		var elementToBind = $('#Request-add-shift-trade').get(0);
		ko.applyBindings(vm, elementToBind);
		bindClickToOpenShiftTrade();
	}
	function _initDatePicker() {
		$('.shift-trade-add-datepicker').textbox()

		$('.shift-trade-add-previous-date').button({
					icons: {
						primary: "ui-icon-triangle-1-w"
					},
					text: false
				});
		$('.shift-trade-add-next-date').button({
			icons: {
				primary: "ui-icon-triangle-1-e"
			},
			text: false
		});
	}

	function setDatePickerRange(relativeStart, relativeEnd) {
		vm.openPeriodStartDate = moment(vm.now).add('days', relativeStart);
		vm.openPeriodEndDate = moment(vm.now).add('days', relativeEnd);

		var element = $('.shift-trade-add-datepicker');
		element.datepicker("option", "minDate", vm.openPeriodStartDate.toDate());
		element.datepicker("option", "maxDate", vm.openPeriodEndDate.toDate());
	}

	function bindClickToOpenShiftTrade() {
		$('#Request-add-shift-trade-button')
			.click(function () {
				$('#Request-add-shift-trade')
					.show();

				_initDatePicker();
				vm.loadPeriod();
			});
	}

	function setShiftTradeRequestDate(date) {
		vm.selectedDate(moment(date));
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
			observable(moment($(element).datepicker("getDate")));
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
			$(element).datepicker("setDate", new Date(value));
		}
	}
};