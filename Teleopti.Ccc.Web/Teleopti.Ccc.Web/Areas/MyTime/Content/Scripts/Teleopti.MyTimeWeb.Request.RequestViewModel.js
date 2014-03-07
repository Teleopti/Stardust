/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>

Teleopti.MyTimeWeb.Request.RequestViewModel = function RequestViewModel(addRequestMethod, firstDayOfWeek, defaultDateTimes) {
	var self = this;
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	self.Templates = ["text-request-detail-template", "absence-request-detail-template", "shifttrade-request-detail-template"];
	self.IsFullDay = ko.observable(false);
	self.IsUpdate = ko.observable(false);
    self.DateFrom = ko.observable(moment().startOf('day'));
    self.DateTo = ko.observable(moment().startOf('day'));
    self.TimeFromInternal = ko.observable(defaultDateTimes ? defaultDateTimes.defaultStartTime : null);
    self.TimeToInternal = ko.observable(defaultDateTimes ? defaultDateTimes.defaultEndTime : null);
    self.DateFormat = ko.observable();
    self.TimeFrom = ko.computed({
        read: function() {
            if (self.IsFullDay()) {
                return defaultDateTimes.defaultFulldayStartTime;
            }
            return self.TimeFromInternal();
        },
        write: function (value) {
            if (self.IsFullDay()) return;
            self.TimeFromInternal(value);
        }
    });
    self.TimeTo = ko.computed({
        read: function () {
            if (self.IsFullDay()) {
                return defaultDateTimes.defaultFulldayEndTime;
            }
            return self.TimeToInternal();
        },
        write: function (value) {
            if (self.IsFullDay()) return;
            self.TimeToInternal(value);
        }
    });
    
    self.FormattedDateFrom = ko.computed({
    	read: function () {
    		return self.DateFrom().format(self.DateFormat());
    	}
    });

    self.FormattedDateTo = ko.computed({
    	read: function () {
    		return self.DateTo().format(self.DateFormat());
    	}
    });

    self.ShowMeridian = ($('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true');
    self.TypeEnum = ko.observable(0);
    self.ShowError = ko.observable(false);
    self.ErrorMessage = ko.observable('');
    self.AbsenceId = ko.observable();
    self.AbsenceTrackedAsDay = ko.observable();
    self.AbsenceTrackedAsHour = ko.observable();
    self.Absences = ko.observableArray();
    self.AbsenceUsed = ko.observable();
    self.AbsenceRemaining = ko.observable();
    self.Subject = ko.observable();
    self.Message = ko.observable();
    self.EntityId = ko.observable();
    self.DenyReason = ko.observable();
    self.IsEditable = ko.observable(true);
    self.IsNewInProgress = ko.observable(false);
    self.weekStart = ko.observable(firstDayOfWeek); 
    self.IsTimeInputEnabled = ko.computed(function () {
        return !self.IsFullDay() && self.IsEditable();
    });

    self.readAbsences = function (data) {
    	if (data.AbsenceTypes) {
    		for (var i = 0; i < (data.AbsenceTypes.length) ; i++) {
    			var newAbsence = {
    				Id: data.AbsenceTypes[i].Id,
    				Name: data.AbsenceTypes[i].Name
    			};

    			self.Absences.push(newAbsence);
    		}
    	}
    };

    self.readAbsenceAccount = function (data) {
    	if (data) {
    		self.AbsenceTrackedAsDay(data.Tracker == "Days");
    		self.AbsenceTrackedAsHour(data.Tracker == "Time");
		    self.AbsenceRemaining(data.Remaining);
		    self.AbsenceUsed(data.Used);
	    }
    };

	function loadAbsenceAccount() {
		ajax.Ajax({
			url: "Requests/FetchAbsenceAccount",
			dataType: "json",
			type: 'GET',
			contentType: 'application/json; charset=utf-8',
			data: {
				absenceId: self.AbsenceId,
				date: self.DateTo
			},
			success: function (data, textStatus, jqXHR) {
				self.ReadAbsenceAccount(data);
			},
			error: function (e) {
				//console.log(e);
			},
			complete: function () {
				//self.IsLoading(false);
			}
		});
	};

	self.AbsenceId.subscribe(function () {
		loadAbsenceAccount();
	});

	self.DateTo.subscribe(function () {
		loadAbsenceAccount();
	});

	self.Template = ko.computed(function () {
		return self.IsUpdate() ? self.Templates[self.TypeEnum()] : "add-new-request-detail-template";
    });
    
    self.ShowAbsencesCombo = ko.computed(function() {
        return self.TypeEnum() === 1 ? true : false;
    });

    self.AddRequestCallback = undefined;

    self.AddRequest = function() {
        addRequestMethod(self, self.AddRequestCallback);
    };
	
    function _setDefaultDates() {
        var year = defaultDateTimes.todayYear;
        var month = defaultDateTimes.todayMonth;
        var day = defaultDateTimes.todayDay;
        self.DateFrom(moment(new Date(year, month - 1, day)));
        self.DateTo(moment(new Date(year, month - 1, day)));
    }
	
    self.AddTextRequest = function (useDefaultDates) {
        if (useDefaultDates != undefined && useDefaultDates === true)
            _setDefaultDates();
        self.IsNewInProgress(true);
		self.TypeEnum(0);
		self.IsFullDay(false);
		
    };

    self.AddAbsenceRequest = function (useDefaultDates) {
        if (useDefaultDates != undefined && useDefaultDates === true)
            _setDefaultDates();
        self.IsNewInProgress(true);
		self.TypeEnum(1);
		self.IsFullDay(true);
    };

    self.CancelAddingNewRequest = function() {

        self.IsNewInProgress(false);
    };
};