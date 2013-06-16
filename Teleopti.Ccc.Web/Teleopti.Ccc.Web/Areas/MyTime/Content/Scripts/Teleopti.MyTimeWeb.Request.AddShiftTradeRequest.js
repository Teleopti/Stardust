/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Content/moment/moment.js" />

Teleopti.MyTimeWeb.Request.AddShiftTradeRequest = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;

	function shiftTradeViewModel() {
		var self = this;
		var layerCanvasPixelWidth = 700;

		self.now = null;
		self.openPeriodStartDate = ko.observable(moment().startOf('year').add('days',-1));
		self.openPeriodEndDate = ko.observable(moment().startOf('year').add('days', -1));
		self.requestedDate = ko.observable(moment().startOf('day'));
		self.selectedDate = ko.observable(moment().startOf('day'));
		self.missingWorkflowControlSet = ko.observable(true);
		self.noPossibleShiftTrades = ko.observable(false);
		self.timeLineLengthInMinutes = ko.observable(0);
		self.hours = ko.observableArray();
		self.mySchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleViewModel());
		self.possibleTradeSchedules = ko.observableArray();
		self.agentChoosed = ko.observable(null);
		self.isSendEnabled = ko.observable(true);
		self.IsLoading = ko.observable(false);
		self.errorMessage = ko.observable();
		self.isReadyLoaded = ko.observable(false);
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
				//rk - don't really like to put DOM stuff here...
				window.scrollTo(0, 0);
			}
			self.agentChoosed(agent);
			self.clearInputForm(); 
		};

		self.clearInputForm = function () {
			self.subject(undefined);
			self.message(undefined);
			self.errorMessage('');
			//ugly hack to fire back event that something happened
			/*setTimeout(function () { $("#Request-add-shift-trade-message-input").change(); }, 0);
			setTimeout(function () { $("#Request-add-shift-trade-subject-input").change(); }, 0);*/
		};

		self._createTimeLine = function (hours) {
			var arrayMap = ko.utils.arrayMap(hours, function (hour) {
				return new Teleopti.MyTimeWeb.Request.TimeLineHourViewModel(hour, self);
			});
		    
			self.hours([]);
			self.hours(arrayMap);
			_positionTimeLineHourTexts();
		};

		self.requestedDate.subscribe(function (newValue) {
		    if (self.selectedDate().diff(newValue) == 0) return;
		    self.chooseAgent(null);
            if (newValue.diff(self.openPeriodStartDate()) < 0) {
                if (self.selectedDate().diff(self.openPeriodStartDate()) == 0) return;
                self.selectedDate(self.openPeriodStartDate());
            } else if (self.openPeriodEndDate().diff(newValue) < 0) {
                if (self.selectedDate().diff(self.openPeriodEndDate()) == 0) return;
                self.selectedDate(self.openPeriodEndDate());
            } else {
                self.selectedDate(newValue);
            }
			self.loadSchedule();
		});

	    self.isRequestedDateValid = function(date) {
	        if (date.diff(self.openPeriodStartDate()) < 0) {
	            return false;
	        } else if (self.openPeriodEndDate().diff(date) < 0) {
	            return false;
	        }
	        return true;
	    };

		self.loadPeriod = function () {
			ajax.Ajax({
				url: "Requests/ShiftTradeRequestPeriod",
				dataType: "json",
				type: 'GET',
				success: function (data, textStatus, jqXHR) {
					if (data.HasWorkflowControlSet) {
					    self.now = moment(new Date(data.NowYear, data.NowMonth-1, data.NowDay));
						setDatePickerRange(data.OpenPeriodRelativeStart, data.OpenPeriodRelativeEnd);
						self.requestedDate(moment(self.now).add('days', data.OpenPeriodRelativeStart));
					} else {
					    self.setScheduleLoadedReady();
					    self.isReadyLoaded(true);
					}
					self.missingWorkflowControlSet(!data.HasWorkflowControlSet);
				}
			});
		};

		self.loadSchedule = function () {
			ajax.Ajax({
				url: "Requests/ShiftTradeRequestSchedule",
				dataType: "json",
				type: 'GET',
				data: { selectedDate: self.selectedDate().toDate().toJSON() },
				beforeSend: function () {
					self.IsLoading(true);
				},
				success: function (data, textStatus, jqXHR) {
					self.timeLineLengthInMinutes(data.TimeLineLengthInMinutes);
					self._createMySchedule(data.MySchedule);
					self._createPossibleTradeSchedules(data.PossibleTradePersons);
					self._createTimeLine(data.TimeLineHours);
					//self._createTimeLine([{ "HourText": "98", "LengthInMinutesToDisplay": 15 }, { "HourText": "99", "LengthInMinutesToDisplay": 15 }]);
					self.setScheduleLoadedReady();
					self.isReadyLoaded(true);
				},
				complete: function() {
					self.IsLoading(false);
				}
			});
		};

	    self.changeRequestedDate = function(movement) {
	        var date = moment(self.selectedDate()).add('days', movement);
	        if (self.isRequestedDateValid(date))
	            self.requestedDate(date);
	    };

		self.nextDate = function () {
		    self.changeRequestedDate(1);
		};

		self.previousDate = function () {
		    self.changeRequestedDate(-1);
		};

		self.loadedDateSwedishFormat = ko.observable();

		self.setScheduleLoadedReady = function () {
			self.loadedDateSwedishFormat(moment(self.requestedDate()).format('YYYY-MM-DD'));
		};
	}

	function _init() {
	    _unbind();
		var elementToBind = $('#Request-add-shift-trade').get(0);
		if (_hasPermission(elementToBind)) {
			vm = new shiftTradeViewModel();
			ko.applyBindings(vm, elementToBind);
			_initButtons();
		}
	}

	function _hasPermission(element) {
		return element!==undefined;
	}
	
	function _initButtons() {
		$('#Request-add-shift-trade-detail-section .send-button')
			.click(function () {
				vm.isSendEnabled(false);
				_saveNewShiftTrade();
			});
		$('#Request-add-shift-trade-detail-section .cancel-button')
			.click(function () {
				vm.chooseAgent(null);
			});
	}

	function _saveNewShiftTrade() {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequest",
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify({
				Date: vm.selectedDate().toDate().toJSON(),
				Subject: vm.subject(),
				Message: vm.message(),
				PersonToId: vm.agentChoosed().personId
			}),
			success: function (data) {
				vm.agentChoosed(null);
				vm.isSendEnabled(true);
				_hideShiftTradeWindow();
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					vm.errorMessage(data.Errors.join('</br>'));
					vm.isSendEnabled(true);
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	function setDatePickerRange(relativeStart, relativeEnd) {
		vm.openPeriodStartDate(moment(vm.now).add('days', relativeStart));
		vm.openPeriodEndDate(moment(vm.now).add('days', relativeEnd));
	}

	function _openAddShiftTradeWindow() {
	    Teleopti.MyTimeWeb.Request.RequestDetail.HideNewTextOrAbsenceRequestView();
		$('#Request-add-shift-trade').show();
	}

	function _hideShiftTradeWindow() {
		$('#Request-add-shift-trade').hide();
	}

	function setShiftTradeRequestDate(date) {
	    vm.isReadyLoaded(false);
		vm.loadedDateSwedishFormat(null); //make sure scenarios wait until requested date is bound
		vm.requestedDate(moment(date));
	    return vm.requestedDate().format('YYYY-MM-DD');
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
    
	function _unbind() {
	    vm = null;
        var element = $('#Request-add-shift-trade')[0];
        if (element) ko.cleanNode(element);
    }

	return {
		Init: function () {
		},
		SetShiftTradeRequestDate: function (date) {
			return setShiftTradeRequestDate(date);
		},
		OpenAddShiftTradeWindow: function () {
		    _init();
		    vm.loadPeriod();
			_openAddShiftTradeWindow();
		},
		HideShiftTradeWindow: function () {
			_hideShiftTradeWindow();
		},
		Dispose: function() {
		    _unbind();
		}
	};

})(jQuery);