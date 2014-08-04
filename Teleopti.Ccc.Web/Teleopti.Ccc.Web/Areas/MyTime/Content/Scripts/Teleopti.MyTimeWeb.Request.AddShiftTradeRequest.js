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
		self.openPeriodStartDate = ko.observable(moment().startOf('year').add('days', -1));
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
		self.availableTeams = ko.observableArray();
		self.selectedTeamInternal = ko.observable();
		self.missingMyTeam = ko.observable();
		self.myTeamId = ko.observable();
		self.selectedPageIndex = ko.observable(1);
		self.pageCount = ko.observable(1);
		self.displayedPages = ko.observableArray();
		self.isMore = ko.observable(false);
		self.isPreviousMore = ko.observable(false);
		self.isPageVisible = ko.observable(true);
		self.filterTimeList =ko.observableArray();

		self.isDetailVisible = ko.computed(function() {
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
		self.isPossibleSchedulesForAllEnabled = ko.observable(false);
		self.availableAllTeamIds = ko.observableArray();
		self.isTradeForMultiDaysEnabled = ko.observable(false);
		self.isFilterByTimeEnabled = ko.observable(false);
		self.chooseHistorys = ko.observableArray();
		self.requestedDates = ko.computed(function() {
			var dates = [];

			if (self.isTradeForMultiDaysEnabled()) {
				$.each(self.chooseHistorys(), function(index, chooseHistoryViewModel) {
					dates.push(chooseHistoryViewModel.selectedDateInFormat());
				});
			} else {
				dates.push(self.requestedDateInternal().format($('#Request-detail-datepicker-format').val().toUpperCase()));
			}

			return dates;
		});
		self.selectedInternal = ko.observable(false);
		self.add = function() {
			var currentTrade = {
				date: self.requestedDate(),
				hours: self.hours(),
				mySchedule: null,
				tradedSchedule: null
			};
			var scheduleStartTime;
			var tradedScheduleModel;
			if (self.selectedInternal()) {
				return;
			}
			$.each(self.possibleTradeSchedules(), function(index, schedule) {
				if (self.agentChoosed().agentName == schedule.agentName) {
					if (self.mySchedule().isDayOff && !schedule.isDayOff) {
						scheduleStartTime = schedule.scheduleStartTime();
					} else if (!self.mySchedule().isDayOff && schedule.isDayOff) {
						scheduleStartTime = self.mySchedule().scheduleStartTime();
					} else {
						scheduleStartTime = self.mySchedule().scheduleStartTime() < schedule.scheduleStartTime() ? self.mySchedule().scheduleStartTime() : schedule.scheduleStartTime();
					}

					var mappedLayers = ko.utils.arrayMap(schedule.layers, function(layer) {
						var minutesSinceTimeLineStart = moment(layer.startTime).diff(scheduleStartTime, 'minutes');
						return new Teleopti.MyTimeWeb.Request.CloneLayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart);
					});
					tradedScheduleModel = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers, schedule.scheduleStartTime(), schedule.scheduleEndTime(), schedule.agentName, schedule.personId, schedule.isDayOff);

				}
			});

			var mappedlayers = [];
			if (self.mySchedule() != null) {
				mappedlayers = ko.utils.arrayMap(self.mySchedule().layers, function(layer) {
					var minutesSinceTimeLineStart = moment(layer.startTime).diff(scheduleStartTime, 'minutes');
					return new Teleopti.MyTimeWeb.Request.CloneLayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart);
				});
			}
			currentTrade.mySchedule = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, self.mySchedule().scheduleStartTime(), self.mySchedule().scheduleEndTime(), self.mySchedule().agentName, self.mySchedule().personId, self.mySchedule().isDayOff);

			currentTrade.tradedSchedule = tradedScheduleModel;
			var currentChooseView = new Teleopti.MyTimeWeb.Request.ChooseHistoryViewModel(currentTrade, self.layerCanvasPixelWidth());
			self.chooseHistorys.push(currentChooseView);
			self.sortByDate();
			self.selectedInternal(true);
			self.isSendEnabled(true);
		};

		self.remove = function(chooseHistoryViewModel) {
			var date = chooseHistoryViewModel.selectedDate;
			var dayToDelete = $.grep(self.chooseHistorys(), function(e) { return e.selectedDate == date; })[0];
			self.chooseHistorys.remove(dayToDelete);

			if (chooseHistoryViewModel.selectedDateInFormat() == self.requestedDate().format(self.DatePickerFormat())) {
				self.selectedInternal(false);
			}
			if (self.chooseHistorys().length < 1) self.isSendEnabled(false);
		};

		self.isAddVisible = ko.computed(function() {
			var addVisible = false;
			if (self.isDetailVisible() && !self.selectedInternal()) {
				addVisible = true;
			}
			return addVisible;
		});

		self.filterStartEndTimeClick = function() {
			$('.dropdown-menu').on('click', function(e) {
				if ($(this).hasClass('filter-time-dropdown-form')) {
					e.stopPropagation();
				}
			});
		};
		
		self.filteredStartTimeArray = ko.observableArray();
		self.filteredSchedules = ko.observableArray();
		self.filterSchedule = function () {
			self.filteredSchedules.removeAll();
			$.each(self.possibleTradeSchedules(), function (i, schedule) {
				$.each(self.filteredStartTimeArray(), function (j, filterTime) {
					var startHour = schedule.scheduleStartTime().hours();
					if (!schedule.IsDayOff && startHour >= filterTime.start && startHour <= filterTime.end) {
						self.filteredSchedules.push(schedule);
					}
				});
			});
			if (self.filteredSchedules().length < 20 && self.selectedPageIndex()<self.pageCount()) {
				self.loadSchedule(self.selectedTeamInternal());
				self.filterSchedule();
			} else {
				self.possibleTradeSchedules.removeAll();
				$.each(self.filteredSchedules(), function(i, filteredSchedule) {
					self.possibleTradeSchedules.push(filteredSchedule);
				});
			}
		};
		self.filterStartTime = ko.computed(function () {
			ko.utils.arrayForEach(self.filterTimeList(), function(timeInFilter) {
				if (timeInFilter.isChecked()) {
					self.filteredStartTimeArray.push(timeInFilter);
					self.filterSchedule();
				} else {
					//todo: need search if already exist!!!
					//self.filteredStartTimeArray.remove(timeInFilter);
					//self.filterSchedule();
				}
			});
		});
		self.isShowList = ko.computed(function() {
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
		self.pixelPerMinute = ko.computed(function() {
			return self.layerCanvasPixelWidth() / self.timeLineLengthInMinutes();
		});

		self._createMySchedule = function(myScheduleObject) {
			var mappedlayers = [];
			if (myScheduleObject != null) {
				var layers = myScheduleObject.ScheduleLayers;
				var scheduleStartTime = moment(layers[0].Start);
				var scheduleEndTime = moment(layers[layers.length - 1].End);
				mappedlayers = ko.utils.arrayMap(layers, function(layer) {
					var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
					return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart, self.pixelPerMinute());
				});
				self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, scheduleStartTime, scheduleEndTime, myScheduleObject.Name, myScheduleObject.PersonId, myScheduleObject.IsDayOff));
			} else {
				self.mySchedule(new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedlayers, moment(), moment(), '', '', false));
			}
		};

		self._createPossibleTradeSchedules = function(possibleTradeSchedules) {
			self.possibleTradeSchedules.removeAll();
			var mappedPersonsSchedule = ko.utils.arrayMap(possibleTradeSchedules, function(personSchedule) {
				var layers = personSchedule.ScheduleLayers;
				var scheduleStartTime = moment(layers[0].Start);
				var scheduleEndTime = moment(layers[layers.length - 1].End);

				var mappedLayers = ko.utils.arrayMap(personSchedule.ScheduleLayers, function(layer) {
					var minutesSinceTimeLineStart = moment(layer.Start).diff(self.timeLineStartTime(), 'minutes');
					return new Teleopti.MyTimeWeb.Request.LayerAddShiftTradeViewModel(layer, minutesSinceTimeLineStart, self.pixelPerMinute());
				});
				var model = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(mappedLayers, scheduleStartTime, scheduleEndTime, personSchedule.Name, personSchedule.PersonId, personSchedule.IsDayOff);
				self.possibleTradeSchedules.push(model);
				return model;
			});

			self.noPossibleShiftTrades(mappedPersonsSchedule.length == 0 ? true : false);
		};

		self.chooseAgent = function(agent) {
			//hide or show all agents
			$.each(self.possibleTradeSchedules(), function(index, value) {
				value.isVisible(agent == null);
			});
			if (agent != null) {
				agent.isVisible(true);
				_redrawLayers();
				//rk - don't really like to put DOM stuff here...
				window.scrollTo(0, 0);
				setSendEnableStatus(self);
			}
			self.agentChoosed(agent);
			self.errorMessage('');
			if (self.isTradeForMultiDaysEnabled() && agent != null) {
				self.add();
			}
			if (agent == null) {
				self.selectedInternal(false);
			}
		};

		self.sendRequest = function() {
			self.isSendEnabled(false);
			sendRequestCallback(self);
			self.chooseAgent(null);
			self.chooseHistorys.removeAll();
			self.goToFirstPage();
		};

		self.cancelRequest = function() {
			self.chooseAgent(null);
			self.chooseHistorys.removeAll();
			self.selectedInternal(false);
			if (self.subject() != undefined) {
				self.subject("");
			}
			if (self.message() != undefined) {
				self.message("");
			}
			self.goToFirstPage();
		};

		self._createTimeLine = function(hours) {
			var firstTimeLineHour = moment(hours[0].StartTime);
			var lastTimeLineHour = moment(hours[hours.length - 1].EndTime);
			self.setTimeLineLengthInMinutes(firstTimeLineHour, lastTimeLineHour);
			self.hours([]);
			if (hours.length < 18) {
				var arrayMap = ko.utils.arrayMap(hours, function(hour) {
					return new Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel(hour, self.timeLineStartTime(), self.pixelPerMinute(), true);
				});

				self.hours.push.apply(self.hours, arrayMap);
			} else {
				for (var i = 0; i < hours.length; i++) {
					var isVisible = (i % 2 != 0);
					var newHour = new Teleopti.MyTimeWeb.Request.TimeLineHourAddShiftTradeViewModel(hours[i], self.timeLineStartTime(), self.pixelPerMinute(), isVisible);
					self.hours.push(newHour);
				}
			}
		};

		self.requestedDate = ko.computed({
			read: function() {
				return self.requestedDateInternal();
			},
			write: function(value) {
				//remove old layer's tooltip if it still exist
				$("[class='tooltip fade top in']").remove();

				if (self.requestedDateInternal().diff(value) == 0) return;
				self.prepareLoad();
				self.requestedDateInternal(value);

				self.loadMyTeamId();
			}
		});

		self.loadSchedule = function(value) {
			if (value != "allTeams") {
				self.loadOneTeamSchedule();
			} else {
				self.loadScheduleForAllTeams();
			}
		};
		
		self.selectedTeam = ko.computed({
			read: function() {
				return self.selectedTeamInternal();
			},
			write: function(value) {
				if (value != null && self.selectedTeamInternal() != null && value != self.selectedTeamInternal()) {
					self.chooseAgent(null);
					self.chooseHistorys.removeAll();
				}
				self.selectedTeamInternal(value);
				if (value == null) return;
				self.prepareLoad();
				self.loadSchedule(value);
			}
		});

		self.nextDateValid = ko.computed(function() {
			return self.openPeriodEndDate().diff(self.requestedDateInternal()) > 0;
		});

		self.previousDateValid = ko.computed(function() {
			return self.requestedDateInternal().diff(self.openPeriodStartDate()) > 0;
		});

		self.prepareLoad = function() {
			self.possibleTradeSchedulesRaw = [];
			self.selectedPageIndex(1);
			self.displayedPages.removeAll();
			self.isPreviousMore(false);
			if (self.agentChoosed() != null && self.isTradeForMultiDaysEnabled()) {
				self.keepSelectedAgentVisible();
			} else
				self.chooseAgent(null);
			self.IsLoading(false);
		};

		self.isRequestedDateValid = function(date) {
			if (date.diff(self.openPeriodStartDate()) < 0) {
				return false;
			} else if (self.openPeriodEndDate().diff(date) < 0) {
				return false;
			}
			return true;
		};

		self.keepSelectedAgentVisible = function() {
			if (self.agentChoosed() != null && self.possibleTradeSchedules() != null) {
				$.each(self.possibleTradeSchedules(), function(index, value) {
					value.isVisible(value.agentName == self.agentChoosed().agentName);
				});
			}

			var isAddAvaiable = false;
			$.each(self.chooseHistorys(), function(index, chooseHistoryViewModel) {
				if (self.requestedDateInternal().format(self.DatePickerFormat()) == chooseHistoryViewModel.selectedDateInFormat()) {
					isAddAvaiable = true;
					return false;
				}
			});
			self.selectedInternal(isAddAvaiable);
		};

		self.updateSelectedPage = function() {
			$.each(self.displayedPages(), function(index, item) {
				if (item.index() == self.selectedPageIndex()) {
					item.isSelected(true);
				} else {
					item.isSelected(false);
				}
			});
		};
		self.goToFirstPage = function() {
			self.selectedPageIndex(1);
			self.isPreviousMore(false);
			self.displayedPages.removeAll();
			self.initDisplayedPages(self.pageCount());
			self.loadSchedule(self.selectedTeamInternal());
		};
		self.goToLastPage = function() {
			self.isMore(false);
			if (self.pageCount() > 5) self.isPreviousMore(true);
			var timesOfNumPerPage = self.pageCount() / 5;
			var modeOfNumPerPage = self.pageCount() % 5;
			if (timesOfNumPerPage > 0) {
				self.displayedPages.removeAll();
				if (modeOfNumPerPage != 0) {
					for (var i = 1; i <= modeOfNumPerPage; i++) {
						self.displayedPages.push(new Teleopti.MyTimeWeb.Request.PageView(Math.floor(timesOfNumPerPage) * 5 + i));
					}
				} else {
					for (var j = 1; j <= 5; j++) {
						self.displayedPages.push(new Teleopti.MyTimeWeb.Request.PageView(self.pageCount() - 5 + j));
					}
				}
			}
			self.setSelectPage(self.pageCount());
		};
		self.initDisplayedPages = function(pageCount) {
			self.displayedPages.removeAll();
			for (var i = 1; i <= pageCount; ++i) {
				if (i <= 5) {
					self.displayedPages.push(new Teleopti.MyTimeWeb.Request.PageView(i));
				} else {
					break;
				}
			}
			$.each(self.displayedPages(), function(index, item) {
				if (item.index() == self.selectedPageIndex()) {
					item.isSelected(true);
				}
			});

			var currentLastPageNumber = self.displayedPages().length > 0 ? self.displayedPages()[self.displayedPages().length - 1].index() : 0;
			if (currentLastPageNumber != 0 && currentLastPageNumber < pageCount) self.isMore(true);
		};

		self.goNextPages = function() {
			$.each(self.displayedPages(), function(index, item) {
				if ((item.index() + 5) <= self.pageCount()) {
					item.index(item.index() + 5);
				} else {
					self.isMore(false);
					self.displayedPages.remove(item);
				}
			});

			if (self.displayedPages()[0].index() > 5) self.isPreviousMore(true);

			self.setSelectPage(self.displayedPages()[0].index());
		};
		
		self.goPreviousPages = function() {
			$.each(self.displayedPages(), function(index, item) {
				if (index + 1 <= self.displayedPages().length) {
					item.index(item.index() - 5);
				}
				if (self.displayedPages()[0].index() == 1) self.isPreviousMore(false);
			});
			
			if (self.displayedPages().length < 5) {
				for (var i = self.displayedPages().length+1; i <= 5; ++i) {
					var page = new Teleopti.MyTimeWeb.Request.PageView(i);
					self.displayedPages.push(page);
					self.isPreviousMore(false);
				}
			}

			if (self.displayedPages()[4].index() < self.pageCount()) self.isMore(true);

			self.setSelectPage(self.displayedPages()[0].index());
		};

		self.selectPage = function (page) {
			self.setSelectPage(page.index());
		};

		self.setSelectPage = function(pageIdx) {
			self.selectedPageIndex(pageIdx);
			self.loadSchedule(self.selectedTeamInternal());
		};
		
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

		self.createAllTeams = function (teams) {
			self.availableAllTeamIds.removeAll();

			var text = $("#Request-all-permitted-teams").val();
			$.each(teams, function(index, team) {
				self.availableAllTeamIds.push(team.id);
			});

			self.availableTeams.push({ id: "allTeams", text: text });
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
                    if (self.isPossibleSchedulesForAllEnabled()) {
                    	self.createAllTeams(data);
                    }
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

		self.loadOneTeamSchedule = function () {
			if (self.IsLoading()) return;
			var take = 20;
			var skip = (self.selectedPageIndex() -1) * take;
			
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
					self.pageCount(data.PageCount);
					if (self.pageCount() == 0) {
						self.isPageVisible(false);
					} else {
						self.isPageVisible(true);
					}
					if (self.displayedPages().length == 0) {
						self.initDisplayedPages(data.PageCount);
					}
					
				    self._createTimeLine(data.TimeLineHours);
				    self._createMySchedule(data.MySchedule);
				    if (self.possibleTradeSchedulesRaw.length > 0) {
					    self.possibleTradeSchedulesRaw = [];
				    }
					var findTradedAgent = false;
				    $.each(data.PossibleTradeSchedules, function (i, item) {
				    	self.possibleTradeSchedulesRaw.push(item);
				    	if (self.agentChoosed() && self.isTradeForMultiDaysEnabled()) {
						    if (item.Name == self.agentChoosed().agentName) {
							    findTradedAgent = true;
						    }
					    }
				    });
				    if (self.agentChoosed() && self.isTradeForMultiDaysEnabled()) {
						if (!findTradedAgent && (self.selectedPageIndex() < data.PageCount)) {
							self.selectedPageIndex(self.selectedPageIndex() + 1);
							self.IsLoading(false);
							self.loadOneTeamSchedule();
						} 
				    }
				    self.updateSelectedPage();
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

		self.loadScheduleForAllTeams = function() {
				if (self.IsLoading()) return;
				var take = 20;
				var skip = (self.selectedPageIndex() - 1) * take;

				ajax.Ajax({
					url: "Requests/ShiftTradeRequestScheduleForAllTeams",
					dataType: "json",
					type: 'GET',
					contentType: 'application/json; charset=utf-8',
					data: {
						selectedDate: self.requestedDateInternal().format($('#Request-detail-datepicker-format').val().toUpperCase()),
						teamIds: self.availableAllTeamIds().join(","),
				    Take: take,
				    Skip: skip
				},
				beforeSend: function () {
					self.IsLoading(true);
				},
				success: function (data, textStatus, jqXHR) {
					self.pageCount(data.PageCount);
					if (self.pageCount() == 0) {
						self.isPageVisible(false);
					} else {
						self.isPageVisible(true);
					}
					if (self.displayedPages().length == 0) {
						self.initDisplayedPages(data.PageCount);
					}
					
				    self._createTimeLine(data.TimeLineHours);
				    self._createMySchedule(data.MySchedule);
				    if (self.possibleTradeSchedulesRaw.length > 0) {
					    self.possibleTradeSchedulesRaw = [];
				    }
					var findTradedAgent = false;
				    $.each(data.PossibleTradeSchedules, function (i, item) {
				    	self.possibleTradeSchedulesRaw.push(item);
				    	if (self.agentChoosed() && self.isTradeForMultiDaysEnabled()) {
				    		if (item.Name == self.agentChoosed().agentName) {
				    			findTradedAgent = true;
				    		}
				    	}
				    });
					if (self.agentChoosed() && self.isTradeForMultiDaysEnabled()) {
						if (!findTradedAgent && (self.selectedPageIndex() < data.PageCount)) {
							self.selectedPageIndex(self.selectedPageIndex() + 1);
							self.IsLoading(false);
							self.loadScheduleForAllTeams();
						}
					}
				    self.updateSelectedPage();
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

		self.loadScheduleForOneTeamFilteredTime = function () {
				if (self.IsLoading()) return;
				var take = 20;
				var skip = (self.selectedPageIndex() - 1) * take;

				ajax.Ajax({
					url: "Requests/ShiftTradeRequestScheduleForOneTeamFilteredTime",
					dataType: "json",
					type: 'GET',
					contentType: 'application/json; charset=utf-8',
					data: {
						selectedDate: self.requestedDateInternal().format($('#Request-detail-datepicker-format').val().toUpperCase()),
						teamIds: self.selectedTeamInternal(),
						filteredStartTimes: self.filteredStartTimeArray(),
				    Take: take,
				    Skip: skip
				},
				beforeSend: function () {
					self.IsLoading(true);
				},
				success: function (data, textStatus, jqXHR) {
					self.pageCount(data.PageCount);
					if (self.pageCount() == 0) {
						self.isPageVisible(false);
					} else {
						self.isPageVisible(true);
					}
					if (self.displayedPages().length == 0) {
						self.initDisplayedPages(data.PageCount);
					}

					self._createTimeLine(data.TimeLineHours);
					self._createMySchedule(data.MySchedule);
					if (self.possibleTradeSchedulesRaw.length > 0) {
						self.possibleTradeSchedulesRaw = [];
					}
					var findTradedAgent = false;
					$.each(data.PossibleTradeSchedules, function (i, item) {
						self.possibleTradeSchedulesRaw.push(item);
						if (self.agentChoosed() && self.isTradeForMultiDaysEnabled()) {
							if (item.Name == self.agentChoosed().agentName) {
								findTradedAgent = true;
							}
						}
					});
					if (self.agentChoosed() && self.isTradeForMultiDaysEnabled()) {
						if (!findTradedAgent && (self.selectedPageIndex() < data.PageCount)) {
							self.selectedPageIndex(self.selectedPageIndex() + 1);
							self.IsLoading(false);
							self.loadScheduleForOneTeamFilteredTime();
						}
					}
					self.updateSelectedPage();
					self._createPossibleTradeSchedules(self.possibleTradeSchedulesRaw);
					self.keepSelectedAgentVisible();
					self.isReadyLoaded(true);

					// Redraw layers after data loaded
					_redrawLayers();
				},
				error: function (e) {
				},
				complete: function () {
					self.IsLoading(false);
				}
			});
		};

		self.changeRequestedDate = function (movement) {
			var date = moment(self.requestedDateInternal()).add('days', movement);
			if (self.isRequestedDateValid(date)) {
	      	self.requestedDate(date);
			}
		};

		self.nextDate = function () {
		    self.changeRequestedDate(1);
		};

		self.previousDate = function () {
		    self.changeRequestedDate(-1);
		};

		self.featureCheck = function() {
			ajax.Ajax({
				url: "../ToggleHandler/IsEnabled?toggle=Request_ShiftTradeRequestForMoreDays_20918",
				success: function(data) {
					if (data.IsEnabled) {
						self.isTradeForMultiDaysEnabled(true);
					}
				}
			});

			ajax.Ajax({
				url: "../ToggleHandler/IsEnabled?toggle=Request_SeePossibleShiftTradesFromAllTeams_28770",
				success: function(data) {
					if (data.IsEnabled) {
						self.isPossibleSchedulesForAllEnabled(true);
					}
				}
			});
			
			ajax.Ajax({
				url: "../ToggleHandler/IsEnabled?toggle=Request_FilterPossibleShiftTradeByTime_24560",
				success: function (data) {
					if (data.IsEnabled) {
						self.isFilterByTimeEnabled(true);
					}
				}
			});
		};

		self.loadFilterTimes = function() {
			var rangStart = 6;
			for (var i = 0; i < 18; i += 2) {
				var rangEnd = rangStart + 2;
				var filterTime = new Teleopti.MyTimeWeb.Request.FilterTimeView(rangStart + ":00 - " + rangEnd + ":00", rangStart, rangEnd, false);
				self.filterTimeList.push(filterTime);
				rangStart += 2;
			}
		};
	}

	function _redrawLayers() {
		var canvasWidth;

		if (vm.isReadyLoaded()) {
			canvasWidth = $("td.shift-trade-possible-trade-schedule:visible").width();
		} else {
			var containerWidth = $("#Request-add-shift-trade").width();
			var nameCellWidth = $("td.shift-trade-agent-name").width();
			canvasWidth = containerWidth - nameCellWidth;
			if (vm.isTradeForMultiDaysEnabled()) {
				var buttonAddCellWidth = $("td.shift-trade-button-cell").width();
				canvasWidth = canvasWidth - buttonAddCellWidth;
			}
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
		if (vm.chooseHistorys() != undefined) {
			$.each(vm.chooseHistorys(), function (index, chooseHistory) {
				chooseHistory.canvasPixelWidth(canvasWidth);
			});
		}
		if (vm.hours() != undefined) {
			$.each(vm.hours(), function (index, hour) {
				hour.pixelPerMinute(vm.pixelPerMinute());
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
			if (vm.subject() != undefined) {
				vm.subject("");
			}
			if (vm.message() != undefined) {
				vm.message("");
			}
			vm.chooseAgent(null);
			_setWeekStart(vm);
			vm.goToFirstPage();
			if(vm.isFilterByTimeEnabled) vm.loadFilterTimes();
		}

		$(window).resize(function() {
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
})(jQuery);