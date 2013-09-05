﻿/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Portal.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.RequestViewModel.js"/>
/// <reference path="~/Content/moment/moment.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Schedule = (function ($) {
	var timeIndicatorDateTime;
	var scheduleHeight = 668;
	var timeLineOffset = 109;
	var pixelToDisplayAll = 33;
	var pixelToDisplayTitle = 16;
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;
	var completelyLoaded;
	var weekStart = 3;
    
	ajax.Ajax({
	    url: 'UserInfo/Culture',
	    dataType: "json",
	    type: 'GET',
	    success: function (data) {
	        weekStart = data.WeekStart;
	    }
	});
    
	function _bindData(data) {
		vm.Initialize(data);
		_initTimeIndicator();
		//_initTooltip();
		//Teleopti.MyTimeWeb.Schedule.Request.PartialInit();
		$('.body-weekview-inner').show();
		completelyLoaded();
	}

	function _initTimeIndicator() {
		var currentDateTimeStart = new Date(new Date().getTeleoptiTime());
		_setTimeIndicator(currentDateTimeStart);
		setInterval(function () {
			var currentDateTime = new Date(new Date().getTeleoptiTime());
			if (timeIndicatorDateTime == undefined || currentDateTime.getMinutes() != timeIndicatorDateTime.getMinutes()) {
				timeIndicatorDateTime = currentDateTime;
				_setTimeIndicator(timeIndicatorDateTime);
			}
		}, 1000);
	}

	var WeekScheduleViewModel = function (userTexts, addRequestViewModel, navigateToRequestsMethod) {
	    var self = this;
	    self.navigateToRequestsMethod = navigateToRequestsMethod;
		self.userTexts = userTexts;
		self.textPermission = ko.observable();
		self.periodSelection = ko.observable();
		self.asmPermission = ko.observable();
		self.absenceRequestPermission = ko.observable();
		self.overtimeAvailabilityPermission = ko.observable();
		self.isCurrentWeek = ko.observable();
		self.timeLines = ko.observableArray();
		self.days = ko.observableArray();
		self.styles = ko.observable();
		self.minDate = ko.observable(moment());
		self.maxDate = ko.observable(moment());

		self.displayDate = ko.observable();
		self.nextWeekDate = ko.observable(moment());
		self.previousWeekDate = ko.observable(moment());
		self.datePickerFormat = ko.observable();

	    self.selectedDate = ko.observable(moment().startOf('day'));
	    
	    self.requestViewModel = ko.observable();

	    self.textRequestActive = ko.observable(false);
	    self.absenceRequestActive = ko.observable(false);
	    self.overtimeAvailabilityActive = ko.observable(false);
	    self.initialRequestDay = null;

	    self.showAddRequestToolbar = ko.computed(function() {
		    return (self.requestViewModel() || '') != '';
	    });

	    self.textRequestActivate = function() {
	    	self.absenceRequestActive(false);
	    	self.overtimeAvailabilityActive(false);
	        if (!self.textRequestActive()) {
	            self.textRequestActive(true);
	            self.showAddTextRequestForm();
	        }
	    };
	    
	    self.absenceRequestActivate = function () {
	    	self.textRequestActive(false);
	    	self.overtimeAvailabilityActive(false);
	        if (!self.absenceRequestActive()) {
	            self.absenceRequestActive(true);
	            self.showAddAbsenceRequestForm();
	        }
	    };

	    self.overtimeAvailabilityActivate = function () {
	    	self.textRequestActive(false);
	    	self.absenceRequestActive(false);
	    	if (!self.overtimeAvailabilityActive()) {
	    		self.overtimeAvailabilityActive(true);
	    		self.showAddOvertimeAvailabilityForm();
	    	}
	    };

		self.setCurrentDate = function (date) {
		    self.selectedDate(date);
	        self.selectedDate.subscribe(function(d) {
	            Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
	        });
		};

	    self.nextWeek = function() {
	        self.selectedDate(self.nextWeekDate());
	    };

	    self.previousWeek = function () {
	        self.selectedDate(self.previousWeekDate());
	    };
	    
		self.today = function () {
		    Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week");
		};

		self.isWithinSelected = function (startDate, endDate) {
			return (startDate <= self.maxDate() && endDate >= self.minDate());
		    
		};

	    self.showAddTextRequestForm = function() {
	        if (self.textPermission() !== true) {
	            return;
	        }
	        self.setRequestViewModel();
	        self.requestViewModel().DateFrom(moment(self.initialRequestDay.date()));
	        self.requestViewModel().DateTo(moment(self.initialRequestDay.date()));
	        self.requestViewModel().AddTextRequest(false);
	    };
	    
	    self.showAddAbsenceRequestForm = function () {
	        if (self.absenceRequestPermission() !== true) {
	            return;
	        }
	        self.setRequestViewModel();
	        self.requestViewModel().DateFrom(moment(self.initialRequestDay.date()));
	        self.requestViewModel().DateTo(moment(self.initialRequestDay.date()));
	        self.requestViewModel().AddAbsenceRequest(false);
	    };

		self.showAddOvertimeAvailabilityForm = function() {
			if (self.overtimeAvailabilityPermission() !== true) {
				return;
			}
			self.setOvertimeAvailabilityViewModel();
			self.requestViewModel().DateFrom(moment(self.initialRequestDay.date()));
			self.requestViewModel().DateTo(moment(self.initialRequestDay.date()));
			if (self.requestViewModel().LoadRequestData) {
				self.requestViewModel().LoadRequestData(self.initialRequestDay);
			}
		};

	    self.showAddRequestForm = function (day) {
	        self.initialRequestDay = day;
		    
	        if ((self.requestViewModel() || '') != '') {
	        	if (self.requestViewModel().LoadRequestData) {
	        		self.requestViewModel().LoadRequestData(self.initialRequestDay);
	        	}
	            self.requestViewModel().DateFrom(moment(day.date()));
	            self.requestViewModel().DateTo(moment(day.date()));
	            return;
	        }

	        if (self.overtimeAvailabilityPermission() === true) {
		        self.overtimeAvailabilityActivate();
	        } else {
		        self.textRequestActivate();
	        }
	    };
	    
        self.setRequestViewModel = function()
        {
            var datePickerFormat = $('#Request-detail-datepicker-format').val().toUpperCase();
            var model = addRequestViewModel();
            model.DateFormat(datePickerFormat);

            self.requestViewModel(model);
        };
		
        self.setOvertimeAvailabilityViewModel = function () {
        	var model = new Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel(ajax, _displayOvertimeAvailability);
	        self.requestViewModel(model);
        };
		
        function _displayOvertimeAvailability(overtimeAvailability) {
        	self.initialRequestDay.overtimeAvailability(overtimeAvailability);
	        self.CancelAddingNewRequest();
        }

	    self.CancelAddingNewRequest = function() {
	        self.requestViewModel(null);
	        self.textRequestActive(false);
	        self.absenceRequestActive(false);
	        self.overtimeAvailabilityActive(false);
	    };
	};

	ko.utils.extend(WeekScheduleViewModel.prototype, {
	    Initialize: function (data) {
		    var self = this;
		    self.absenceRequestPermission(data.RequestPermission.AbsenceRequestPermission);
		    self.overtimeAvailabilityPermission(data.RequestPermission.OvertimeAvailabilityPermission);
			self.textPermission(data.RequestPermission.TextRequestPermission);
			self.periodSelection(JSON.stringify(data.PeriodSelection));
			self.asmPermission(data.AsmPermission);
			self.isCurrentWeek(data.IsCurrentWeek);
			self.displayDate(data.PeriodSelection.Display);
			self.setCurrentDate(moment(data.PeriodSelection.Date));
		    self.nextWeekDate(moment(data.PeriodSelection.PeriodNavigation.NextPeriod));
		    self.previousWeekDate(moment(data.PeriodSelection.PeriodNavigation.PrevPeriod));
		    self.datePickerFormat(data.DatePickerFormat);
		    
			var styleToSet = {};
			$.each(data.Styles, function (key, value) {
				styleToSet[value.Name] = 'rgb({0})'.format(value.RgbColor);
			});
			self.styles(styleToSet);
			var timelines = ko.utils.arrayMap(data.TimeLine, function (item) {
				return new TimelineViewModel(item);
			});
			self.timeLines(timelines);
			var days = ko.utils.arrayMap(data.Days, function (item) {
				return new DayViewModel(item, self);
			});
			self.days(days);
			var minDateArr = data.PeriodSelection.SelectedDateRange.MinDate.split('-');
			var maxDateArr = data.PeriodSelection.SelectedDateRange.MaxDate.split('-');

			self.minDate(moment(new Date(minDateArr[0], minDateArr[1] - 1, minDateArr[2])).add('days', -1));
			self.maxDate(moment(new Date(maxDateArr[0], maxDateArr[1] - 1, maxDateArr[2])).add('days', 1));
		}
	});

	var DayViewModel = function (day, parent) {
		var self = this;

		self.fixedDate = ko.observable(day.FixedDate);
		self.dayOfWeek = ko.observable(day.DayOfWeekNumber);
		self.date = ko.observable(day.Date);
		self.state = ko.observable(day.State);
		self.headerTitle = ko.observable(day.Header.Title);
		self.headerDayDescription = ko.observable(day.Header.DayDescription);
		self.headerDayNumber = ko.observable(day.Header.DayNumber);
		self.textRequestPermission = ko.observable(parent.textPermission());
		self.summaryStyleClassName = ko.observable(day.Summary.StyleClassName);
		self.summaryTitle = ko.observable(day.Summary.Title);
		self.summaryTimeSpan = ko.observable(day.Summary.TimeSpan);
		self.summary = ko.observable(day.Summary.Summary);
		self.noteMessage = ko.computed(function () {
			//need to html encode due to not bound to "text" in ko
			return $('<div/>').text(day.Note.Message).html();
		});
		self.textRequestCount = ko.observable(day.TextRequestCount);
		self.allowance = ko.observable(day.Allowance);
		self.absenceAgents = ko.observable(day.AbsenceAgents);
		self.fullTimeEquivalent = ko.observable(day.FulltimeEquivalent);
		self.overtimeAvailability = ko.observable(day.OvertimeAvailabililty);

		self.basedOnAllowanceChance = function (options) {
			var percent;
			if (self.allowance() != 0)
				percent = 100 * ((self.allowance() - self.absenceAgents()) / self.allowance());
			else {
				percent = 0;
			}
			
			var index = 0;
			if (percent > 0 && (self.allowance() - self.absenceAgents()) >= self.fullTimeEquivalent()) {
			    index = percent > 30 && (self.allowance() - self.absenceAgents()) >= 2 * self.fullTimeEquivalent() ? 2 : 1;
			}		
			return options[index];
		};

		self.holidayChanceText = ko.computed(function () {
			return parent.userTexts.chanceOfGettingAbsencerequestGranted + self.basedOnAllowanceChance([parent.userTexts.poor, parent.userTexts.fair, parent.userTexts.good]);
		});
		
		self.holidayChanceColor = ko.computed(function () {

			return self.basedOnAllowanceChance(["red", "yellow", "green"]);
		});
		
		self.hasTextRequest = ko.computed(function () {
			return self.textRequestCount() > 0;
		});

		self.hasNote = ko.observable(day.HasNote);

		self.textRequestText = ko.computed(function () {
			return parent.userTexts.xRequests.format(self.textRequestCount());
		});
		
		self.textOvertimeAvailabilityText = ko.computed(function () {
			return self.overtimeAvailability().StartTime + " - " + self.overtimeAvailability().EndTime;
		});

		self.classForDaySummary = ko.computed(function () {
			var showRequestClass = self.textRequestPermission() ? 'show-request ' : '';
			return 'third category ' + showRequestClass + self.summaryStyleClassName(); //last one needs to be becuase of "stripes" and similar
		});

		self.colorForDaySummary = ko.computed(function () {
			return parent.styles()[self.summaryStyleClassName()];
		});

		self.textColor = ko.computed(function () {

			var backgroundColor = parent.styles()[self.summaryStyleClassName()];
			if (backgroundColor != null && backgroundColor != 'undefined') {
				return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
			}
			return 'black';
		});

		self.layers = ko.utils.arrayMap(day.Periods, function (item) {
			return new LayerViewModel(item, parent);
		});
		
		self.availability = ko.observable(day.Availability);
	    
		self.absenceRequestPermission = ko.computed(function () {
		    if (!parent.absenceRequestPermission() || !self.availability())
		        return false;
		    else {
		        return true;
		    }	
	    });

	    self.navigateToRequests = function() {
	        parent.navigateToRequestsMethod();
	    };
	};
	var LayerViewModel = function (layer, parent) {
		var self = this;

		self.title = ko.observable(layer.Title);
		self.hasMeeting = ko.computed(function () {
			return layer.Meeting != null;
		});
		self.meetingTitle = ko.computed(function () {
			if (self.hasMeeting()) {
				return layer.Meeting.Title;
			}
			return null;
		});
		self.meetingLocation = ko.computed(function () {
			if (self.hasMeeting()) {
				return layer.Meeting.Location;
			}
			return null;
		});
		self.meetingDescription = ko.computed(function () {
			if (self.hasMeeting()) {
				if (layer.Meeting.Description.length > 300) {
					return layer.Meeting.Description.substring(0, 300) + '...';
				}
				return layer.Meeting.Description;
			}
			return null;
		});
		self.timeSpan = ko.observable(layer.TimeSpan);
		self.color = ko.observable('rgb(' + layer.Color + ')');
		self.textColor = ko.computed(function () {
			if (layer.Color != null && layer.Color != 'undefined') {
				var backgroundColor = 'rgb(' + layer.Color + ')';
				return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
			}
			return 'black';
		});
		self.tooltipText = ko.computed(function () {
			//not nice! rewrite tooltips in the future!
			var text = '';
			if (self.hasMeeting()) {
				text =  '<div>{0}</div><div><dl><dt>{1} {2}</dt><dt>{3} {4}</dt><dt>{5} {6}</dt></dl></div>'
					.format(self.timeSpan(),
							parent.userTexts.subjectColon,
							$('<div/>').text(self.meetingTitle()).html(),
							parent.userTexts.locationColon,
							$('<div/>').text(self.meetingLocation()).html(),
							parent.userTexts.descriptionColon,
							$('<div/>').text(self.meetingDescription()).html());
			} else {
				text = self.timeSpan();
			}

			return '<div>{0}</div>{1}'.format(self.title(), text);
		});
		self.startPositionPercentage = ko.observable(layer.StartPositionPercentage);
		self.endPositionPercentage = ko.observable(layer.EndPositionPercentage);
		self.top = ko.computed(function () {
			return Math.round(scheduleHeight * self.startPositionPercentage());
		});
		self.height = ko.computed(function () {
			var bottom = Math.round(scheduleHeight * self.endPositionPercentage()) - 1;
			return bottom - self.top();
		});
		self.topPx = ko.computed(function () {
			return self.top() + 'px';
		});
		self.heightPx = ko.computed(function () {
			return self.height() + 'px';
		});
		self.heightDouble = ko.computed(function () {
			return scheduleHeight * (self.endPositionPercentage() - self.startPositionPercentage());
		});
		self.showTitle = ko.computed(function () {
			return self.heightDouble() > pixelToDisplayTitle;
		});
		self.showDetail = ko.computed(function () {
			return self.heightDouble() > pixelToDisplayAll;
		});
	};

	var TimelineViewModel = function (timeline) {
		var self = this;
		self.positionPercentage = ko.observable(timeline.PositionPercentage);

		self.minutes = ko.observable(timeline.Time.TotalMinutes);
		var timeFromMinutes = moment().startOf('day').add('minutes', self.minutes());

		self.time = ko.observable(timeFromMinutes.format('H:mm'));
		if (timeline.Culture == "en-US") {
			self.time(timeFromMinutes.format('h A'));
		}
		self.timeText = self.time() + "\ntotalMinutes" + self.minutes();

		self.topPosition = ko.computed(function () {
			return Math.round(scheduleHeight * self.positionPercentage()) + timeLineOffset + 'px';
		});
		self.evenHour = ko.computed(function () {
			if (timeFromMinutes.format('mm') != 0) { return false; }
			else { return true; }
		});
	};

	function _setTimeIndicator(theDate) {
		if ($('.week-schedule-ASM-permission-granted').text().indexOf('yes') == -1 ||
			$('.week-schedule-current-week').text().indexOf('yes') == -1) {
			return;
		}

		var timelineHeight = 668;
		var offset = 120;
		var timeindicatorHeight = 2;

		var hours = theDate.getHours();
		var minutes = theDate.getMinutes();
		var clientNowMinutes = (hours * 60) + (minutes * 1);

		var timelineStartMinutes = getMinutes(".weekview-timeline", true);
		var timelineEndMinutes = getMinutes(".weekview-timeline", false);

		var division = (clientNowMinutes - timelineStartMinutes) / (timelineEndMinutes - timelineStartMinutes);
		var position = Math.round(timelineHeight * division) - Math.round(timeindicatorHeight / 2);

		var dayOfWeek = moment(theDate).day();
		var timeIndicator = $('ul[data-mytime-dayofweek="' + dayOfWeek + '"] .week-schedule-time-indicator');
		var timeIndicatorTimeLine = $('.week-schedule-time-indicator-small');

		if (timelineStartMinutes <= clientNowMinutes && clientNowMinutes <= timelineEndMinutes) {
			timeIndicator.css("top", position).show();
			timeIndicatorTimeLine.css("top", position + offset).show();
		}
		else {
			timeIndicator.hide();
			timeIndicatorTimeLine.hide();
		}
	}

	function getMinutes(elementSelector, first) {
		var parent = $(elementSelector);
		var children = parent.children('.weekview-timeline-label');
		var timeString;
		if (first) {
			timeString = children.first().text();
		} else {
			timeString = children.last().text();
		}
		var timeParts = timeString.split("totalMinutes");
		return timeParts[1];
	}

	function _subscribeForChanges() {
		ajax.Ajax({
			url: 'MessageBroker/FetchUserData',
			dataType: "json",
			type: 'GET',
			success: function (data) {
				Teleopti.MyTimeWeb.MessageBroker.AddSubscription({
					url: data.Url,
					callback: Teleopti.MyTimeWeb.Schedule.ReloadScheduleListener,
					domainType: 'IScheduleChangedInDefaultScenario',
					businessUnitId: data.BusinessUnitId,
					datasource: data.DataSourceName,
					referenceId: data.AgentId
				});
			}
		});
	}
    
	function _cleanBindings() {
        ko.cleanNode($('#body-inner')[0]);
        if (vm != null) {
            vm.days([]);
            vm.timeLines([]);
            vm = null;
        }
	}
    
	function _displayRequest(data) {
	    var date = moment(new Date(data.DateFromYear, data.DateFromMonth - 1, data.DateFromDayOfMonth));
	    var formattedDate = date.format('YYYY-MM-DD');
	    var textRequestCount = $('ul[data-mytime-date="' + formattedDate + '"] .text-request');
	    var decodedTitle = $('<div/>').html(textRequestCount.attr('title')).text();
	    if (decodedTitle == undefined)
	        return;
	    var newTitle = decodedTitle.replace(/[\d\.]+/g, parseInt(textRequestCount.text()) + 1);
	    textRequestCount.attr('title', newTitle);
	    textRequestCount
			.show()
			.children()
			.first()
			.text(parseInt(textRequestCount.text()) + 1)
	    ;
	    
	    vm.CancelAddingNewRequest();
	}
    
	function _navigateToRequests() {
	    Teleopti.MyTimeWeb.Portal.NavigateTo("Requests/Index");
	}

	return {
		Init: function () {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Schedule/Week', Teleopti.MyTimeWeb.Schedule.PartialInit, Teleopti.MyTimeWeb.Schedule.PartialDispose);
			}
		},
		PartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			Teleopti.MyTimeWeb.Common.Layout.ActivateCustomInput();
			readyForInteractionCallback();
			completelyLoaded = completelyLoadedCallback;
		},
		SetupViewModel: function (userTexts, defaultDateTimes) {
		    var addRequestViewModel = function () {
		        var model = new Teleopti.MyTimeWeb.Request.RequestViewModel(Teleopti.MyTimeWeb.Request.RequestDetail.AddTextOrAbsenceRequest, weekStart, defaultDateTimes);
		        model.AddRequestCallback = _displayRequest;
		        return model;
		    };

			vm = new WeekScheduleViewModel(userTexts, addRequestViewModel, _navigateToRequests);
			ko.applyBindings(vm, $('#page')[0]);
		},
		LoadAndBindData: function () {
			ajax.Ajax({
				url: 'Schedule/FetchData',
				dataType: "json",
				type: 'GET',
				data: {
					date: Teleopti.MyTimeWeb.Portal.ParseHash().dateHash
				},
				success: function (data) {
					_bindData(data);
					_subscribeForChanges();
				}
			});
		},

		ReloadScheduleListener: function (notification) {
			var messageStartDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate);
			var messageEndDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate);

			if (vm.isWithinSelected(messageStartDate, messageEndDate)) {
				ajax.Ajax({
					url: 'Schedule/FetchData',
					dataType: "json",
					type: 'GET',
					data: {
						date: Teleopti.MyTimeWeb.Portal.ParseHash().dateHash
					},
					success: function (data) {
						_bindData(data);
					}
				});
			};
		},
		PartialDispose: function () {
		    _cleanBindings();
		},
		SetTimeIndicator: function (date) {
			_setTimeIndicator(date);
		}
	};

})(jQuery);


