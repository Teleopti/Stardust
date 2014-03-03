/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Content/moment/moment.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
	Teleopti.MyTimeWeb = {};
}
if (typeof (Teleopti.MyTimeWeb.Request) === 'undefined') {
	Teleopti.MyTimeWeb.Request = {};
}

Teleopti.MyTimeWeb.Request.AddShiftTradeRequest = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;

	function shiftTradeViewModel(sendRequestCallback) {
		var self = this;
		var layerCanvasPixelWidth = 700;

		self.now = null;
		self.weekStart = ko.observable(1); 
		self.openPeriodStartDate = ko.observable(moment().startOf('year').add('days',-1));
		self.openPeriodEndDate = ko.observable(moment().startOf('year').add('days', -1));
		self.requestedDate = ko.observable(moment().startOf('day'));
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
		self.requestedDateInternal = ko.observable(moment().startOf('day'));
		self.DatePickerFormat = ko.observable();
		var datePickerFormat = $('#Request-detail-datepicker-format').val().toUpperCase();
		self.DatePickerFormat(datePickerFormat);
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
			self.errorMessage('');
		};

		self.sendRequest = function () {
		    self.isSendEnabled(false);
		    sendRequestCallback(self);
		};

		self.cancelRequest = function () {
		    self.chooseAgent(null);
		};

		self._createTimeLine = function (hours) {
			var arrayMap = ko.utils.arrayMap(hours, function (hour) {
				return new Teleopti.MyTimeWeb.Request.TimeLineHourViewModel(hour, self);
			});
		    
			self.hours([]);
			self.hours.push.apply(self.hours, arrayMap);
			_positionTimeLineHourTexts();
		};

        self.requestedDate = ko.computed({
            read: function() {
                return self.requestedDateInternal();
            },
            write: function (value) {
                if (self.requestedDateInternal().diff(value) == 0) return;
                self.chooseAgent(null);
                self.requestedDateInternal(value);
                self.loadSchedule();
            }
        });

        self.nextDateValid = ko.computed(function () {
        	return self.openPeriodEndDate().diff(self.requestedDateInternal())>0;
		});

        self.previousDateValid = ko.computed(function () {
        	return self.requestedDateInternal().diff(self.openPeriodStartDate())>0;
		});

        self.isRequestedDateValid = function (date) {
        	if (date.diff(self.openPeriodStartDate()) < 0) {
	            return false;
        	} else if (self.openPeriodEndDate().diff(date) < 0) {
	            return false;
	        }
	        return true;
	    };

		self.loadPeriod = function (date) {
			ajax.Ajax({
				url: "Requests/ShiftTradeRequestPeriod",
				dataType: "json",
				type: 'GET',
				success: function (data, textStatus, jqXHR) {
					if (data.HasWorkflowControlSet) {
					    self.now = moment(new Date(data.NowYear, data.NowMonth-1, data.NowDay));
						setDatePickerRange(data.OpenPeriodRelativeStart, data.OpenPeriodRelativeEnd);
						var requestedDate = moment(self.now).add('days', data.OpenPeriodRelativeStart);
						if (date && Object.prototype.toString.call(date) === '[object Date]') {
							var md = moment(date);
							if (self.isRequestedDateValid(md)) {
							    requestedDate = md;
							}
						}
					    self.requestedDate(requestedDate);
					}
					else {
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
				data: { selectedDate: self.requestedDateInternal().toDate().toJSON() },
				beforeSend: function () {
					self.IsLoading(true);
				},
				success: function (data, textStatus, jqXHR) {
					self.timeLineLengthInMinutes(data.TimeLineLengthInMinutes);
					self._createMySchedule(data.MySchedule);
					self._createPossibleTradeSchedules(data.PossibleTradePersons);
					self._createTimeLine(data.TimeLineHours);
					self.setScheduleLoadedReady();
					self.isReadyLoaded(true);
				},
				complete: function() {
					self.IsLoading(false);
				}
			});
		};

		self.changeRequestedDate = function (movement) {
	        var date = moment(self.requestedDateInternal()).add('days', movement);
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
		var elementToBind = $('#Request-add-shift-trade')[0];
		if (elementToBind !== undefined) {
		    if ((vm || '') == '' || !ko.dataFor(elementToBind)) {
		    	vm = new shiftTradeViewModel(_saveNewShiftTrade);
		    	ko.applyBindings(vm, elementToBind);

			    _setWeekStart(vm);
		    }
		}
	}

	function _setWeekStart(vm) {
		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {
			vm.weekStart(data.WeekStart);
		});
	}

	function _saveNewShiftTrade(viewModel) {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequest",
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify({
			    Date: viewModel.requestedDateInternal().toDate().toJSON(),
			    Subject: viewModel.subject(),
			    Message: viewModel.message(),
			    PersonToId: viewModel.agentChoosed().personId
			}),
			success: function (data) {
			    viewModel.agentChoosed(null);
			    viewModel.isSendEnabled(true);
				_hideShiftTradeWindow();
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					viewModel.errorMessage(data.Errors.join('</br>'));
					viewModel.isSendEnabled(true);
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
    
	return {
		Init: function () {
		},
		SetShiftTradeRequestDate: function (date) {
			return setShiftTradeRequestDate(date);
		},
		OpenAddShiftTradeWindow: function (date) {
			_init();
		    vm.loadPeriod(date);
		    _openAddShiftTradeWindow();
		},
		HideShiftTradeWindow: function () {
			_hideShiftTradeWindow();
		},
		Dispose: function() {
		}
	};

})(jQuery);