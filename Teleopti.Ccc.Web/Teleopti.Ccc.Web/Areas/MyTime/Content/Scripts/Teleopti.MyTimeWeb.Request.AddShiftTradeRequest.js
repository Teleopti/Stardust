/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
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
		self.requestedDate = ko.observable();
		self.selectedDate = ko.observable();
		self.missingWorkflowControlSet = ko.observable(false);
		self.noPossibleShiftTrades = ko.observable(false);
		self.timeLineLengthInMinutes = ko.observable(0);
		self.hours = ko.observableArray();
		self.mySchedule = ko.observable(new personScheduleViewModel());
		self.possibleTradeSchedules = ko.observableArray();
		self.pixelPerMinute = ko.computed(function () {
			return layerCanvasPixelWidth / self.timeLineLengthInMinutes();
		});

		self._createMySchedule = function (myScheduleObject) {
			var mappedlayers = ko.utils.arrayMap(myScheduleObject.ScheduleLayers, function (layer) {
				return new layerViewModel(layer, myScheduleObject.MinutesSinceTimeLineStart, self.pixelPerMinute());
			});
			self.mySchedule(new personScheduleViewModel(mappedlayers, myScheduleObject));
		};

		self._createPossibleTradeSchedules = function (possibleTradePersons) {
			var mappedPersonsSchedule = ko.utils.arrayMap(possibleTradePersons, function (personSchedule) {

				var mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function (layer) {
					return new layerViewModel(layer, personSchedule.MinutesSinceTimeLineStart, self.pixelPerMinute());
				});

				return new personScheduleViewModel(mappedLayers, personSchedule);
			});

			self.noPossibleShiftTrades(mappedPersonsSchedule.length == 0 ? true : false);
			self.possibleTradeSchedules(mappedPersonsSchedule);
		};

		self._createTimeLine = function (hours) {
			var arrayMap = ko.utils.arrayMap(hours, function (hour) {
				return new timeLineHourViewModel(hour, self);
			});
			self.hours(arrayMap);
			_positionTimeLineHourTexts();
		};

		self.requestedDate.subscribe(function (newValue) {
			if (newValue.diff(self.openPeriodStartDate) < 0) {
				self.selectedDate(moment(self.openPeriodStartDate));
			} else if (self.openPeriodEndDate.diff(newValue) < 0) {
				self.selectedDate(moment(self.openPeriodEndDate));
			} else {
				self.selectedDate(newValue);
			}
			self.loadSchedule();
		});

		self.loadPeriod = function () {
			ajax.Ajax({
				url: "Requests/ShiftTradeRequestPeriod",
				dataType: "json",
				type: 'GET',
				success: function (data, textStatus, jqXHR) {
					self.missingWorkflowControlSet(!data.HasWorkflowControlSet);
					if (data.HasWorkflowControlSet) {
						setDatePickerRange(data.OpenPeriodRelativeStart, data.OpenPeriodRelativeEnd);
						self.requestedDate(moment(self.now).add('days', data.OpenPeriodRelativeStart));
					} else {
						self.setScheduleLoadedReady();
					}
				}
			});
		};

		self.loadSchedule = function () {
			ajax.Ajax({
				url: "Requests/ShiftTradeRequestSchedule",
				dataType: "json",
				type: 'GET',
				data: { selectedDate: self.selectedDate().toDate().toJSON() },
				success: function (data, textStatus, jqXHR) {
					self.timeLineLengthInMinutes(data.TimeLineLengthInMinutes);
					self._createMySchedule(data.MySchedule);
					self._createPossibleTradeSchedules(data.PossibleTradePersons);
					self._createTimeLine(data.TimeLineHours);
					self.setScheduleLoadedReady();
				}
			});
		};

		self.nextDate = function () {
			self.requestedDate(moment(self.selectedDate()).add('days', 1));
		};

		self.previousDate = function () {
			self.requestedDate(moment(self.selectedDate()).add('days', -1));
		};

		self.loadedDateSwedishFormat = ko.observable();

		self.setScheduleLoadedReady = function () {
			self.loadedDateSwedishFormat(moment(self.requestedDate()).format('YYYY-MM-DD'));
		};
	}

	function personScheduleViewModel(layers, scheduleObject) {
		var self = this;
		var minutesSinceTimeLineStart = 0;
		var agentName = '';
		var dayOffText = '';
		var hasUnderlyingDayOff = false;
		if (scheduleObject) {
			agentName = scheduleObject.Name;
			minutesSinceTimeLineStart = scheduleObject.MinutesSinceTimeLineStart;
			dayOffText = scheduleObject.DayOffText;
			hasUnderlyingDayOff = scheduleObject.HasUnderlyingDayOff;
		}

		self.agentName = agentName;
		self.layers = layers;
		self.minutesSinceTimeLineStart = minutesSinceTimeLineStart;
		self.dayOffText = dayOffText;
		self.hasUnderlyingDayOff = ko.observable(hasUnderlyingDayOff);
		self.showDayOffStyle = function () {
			if (self.hasUnderlyingDayOff() == true | self.dayOffText.length > 0) {
				return true;
			}
			return false;
		};

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
			if (self.payload) {
				return self.startTime + '-' + self.endTime + ' ' + self.payload;
			}
			return '';
		});
	}

	function timeLineHourViewModel(hour, parentViewModel) {
		var self = this;
		var borderSize = 1;

		self.hourText = hour.HourText;
		self.lengthInMinutes = hour.LengthInMinutesToDisplay;
		self.leftPx = ko.observable('-8px');
		self.hourWidth = ko.computed(function () {
			return self.lengthInMinutes * parentViewModel.pixelPerMinute() - borderSize + 'px';
		});
	}

	function _init() {
		vm = new shiftTradeViewModel();
		var elementToBind = $('#Request-add-shift-trade').get(0);
		ko.applyBindings(vm, elementToBind);
		vm.loadPeriod();
	}
	function _initDatePicker() {
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

	function _openAddShiftTradeWindow() {
		Teleopti.MyTimeWeb.Request.RequestDetail.HideEditSection();
		_initDatePicker();
		$('#Request-add-shift-trade').show();
		_positionTimeLineHourTexts();
	}

	function _hideShiftTradeWindow() {
		$('#Request-add-shift-trade').hide();
	}
	
	function setShiftTradeRequestDate(date) {
		vm.loadedDateSwedishFormat(null); //make sure scenarios wait until requested date is bound
		vm.requestedDate(moment(date));
	}

	function _positionTimeLineHourTexts() {
		$('.shift-trade-timeline-number').each(function () {
			var leftPx = Math.round(this.offsetWidth / 2);
			if (leftPx > 0) {
				ko.dataFor(this).leftPx(-leftPx + 'px');
			}
		});
	}

	return {
		Init: function () {
			_init();
		},
		SetShiftTradeRequestDate: function (date) {
			setShiftTradeRequestDate(date);
		},
		OpenAddShiftTradeWindow: function () {
			_openAddShiftTradeWindow();
		},
		HideShiftTradeWindow: function () {
			_hideShiftTradeWindow();
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