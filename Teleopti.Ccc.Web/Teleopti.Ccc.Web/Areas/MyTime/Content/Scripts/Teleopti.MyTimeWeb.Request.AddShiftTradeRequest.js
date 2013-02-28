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
		self.mySchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleViewModel());
		self.possibleTradeSchedules = ko.observableArray();
		self.agentChoosed = ko.observable(null);
		self.isDetailVisible = ko.computed(function () {
			if (self.agentChoosed() === null) {
				return false;
			}
			return true;
		});
		self.subject = ko.observable();
		self.message = ko.observable();
		self.pixelPerMinute = ko.computed(function () {
			return layerCanvasPixelWidth / self.timeLineLengthInMinutes();
		});

		self._createMySchedule = function (myScheduleObject) {
			var mappedlayers = ko.utils.arrayMap(myScheduleObject.ScheduleLayers, function (layer) {
				return new Teleopti.MyTimeWeb.Request.LayerViewModel(layer, myScheduleObject.MinutesSinceTimeLineStart, self.pixelPerMinute());
			});
			self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleViewModel(mappedlayers, myScheduleObject));
		};

		self._createPossibleTradeSchedules = function (possibleTradePersons) {
			var mappedPersonsSchedule = ko.utils.arrayMap(possibleTradePersons, function (personSchedule) {

				var mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function (layer) {
					return new Teleopti.MyTimeWeb.Request.LayerViewModel(layer, personSchedule.MinutesSinceTimeLineStart, self.pixelPerMinute());
				});

				return new Teleopti.MyTimeWeb.Request.PersonScheduleViewModel(mappedLayers, personSchedule);
			});

			self.noPossibleShiftTrades(mappedPersonsSchedule.length == 0 ? true : false);
			self.possibleTradeSchedules(mappedPersonsSchedule);
		};

		self.chooseAgent = function (agent) {
			//hide or show all agents
			$.each(self.possibleTradeSchedules(), function (index, value) {
				value.isVisible(agent == null);
			});
			if (agent != null) {
				agent.isVisible(true);
			}
			self.agentChoosed(agent);
		};

		self._createTimeLine = function (hours) {
			var arrayMap = ko.utils.arrayMap(hours, function (hour) {
				return new Teleopti.MyTimeWeb.Request.TimeLineHourViewModel(hour, self);
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

	function _init() {
		vm = new shiftTradeViewModel();
		var elementToBind = $('#Request-add-shift-trade').get(0);
		ko.applyBindings(vm, elementToBind);
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

	function _initLabels() {
		$('#Request-add-shift-trade-detail-section input[type=text], #Request-add-shift-trade-detail-section textarea')
			.labeledinput()
			;
	}

	function _initButtons() {
		$('#Request-add-shift-trade-detail-section .send-button')
			.button()
			.click(function () {
				_saveNewShiftTrade();
				_hideShiftTradeWindow();
			});
		$('#Request-add-shift-trade-detail-section .cancel-button')
			.button()
			.click(function () {
				vm.chooseAgent(null);
			});
	}

	function _saveNewShiftTrade() {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequest",
			dataType: "json",
			type: 'POST',
			data: {
				Date: vm.selectedDate().toDate().toJSON(),
				Subject: vm.subject(),
				Message: vm.message(),
				PersonToId: vm.agentChoosed().personId
			},
			success: function () {
				vm.agentChoosed(null);
			}
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
		_initLabels();
		_initButtons();
		$('#Request-add-shift-trade').show();
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
		_initAgentNameOverflow();
	}

	function _initAgentNameOverflow() {
		$('.shift-trade-agent-name')
			.hoverIntent({
				interval: 200,
				timeout: 200,
				over: function () {
					if ($(this).hasHiddenContent())
						$(this).addClass('shift-trade-agent-name-hover');
				},
				out: function () {
					$(this).removeClass('shift-trade-agent-name-hover');
				}
			})
	    ;
	}

	return {
		Init: function () {
			_init();
		},
		SetShiftTradeRequestDate: function (date) {
			setShiftTradeRequestDate(date);
		},
		OpenAddShiftTradeWindow: function () {
			vm.loadPeriod();
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

		//handle the field keydown for enter key
		ko.utils.registerEventHandler(element, "keydown", function (key) {
			if (key.keyCode == 13) {
				var observable = valueAccessor();
				observable(moment($(element).datepicker("getDate")));
			}
		});

		//handle disposal (if KO removes by the template binding)
		ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
			$(element).datepicker("destroy");
		});

	},
	update: function (element, valueAccessor) {
		var value = ko.utils.unwrapObservable(valueAccessor());
		$(element).datepicker("setDate", new Date(value));
	}
};