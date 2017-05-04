$(document).ready(function () {
    module("Teleopti.MyTimeWeb.Schedule.MobileStartDay");
    var hash = "";
    var dataService;

    test("should navigate to next date when swiping left", function () {
        setup();

        $("body").addClass("dayview-view-body");
        Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(fakeReadyForInteractionCallback, fakeCompletelyLoadedCallback, dataService);
        var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
        var currentDate = vm.selectedDate();
        $(".dayview-view-body").swipe("option").swipeLeft();
        $(".dayview-view-body").swipe("disable");
        equal(vm.selectedDate().format("MMM Do YY"), moment(currentDate).add(1, 'days').format("MMM Do YY"));

    });

    test("should navigate to previous date when swiping right", function () {
        setup();

        $("body").addClass("dayview-view-body");
        Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(fakeReadyForInteractionCallback, fakeCompletelyLoadedCallback, dataService);
        var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
        var currentDate = vm.selectedDate();
        $(".dayview-view-body").swipe("option").swipeRight();
        $(".dayview-view-body").swipe("disable");
        equal(vm.selectedDate().format("MMM Do YY"), moment(currentDate).add(-1, 'days').format("MMM Do YY"));
    });

    test("should set timelines", function () {
        setup();
        Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(fakeReadyForInteractionCallback, fakeCompletelyLoadedCallback, dataService);
        var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

        equal(vm.timeLines().length, 12);
    });

    test("should set top position for timeline", function () {
        setup();
        Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(fakeReadyForInteractionCallback, fakeCompletelyLoadedCallback, dataService);
        var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

        equal(vm.timeLines()[1].topPosition(), "18px");
    });

    test("should set display time for timeline", function () {
        setup();
        Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(fakeReadyForInteractionCallback, fakeCompletelyLoadedCallback, dataService);
        var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

        equal(vm.timeLines()[1].timeText, "07:00");
    });

    test("should set event hour false for timeline", function () {
        setup();
        Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(fakeReadyForInteractionCallback, fakeCompletelyLoadedCallback, dataService);
        var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

        equal(vm.timeLines()[0].evenHour(), false);
    });

    test("should set event hour true for timeline", function () {
        setup();
        Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(fakeReadyForInteractionCallback, fakeCompletelyLoadedCallback, dataService);
        var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

        equal(vm.timeLines()[1].evenHour(), true);
    });

    test("should set timeline height", function () {
        setup();
        Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(fakeReadyForInteractionCallback, fakeCompletelyLoadedCallback, dataService);
        var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

        equal(vm.scheduleHeight(), "651px");
    });

    test("should set unreadMessage", function () {
        setup();

        Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(fakeReadyForInteractionCallback, fakeCompletelyLoadedCallback, dataService);
        var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
        equal(vm.unreadMessageCount(), 2);
    });

    test("should navigate to messages", function () {
	    Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
		    if (x === "MyTimeWeb_DayScheduleForStartPage_43446") return true;
		    return false;
        };

        setup();

        var fakeWindow = getFakeWindow();
        var setting = getDefaultSetting();
        Teleopti.MyTimeWeb.Portal.Init(setting, fakeWindow); 

        Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(fakeReadyForInteractionCallback, fakeCompletelyLoadedCallback, dataService);
        var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm(); 
        vm.navigateToMessages();
		 
        equal(hash, "MessageTab");
    }); 

    function fakeCompletelyLoadedCallback() { }

    function fakeReadyForInteractionCallback() { }

    function setup() {
        var ajax = {
            Ajax: function (options) {
                if (options.url === "../api/Schedule/FetchDayData") {
                    options.success({
						"UnReadMessageCount":2,
                        "Date": "2017-04-28",
                        "DisplayDate": "28/04/2017",
                        "IsToday": false, "Schedule": {
                            "TextRequestCount": 0,
                            "Date": "28/04/2017",
                            "FixedDate": "2017-04-28",
                            "State": 0,
                            "Header": {
                                "Title": "Friday",
                                "Date": "28/04/2017",
                                "DayDescription": "",
                                "DayNumber": "28"
                            },
                            "Note": {
                                 "Message": ""
                            },
                            "OvertimeAvailabililty": {
                                "HasOvertimeAvailability": false,
                                "StartTime": null,
                                "EndTime": null,
                                "EndTimeNextDay": false,
                                "DefaultStartTime": "16:00",
                                "DefaultEndTime": "17:00",
                                "DefaultEndTimeNextDay": false
                            }, "HasOvertime": false,
                            "IsFullDayAbsence": false,
                            "IsDayOff": false,
                            "Summary": {
                                "Title": "Early",
                                "TimeSpan": "07:00 - 16:00",
                                "StartTime": "0001-01-01T00:00:00",
                                "EndTime": "0001-01-01T00:00:00",
                                "Summary": "8:00",
                                "StyleClassName": "color_80FF80",
                                "Meeting": null,
                                "StartPositionPercentage": 0.0,
                                "EndPositionPercentage": 0.0,
                                "Color": "rgb(128,255,128)",
                                "IsOvertime": false
                            },
                            "Periods": [ {
                                    "Title": "Invoice", "TimeSpan": "07:00 - 08:00", "StartTime": "2017-04-28T07:00:00", "EndTime": "2017-04-28T08:00:00", "Summary": "1:00", "StyleClassName": "color_FF8080", "Meeting": null, "StartPositionPercentage": 0.0263157894736842105263157895, "EndPositionPercentage": 0.1315789473684210526315789474, "Color": "255,128,128", "IsOvertime": false
                                },
                                {
                                    "Title": "Phone", "TimeSpan": "08:00 - 09:00", "StartTime": "2017-04-28T08:00:00", "EndTime": "2017-04-28T09:00:00", "Summary": "1:00", "StyleClassName": "color_80FF80", "Meeting": null, "StartPositionPercentage": 0.1315789473684210526315789474, "EndPositionPercentage": 0.2368421052631578947368421053, "Color": "128,255,128", "IsOvertime": false
                                },
                                {
                                    "Title": "Short break", "TimeSpan": "09:00 - 09:15", "StartTime": "2017-04-28T09:00:00", "EndTime": "2017-04-28T09:15:00", "Summary": "0:15", "StyleClassName": "color_FF0000", "Meeting": null, "StartPositionPercentage": 0.2368421052631578947368421053, "EndPositionPercentage": 0.2631578947368421052631578947, "Color": "255,0,0", "IsOvertime": false
                                },
                                {
                                    "Title": "Phone", "TimeSpan": "09:15 - 11:00", "StartTime": "2017-04-28T09:15:00", "EndTime": "2017-04-28T11:00:00", "Summary": "1:45", "StyleClassName": "color_80FF80", "Meeting": null, "StartPositionPercentage": 0.2631578947368421052631578947, "EndPositionPercentage": 0.4473684210526315789473684211, "Color": "128,255,128", "IsOvertime": false
                                },
                                {
                                    "Title": "Lunch", "TimeSpan": "11:00 - 12:00", "StartTime": "2017-04-28T11:00:00", "EndTime": "2017-04-28T12:00:00", "Summary": "1:00", "StyleClassName": "color_FFFF00", "Meeting": null, "StartPositionPercentage": 0.4473684210526315789473684211, "EndPositionPercentage": 0.5526315789473684210526315789, "Color": "255,255,0", "IsOvertime": false
                                },
                                {
                                     "Title": "Phone", "TimeSpan": "12:00 - 14:00", "StartTime": "2017-04-28T12:00:00", "EndTime": "2017-04-28T14:00:00", "Summary": "2:00", "StyleClassName": "color_80FF80", "Meeting": null, "StartPositionPercentage": 0.5526315789473684210526315789, "EndPositionPercentage": 0.7631578947368421052631578947, "Color": "128,255,128", "IsOvertime": false
                                },
                                {
                                     "Title": "Short break", "TimeSpan": "14:00 - 14:15", "StartTime": "2017-04-28T14:00:00", "EndTime": "2017-04-28T14:15:00", "Summary": "0:15", "StyleClassName": "color_FF0000", "Meeting": null, "StartPositionPercentage": 0.7631578947368421052631578947, "EndPositionPercentage": 0.7894736842105263157894736842, "Color": "255,0,0", "IsOvertime": false
                                },
                                {
                                     "Title": "Phone", "TimeSpan": "14:15 - 16:00", "StartTime": "2017-04-28T14:15:00", "EndTime": "2017-04-28T16:00:00", "Summary": "1:45", "StyleClassName": "color_80FF80", "Meeting": null, "StartPositionPercentage": 0.7894736842105263157894736842, "EndPositionPercentage": 0.9736842105263157894736842105, "Color": "128,255,128", "IsOvertime": false
                                }],
                            "DayOfWeekNumber": 5,
                            "Availability": false,
                            "HasNote": false,
                            "ProbabilityClass": "",
                            "ProbabilityText": "",
                            "SeatBookings": []
                        },
                        "RequestPermission": { "TextRequestPermission": true, "AbsenceRequestPermission": true, "ShiftTradeRequestPermission": false, "OvertimeAvailabilityPermission": true, "AbsenceReportPermission": true, "ShiftExchangePermission": true, "ShiftTradeBulletinBoardPermission": true, "PersonAccountPermission": true
                        }, "TimeLine": [{
                             "Time": "06:45:00", "TimeLineDisplay": "06:45", "PositionPercentage": 0.0, "TimeFixedFormat": null
                        }, {
                             "Time": "07:00:00", "TimeLineDisplay": "07:00", "PositionPercentage": 0.0263157894736842105263157895, "TimeFixedFormat": null
                        }, {
                             "Time": "08:00:00", "TimeLineDisplay": "08:00", "PositionPercentage": 0.1315789473684210526315789474, "TimeFixedFormat": null
                        }, {
                             "Time": "09:00:00", "TimeLineDisplay": "09:00", "PositionPercentage": 0.2368421052631578947368421053, "TimeFixedFormat": null
                        }, {
                             "Time": "10:00:00", "TimeLineDisplay": "10:00", "PositionPercentage": 0.3421052631578947368421052632, "TimeFixedFormat": null
                        }, {
                             "Time": "11:00:00", "TimeLineDisplay": "11:00", "PositionPercentage": 0.4473684210526315789473684211, "TimeFixedFormat": null
                        }, {
                             "Time": "12:00:00", "TimeLineDisplay": "12:00", "PositionPercentage": 0.5526315789473684210526315789, "TimeFixedFormat": null
                        }, {
                             "Time": "13:00:00", "TimeLineDisplay": "13:00", "PositionPercentage": 0.6578947368421052631578947368, "TimeFixedFormat": null
                        }, {
                             "Time": "14:00:00", "TimeLineDisplay": "14:00", "PositionPercentage": 0.7631578947368421052631578947, "TimeFixedFormat": null
                        }, {
                             "Time": "15:00:00", "TimeLineDisplay": "15:00", "PositionPercentage": 0.8684210526315789473684210526, "TimeFixedFormat": null
                        }, {
                             "Time": "16:00:00", "TimeLineDisplay": "16:00", "PositionPercentage": 0.9736842105263157894736842105, "TimeFixedFormat": null
                        }, {
                             "Time": "16:15:00", "TimeLineDisplay": "16:15", "PositionPercentage": 1.0, "TimeFixedFormat": null
                        }],
                        "AsmPermission": true,
                        "ViewPossibilityPermission": false,
                        "DatePickerFormat": "dd/MM/yyyy",
                        "DaylightSavingTimeAdjustment": {
                             "StartDateTime": "2017-03-26T01:00:00", "EndDateTime": "2017-10-29T02:00:00", "AdjustmentOffsetInMinutes": 60.0
                        },
                        "BaseUtcOffsetInMinutes": 60.0,
                        "CheckStaffingByIntraday": false,
                        "Possibilities": [],
                        "SiteOpenHourIntradayPeriod": null
                    });
                }
            }
        };
        dataService = new Teleopti.MyTimeWeb.Schedule.MobileStartDay.DataService(ajax);
	    this.crossroads = {
		    addRoute: function () { }
        };
	    this.hasher = {
		    initialized: {
			    add: function () { }
		    },
		    changed: {
			    add: function () { }
		    },
            init: function () { },
            setHash: function (data) { hash = data; }
	    };

        initPortal();

        Teleopti.MyTimeWeb.UserInfo = {
            WhenLoaded: function (whenLoadedCallBack) {
                var data = { WeekStart: "" };
                whenLoadedCallBack(data);
            }
        };
    }

    function initPortal() {
        this.crossroads = {
            addRoute: function () { }
        };

        var setting = getDefaultSetting();
        var fakeWindow = getFakeWindow();
        var commonAjax = {
            Ajax: function (options) {
            }
        };
        Teleopti.MyTimeWeb.Portal.Init(setting, fakeWindow, commonAjax);
	}

    function getDefaultSetting() {
        return {
            defaultNavigation: '/',
            baseUrl: '/',
            startBaseUrl: '/'
        };
    }
    function getFakeWindow() {
        return {
            location: {
                hash: "#",
                url: "",
                replace: function (newUrl) {
                    this.url = newUrl;
                }
            },
            navigator: {
                userAgent: "Android"
            }
        };
    }
})