Teleopti.MyTimeWeb.Schedule.Layout = (function ($) {

	function _setDayState(curDay) {
		switch ($(curDay).data('mytime-state')) {
			case 1:
				break;
			case 2:
				$(curDay).addClass('today');
				break;
			case 3:
				$(curDay).addClass('editable');
				break;
			case 4:
				$(curDay).addClass('non-editable');
				break;
		}
	}

	return {
		SetSchemaItemsHeights: function () {
			var currentTallest = 0; // Tallest li per row
			var currentLength = 0;  // max amount of li's
			var currentHeight = 0;  // max height of ul
			var i = 0;
			$('.weekview-day').each(function () {
				if ($('li', this).length > currentLength) {
					currentLength = $('li', this).length;
				}

				_setDayState($(this));
			});
			for (i = 3; i <= currentLength; i++) {
				var currentLiRow = $('.weekview-day li:nth-child(' + i + ')');
				$(currentLiRow).each(function () {
					if ($(this).height() > currentTallest) {
						currentTallest = $(this).height();
					}

				});
				$('>div', $(currentLiRow)).css({ 'min-height': currentTallest - 20 }); // remove padding from height
				currentTallest = 0;
			}

			$('.weekview-day').each(function () {
				if ($(this).height() > currentHeight) {
					currentHeight = $(this).height();
				}
			});

			$('.weekview-day li:last-child').each(function () {
				var ulHeight = $(this).parent().height();
				var incBorders = (currentLength * 6);
				$(this).height((currentHeight - ulHeight) + incBorders);
			});
		}
	};

})(jQuery);
