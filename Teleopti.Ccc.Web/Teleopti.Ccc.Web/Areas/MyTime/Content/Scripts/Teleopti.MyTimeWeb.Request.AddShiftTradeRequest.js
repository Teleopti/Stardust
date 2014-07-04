/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="jquery.visible.js" />

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
		self.layerCanvasPixelWidth = ko.observable();

		self.now = null;
		self.weekStart = ko.observable(1); 
		self.openPeriodStartDate = ko.observable(moment().startOf('year').add('days',-1));
		self.openPeriodEndDate = ko.observable(moment().startOf('year').add('days', -1));
		self.requestedDate = ko.observable(moment().startOf('day'));
		self.missingWorkflowControlSet = ko.observable(true);
		
		self.noPossibleShiftTrades = ko.observable(false);
		self.hours = ko.observableArray();
		self.mySchedule = ko.observable(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel());
		self.possibleTradeSchedules = ko.observableArray();
		self.possibleTradeSchedulesRaw = [];
		self.agentChoosed = ko.observable(null);
		self.isSendEnabled = ko.observable(true);
		self.IsLoading = ko.observable(false);
		self.errorMessage = ko.observable();
		self.isReadyLoaded = ko.observable(false);
		self.requestedDateInternal = ko.observable(moment().startOf('day'));
		self.myTeamFilter = ko.observable(false);
		self.timeLineStartTime = ko.observable();
	    self.timeLineLengthInMinutes = ko.observable();
		self.DatePickerFormat = ko.observable();
		var datePickerFormat = $('#Request-detail-datepicker-format').val().toUpperCase();
		self.DatePickerFormat(datePickerFormat);
		self.IsLastPage = false;
	    self.availableTeams = ko.observableArray();
	    self.selectedTeamInternal = ko.observable();
	    self.missingMyTeam = ko.observable();
	    self.myTeamId = ko.observable();
		self.isDetailVisible = ko.computed(function () {
			if (self.agentChoosed() === null) {
				return false;
			}
			return true;
		});
		self.subject = ko.observable();
		self.message = ko.observable();

		self.sortByDate = function() {
			if (self.chooseHistorys().length > 1) {
				self.chooseHistorys.sort(function(a, b) {
					return a.selectedDate() > b.selectedDate();
				});
			}
		};
		self.isTradeForMultiDaysEnabled = ko.observable(false);
		self.chooseHistorys = ko.observableArray();
		self.requestedDates = ko.computed(function() {
			var dates = [];
			$.each(self.chooseHistorys(), function(index, chooseHistoryViewModel) {
				dates.push(chooseHistoryViewModel.selectedDate());
			});
			return dates;
		});
		self.selectedInternal = ko.observable(false);
		self.add = function () {
			var currentTrade = {
				date: self.requestedDate,
				hours: self.hours,
				mySchedule: self.mySchedule,
				tradedSchedule: null
			};
			$.each(self.possibleTradeSchedules(), function(index, schedule) {
				if (self.agentChoosed().agentName == schedule.agentName) {
					currentTrade.tradedSchedule = schedule;
					var currentChooseView = new Teleopti.MyTimeWeb.Request.ChooseHistoryViewModel(currentTrade);
					self.chooseHistorys.push(currentChooseView);
					self.sortByDate();
					self.selectedInternal(true);
					self.isSendEnabled(true);
				}
			});
		};
		
		self.remove = function (chooseHistoryViewModel) {
			var date = chooseHistoryViewModel.selectedDate;
			var dayToDelete = $.grep(self.chooseHistorys(), function (e) { return e.selectedDate == date; })[0];
			self.chooseHistorys.remove(dayToDelete);

			if (chooseHistoryViewModel.selectedDateInFormat() == self.requestedDate().format(self.DatePickerFormat())) {
				self.selectedInternal(false);
			}
			if (self.chooseHistorys().length < 1) self.isSendEnabled(false);
		};

		self.isAddVisible = ko.computed(function(){
			var addVisible = false;
			if (self.isDetailVisible() && !self.selectedInternal()) {
				addVisible = true;
			}
			return addVisible;
		});

		self.isShowList = ko.computed(function () {
			var showList = false;
			if (self.isDetailVisible() && self.chooseHistorys().length > 0) {
				showList = true;
			}
			return showList;
		});

		self.tradeCartHeight = ko.computed(function() {
			var oneCandidateDayHeight = 110;
			return oneCandidateDayHeight * self.chooseHistorys().length + 'px';
		});

	    self.setTimeLineLengthInMinutes = function(firstHour, lastHour) {
	        self.timeLineStartTime(firstHour);
	        self.timeLineLengthInMinutes(lastHour.diff(firstHour, 'minutes'));
	    };
	    self.pixelPerMinute = ko.computed(function () {
	    	return self.layerCanvasPixelWidth() / self.timeLineLengthInMinutes();
		});
        
		self._createMySchedule = function (myScheduleObject) {
		    var mappedlayers = [];
		    if (myScheduleObject != null) {
		        mappedlayers = ko.utils.arrayMap(myScheduleObject.ScheduleLayers, function (layer) {
		            var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
		            return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart);
		        });
		    }
		    self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, myScheduleObject));
		};

		self._createPossibleTradeSchedules = function (possibleTradeSchedules) {
			self.possibleTradeSchedules.removeAll();
			var mappedPersonsSchedule = ko.utils.arrayMap(possibleTradeSchedules, function (personSchedule) {
				
			    var mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function (layer) {
			    	var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
			    	return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart);;
			    });
			    var model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers, personSchedule);
				 self.possibleTradeSchedules.push(model);
				 return model;
			});

			self.noPossibleShiftTrades(mappedPersonsSchedule.length == 0 ? true : false);
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
				setSendEnableStatus(self);
			}
			self.agentChoosed(agent);
			self.errorMessage('');
		};

		self.sendRequest = function () {
		    self.isSendEnabled(false);
		    sendRequestCallback(self);
		    self.chooseAgent(null);
			self.chooseHistorys.removeAll();
		};

		self.cancelRequest = function () {
			self.chooseAgent(null);
			self.chooseHistorys.removeAll();
			self.selectedInternal(false);
		};

		self._createTimeLine = function (hours) {
		    var firstTimeLineHour = moment(hours[0].StartTime);
		    var lastTimeLineHour = moment(hours[hours.length - 1].EndTime);
		    self.setTimeLineLengthInMinutes(firstTimeLineHour, lastTimeLineHour);
		    self.hours([]);
			if (hours.length < 18) {
				var arrayMap = ko.utils.arrayMap(hours, function(hour) {
					return new Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel(hour, self);
				});

				self.hours.push.apply(self.hours, arrayMap);
			} else {
				for (var i = 0; i < hours.length; i++) {
					var newHour = new Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel(hours[i], self);
					self.hours.push(newHour);
					i++;
				}
			}
		};

        self.requestedDate = ko.computed({
        	read: function () {
                return self.requestedDateInternal();
            },
        	write: function (value) {
        		//remove old layer's tooltip if it still exist
        		$("[class='tooltip fade top in']").remove();
        		
                if (self.requestedDateInternal().diff(value) == 0) return;
                self.prepareLoad();
                self.requestedDateInternal(value);
                
                self.loadMyTeamId();
            }
        });
	    
        self.selectedTeam = ko.computed({
            read: function () {
                return self.selectedTeamInternal();
            },
            write: function (value) {
            	if (value != null && self.selectedTeamInternal() != null && value != self.selectedTeamInternal()) {
            		self.chooseAgent(null);
		            self.chooseHistorys.removeAll();
	            }
                self.selectedTeamInternal(value);
                if (value == null) return;
                self.prepareLoad();
                self.loadSchedule();
            }
        });

        self.nextDateValid = ko.computed(function () {
        	return self.openPeriodEndDate().diff(self.requestedDateInternal())>0;
		});

        self.previousDateValid = ko.computed(function () {
        	return self.requestedDateInternal().diff(self.openPeriodStartDate())>0;
		});

	    self.prepareLoad = function() {
	        self.possibleTradeSchedulesRaw = [];
	        self.IsLastPage = false;
			if (self.agentChoosed() != null) {
				self.keepSelectedAgentVisible();
			}
	        else
				self.chooseAgent(null);
	        self.IsLoading(false);
	    };

        self.isRequestedDateValid = function (date) {
        	if (date.diff(self.openPeriodStartDate()) < 0) {
	            return false;
        	} else if (self.openPeriodEndDate().diff(date) < 0) {
	            return false;
	        }
	        return true;
        };
        
		self.keepSelectedAgentVisible = function() {
			if (self.agentChoosed() != null && self.possibleTradeSchedules() != null) {
				$.each(self.possibleTradeSchedules(), function (index, value) {
					value.isVisible(value.agentName == self.agentChoosed().agentName);
				});
			}

			var isAddAvaiable = false;
			$.each(self.chooseHistorys(), function (index, chooseHistoryViewModel) {
				if (self.requestedDateInternal().format(self.DatePickerFormat()) == chooseHistoryViewModel.selectedDateInFormat()) {
					isAddAvaiable = true;
					return false;
				}
			});
			self.selectedInternal(isAddAvaiable);
		}

        self.loadMyTeamId = function () {
            ajax.Ajax({
                url: "Requests/ShiftTradeRequestMyTeam",
                dataType: "json",
                type: 'GET',
                contentType: 'application/json; charset=utf-8',
                data: {
                	selectedDate: self.requestedDateInternal().format($('#Request-detail-datepicker-format').val().toUpperCase())
                },
                beforeSend: function () {
                    //self.IsLoading(true);
                },
                success: function (data, textStatus, jqXHR) {
                    if (!data) {
                        self.myTeamId(undefined);
                        self.missingMyTeam(true);
                        self.isReadyLoaded(true);
                        return;
                    }
                    self.missingMyTeam(false);
                    self.myTeamId(data);
                    self.loadTeams();
                    
                },
                error: function (e) {
                    //console.log(e);
                },
                complete: function () {
                    //self.IsLoading(false);
                }
            });
        };
	    
        self.loadTeams = function () {
            var teamToSelect = self.selectedTeamInternal() ? self.selectedTeamInternal() : self.myTeamId();

            ajax.Ajax({
            	url: "Team/TeamsForShiftTrade",
                dataType: "json",
                type: 'GET',
                contentType: 'application/json; charset=utf-8',
                data: {
                	date: self.requestedDateInternal().format($('#Request-detail-datepicker-format').val().toUpperCase())
                },
                beforeSend: function () {
                    //self.IsLoading(true);
                },
                success: function (data, textStatus, jqXHR) {
                    self.selectedTeam(null);
                    self.availableTeams(data);
                    self.selectedTeam(teamToSelect);
                    
                },
                error: function (e) {
                    //console.log(e);
                },
                complete: function () {
                    //self.IsLoading(false);
                }
            });
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
					    self.isReadyLoaded(true);
					}
					self.missingWorkflowControlSet(!data.HasWorkflowControlSet);
				}
			});
		};

		self.loadSchedule = function () {
			if (self.IsLoading()) return;
			var skip = self.possibleTradeSchedulesRaw.length;
			var take = 50;
			if (self.IsLastPage) return;
			ajax.Ajax({
				url: "Requests/ShiftTradeRequestSchedule",
				dataType: "json",
				type: 'GET',
				contentType: 'application/json; charset=utf-8',
				data: {
					selectedDate: self.requestedDateInternal().format($('#Request-detail-datepicker-format').val().toUpperCase()),
				    teamId: self.selectedTeamInternal(),
				    Take: take,
				    Skip: skip
				},
				beforeSend: function () {
					self.IsLoading(true);
				},
				success: function (data, textStatus, jqXHR) {
				    self._createTimeLine(data.TimeLineHours);
				    self._createMySchedule(data.MySchedule);
					self.IsLastPage = data.IsLastPage;
				    $.each(data.PossibleTradeSchedules, function (i, item) {
				    	self.possibleTradeSchedulesRaw.push(item);
				    });
					
				    self._createPossibleTradeSchedules(self.possibleTradeSchedulesRaw);
					self.keepSelectedAgentVisible();
					self.isReadyLoaded(true);

					// Redraw layers after data loaded
					_redrawLayers();
				},
				error: function(e) {
				    //console.log(e);
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

		self.featureCheck = function () {
			ajax.Ajax({
				url: "../ToggleHandler/IsEnabled?toggle=Request_ShiftTradeRequestForMoreDays_20918",
				success: function (data) {
					if (data.IsEnabled) {
						self.isTradeForMultiDaysEnabled(true);
					}
				}
			});
		}
	}

	function _redrawLayers() {
		var canvasWidth;

		if (vm.isReadyLoaded()) {
			canvasWidth = $("td.shift-trade-possible-trade-schedule").width();
		} else {
			var containerWidth = $("#Request-add-shift-trade").width();
			var nameCellWidth = $("td.shift-trade-agent-name").width();
			canvasWidth = containerWidth - nameCellWidth;
		}

		vm.layerCanvasPixelWidth(canvasWidth);

		if (vm.mySchedule() != undefined) {
			$.each(vm.mySchedule().layers, function (index, vmScheduleAddShiftTrade) {
				vmScheduleAddShiftTrade.pixelPerMinute(vm.pixelPerMinute());
			});
		}

		if (vm.possibleTradeSchedules() != undefined) {
			$.each(vm.possibleTradeSchedules(), function (index, vmPersonScheduleAddShiftTrade) {
				$.each(vmPersonScheduleAddShiftTrade.layers, function (index, vmScheduleAddShiftTrade) {
					vmScheduleAddShiftTrade.pixelPerMinute(vm.pixelPerMinute());
				});
			});
		}
	}

	function _init() {
		var elementToBind = $('#Request-add-shift-trade')[0];
		if (elementToBind !== undefined) {
			if ((vm || '') == '') {
				vm = new shiftTradeViewModel(_saveNewShiftTrade);
				vm.featureCheck();
				ko.applyBindings(vm, elementToBind);
			}
			vm.chooseAgent(null);
			_setWeekStart(vm);
		}

		$(window).resize(function () {
			_redrawLayers();
		});
	}

	function _setWeekStart(vm) {
		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {
			vm.weekStart(data.WeekStart);
		});
	}

	function setSendEnableStatus(viewModel) {
		if (viewModel.isTradeForMultiDaysEnabled() && viewModel.chooseHistorys().length < 1) {
			viewModel.isSendEnabled(false);
		} else {
			viewModel.isSendEnabled(true);
		}
	}
	
	function _saveNewShiftTrade(viewModel) {
		ajax.Ajax({
			url: "Requests/ShiftTradeRequest",
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			data: JSON.stringify({
				Dates: viewModel.requestedDates(),
				Subject: viewModel.subject(),
				Message: viewModel.message(),
				PersonToId: viewModel.agentChoosed().personId
			}),
			success: function (data) {
				viewModel.agentChoosed(null);
				setSendEnableStatus(viewModel);
				_hideShiftTradeWindow();
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					viewModel.errorMessage(data.Errors.join('</br>'));
					setSendEnableStatus(viewModel);
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
		vm.requestedDate(moment(date));
	    return vm.requestedDate().format('YYYY-MM-DD');
	}

	function _positionTimeLineHourTexts() {
	    $('.shift-trade-label').each(function () {
			var leftPx = Math.round(this.offsetWidth / 2);
			if (leftPx > 0) {
				ko.dataFor(this).leftPos(-leftPx + 'px');
			}
		});
		//_initAgentNameOverflow();
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



	function _cleanUp() {
		var addShiftTradeElement = $('#Request-add-shift-trade')[0];
		if (addShiftTradeElement) {
			ko.cleanNode(addShiftTradeElement);
		}
		vm = null;
	}
	return {
		Init: function () {
			vm = '';
			initScrollPaging();
		},
		SetShiftTradeRequestDate: function (date) {
			return setShiftTradeRequestDate(date);
		},
		OpenAddShiftTradeWindow: function (date) {
			if (vm.chooseHistorys != undefined) {
				vm.chooseHistorys.removeAll();
			}
			_init();
		    vm.loadPeriod(date);
		    _openAddShiftTradeWindow();
		},
		HideShiftTradeWindow: function () {
			_hideShiftTradeWindow();
		},
		Dispose: function () {
			_cleanUp();
		}
	};

	function initScrollPaging() {
		$(window).scroll(loadAPageIfRequired);
	}
	
	function loadAPageIfRequired() {

	    if ($('#Request-add-shift-trade').filter(':visible').length == 0)
	    	return;
		
	    $('#tooltipContainer').each(function (i, el) {
			// Is this element visible onscreen?
			// LoadMore
			var elem = $(el);
			if (elem.visible(true)) {
				vm.loadSchedule();
			}	
		});
	}
	
})(jQuery);