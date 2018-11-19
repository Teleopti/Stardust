$(document).ready(function() {
	module('Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel', {
		setup: function() {
			Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_DayScheduleForStartPage_Command_44209');
		},
		teardown: function() {
			Teleopti.MyTimeWeb.Common.DisableToggle('MyTimeWeb_DayScheduleForStartPage_Command_44209');
		}
	});

	var resetLocale = function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			DateFormat: 'YYYY-MM-DD',
			UseJalaaliCalendar: false
		});
	};

	var constants = Teleopti.MyTimeWeb.Common.Constants;

	function fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(formattedDate) {
		var result = [];
		for (var i = 0; i < (24 * 60) / constants.probabilityIntervalLengthInMinute; i++) {
			result.push({
				Date: formattedDate,
				StartTime: moment(formattedDate)
					.startOf('day')
					.add(constants.probabilityIntervalLengthInMinute * i, 'minutes')
					.format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: moment(formattedDate)
					.startOf('day')
					.add(constants.probabilityIntervalLengthInMinute * (i + 1), 'minutes')
					.format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: constants.probabilityIntervalLengthInMinute * i < 12 * 60 ? 0 : 1
			});
		}
		return result;
	}

	test('should get current date', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		equal(viewModel.selectedDate().format('YYYY-MM-DD'), moment().format('YYYY-MM-DD'));
	});

	test('should set display date', function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			DateFormat: 'DD/MM/YYYY',
			UseJalaaliCalendar: false
		});
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.displayDate(), moment().format('DD/MM/YYYY'));
	});

	test('should display date correctly when in Persian date format', function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			DateFormat: 'DD/MM/YYYY',
			UseJalaaliCalendar: true
		});
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.displayDate(), moment(rawData.Date).format(Teleopti.MyTimeWeb.Common.DateFormat));

		resetLocale();
	});

	test('should set summary color', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: '0, 255, 0',
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.summaryColor(), rawData.Schedule.Summary.Color);
	});

	test('should set text color to white or black according to summary color', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: 'rgb(0,128,128)', //deep cobalt blue
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.summaryColor(), rawData.Schedule.Summary.Color);
		equal(viewModel.textColor(), 'white');

		rawData = {
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: 'rgb(159,224,35)', //light green
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.summaryColor(), rawData.Schedule.Summary.Color);
		equal(viewModel.textColor(), 'black');
	});

	test('should set summary name', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: 'Early',
					TimeSpan: null
				},
				Header: { Title: null }
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.summaryName(), rawData.Schedule.Summary.Title);
	});

	test('should set summary time', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: '9:00-18:00'
				},
				Header: { Title: null }
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.summaryTime(), rawData.Schedule.Summary.TimeSpan);
	});

	test('should set whether current day is day off', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null },
				IsDayOff: true
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.isDayOff, rawData.Schedule.IsDayOff);
	});

	test('should set empty layers when there is no period', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		var rawData = {
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null },
				IsDayOff: true,
				Periods: []
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.layers().length, 0);
	});

	test('should set timespan with +1 for overnight activity', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null },
				Periods: [
					{
						Title: 'Phone',
						TimeSpan: '22:00 - 04:00 +1',
						StartTime: moment()
							.subtract('day', 1)
							.startOf('day')
							.add('hour', 22)
							.format('YYYY-MM-DDTHH:mm:ss'),
						EndTime: moment()
							.add('day', 1)
							.add('hour', 4)
							.format('YYYY-MM-DDTHH:mm:ss'),
						Summary: '22:00 - 04:00 +1',
						StyleClassName: 'color_80FF80',
						Meeting: null,
						StartPositionPercentage: 0.1896551724137931034482758621,
						EndPositionPercentage: 1,
						Color: '128,255,128',
						IsOvertime: false
					}
				]
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.layers().length, 1);
		equal(viewModel.layers()[0].timeSpan, '22:00 - 04:00 +1');
	});

	test('should call out menu list when clicking plus icon at bottom right', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		viewModel.showMenu();

		equal(viewModel.menuIsVisible(), true);
	});

	test('should hide menu list or request form when clicking on menu item or outside', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		viewModel.showMenu();
		viewModel.hideMenuAndRequestForm();

		equal(viewModel.menuIsVisible(), false);
		equal(viewModel.focusingRequestForm(), false);
	});

	test('should hide plus icon after calling out menu list', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		viewModel.showMenu();

		equal(viewModel.menuIconIsVisible(), false);
	});

	test('should hide post shift trade menu if viewing day is out of open period', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			ShiftTradeRequestSetting: {
				HasWorkflowControlSet: true,
				NowDay: moment().format('D'),
				NowMonth: moment().format('M'),
				NowYear: moment().format('YYYY'),
				OpenPeriodRelativeEnd: 99,
				OpenPeriodRelativeStart: 1
			},
			RequestPermission: {
				ShiftExchangePermission: true
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.showPostShiftTradeMenu(), false);
	});

	test('should show post shift trade menu when viewing day is in open period', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		var rawData = {
			Date: moment()
				.add('days', 1)
				.format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			ShiftTradeRequestSetting: {
				HasWorkflowControlSet: true,
				NowDay: moment().format('D'),
				NowMonth: moment().format('M'),
				NowYear: moment().format('YYYY'),
				OpenPeriodRelativeEnd: 99,
				OpenPeriodRelativeStart: 1
			},
			RequestPermission: {
				ShiftExchangePermission: true
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.showPostShiftTradeMenu(), true);
	});

	test("should reset form and menu status after click 'Cancel' in request forms", function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		viewModel.showAddTextRequestForm();
		equal(viewModel.requestViewModel() != undefined, true);
		equal(viewModel.menuIconIsVisible(), false);
		equal(viewModel.focusingRequestForm(), true);

		viewModel.requestViewModel().CancelAddingNewRequest();
		equal(viewModel.requestViewModel(), undefined);
		equal(viewModel.menuIconIsVisible(), true);
		equal(viewModel.focusingRequestForm(), false);
	});

	test('should not show overtime availability command item without permission', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			RequestPermission: {
				OvertimeAvailabilityPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.overtimeAvailabilityPermission(), false);
	});

	test('should not show absence reporting command item without permission', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			RequestPermission: {
				AbsenceReportPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.absenceReportPermission(), false);
	});

	test('should not show absence reporting command item if selected date is neigther today nor tomorrow', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			RequestPermission: {
				AbsenceReportPermission: true
			}
		};
		viewModel.readData(rawData);

		equal(viewModel.absenceReportPermission(), true);
		equal(viewModel.showAbsenceReportingCommandItem(), true);

		rawData.Date = moment()
			.add('day', 3)
			.format('YYYY-MM-DD');
		viewModel.readData(rawData);

		equal(viewModel.absenceReportPermission(), true);
		equal(viewModel.showAbsenceReportingCommandItem(), false);
	});

	test('should not show text request command item without permission', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			RequestPermission: {
				TextRequestPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.textRequestPermission(), false);
	});

	test('should not show absence request command item without permission', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			RequestPermission: {
				AbsenceRequestPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.absenceRequestPermission(), false);
	});

	test('should not show shift trade request command item without permission', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			RequestPermission: {
				ShiftTradeRequestPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.shiftTradeRequestPermission(), false);
	});

	test('should not show post shift for trade command item without permission', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			RequestPermission: {
				ShiftExchangePermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.showPostShiftTradeMenu(), false);
	});

	test('should not show add overtime request command item without permission', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			RequestPermission: {
				OvertimeRequestPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.showAddOvertimeRequestMenu(), false);
	});

	test('should not show probability option icon without permission', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			ViewPossibilityPermission: false
		};
		viewModel.readData(rawData);
		equal(viewModel.showProbabilityOptionsToggleIcon(), false);
	});

	test('should not show menu icon when has no permission for any operation', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			ViewPossibilityPermission: false,
			RequestPermission: {
				OvertimeAvailabilityPermission: false,
				AbsenceReportPermission: false,
				TextRequestPermission: false,
				AbsenceRequestPermission: false,
				ShiftTradeRequestPermission: false,
				PersonAccountPermission: false,
				OvertimeRequestPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.isCommandEnable(), false);
	});

	test('should show menu icon when has at least one permission for operation', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			ViewPossibilityPermission: false,
			RequestPermission: {
				OvertimeAvailabilityPermission: true,
				AbsenceReportPermission: false,
				TextRequestPermission: false,
				AbsenceRequestPermission: false,
				ShiftTradeRequestPermission: false,
				PersonAccountPermission: false,
				OvertimeRequestPermission: false
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.isCommandEnable(), true);
	});

	test('should not show absence probability option item if CheckStaffingByIntraday is not toggled on ', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel()
		var rawData = {
			Date: moment
				.utc()
				.zone(-60)
				.format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			ViewPossibilityPermission: true,
			CheckStaffingByIntraday: false,
			AbsenceProbabilityEnabled: true,
			OvertimeProbabilityEnabled: true,
			StaffingInfoAvailableDays: 14
		};
		viewModel.readData(rawData);
		equal(viewModel.showProbabilityOptionsToggleIcon(), true);
		equal(viewModel.absenceProbabilityEnabled(), false);
	});

	test('should not show absence probability option item if Absence Probability is toggled off in fat client ', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel()
		var rawData = {
			Date: moment
				.utc()
				.zone(-60)
				.format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			ViewPossibilityPermission: true,
			CheckStaffingByIntraday: true,
			AbsenceProbabilityEnabled: false,
			OvertimeProbabilityEnabled: true,
			StaffingInfoAvailableDays: 14
		};
		viewModel.readData(rawData);
		equal(viewModel.showProbabilityOptionsToggleIcon(), true);
		equal(viewModel.absenceProbabilityEnabled(), false);
	});

	test('should not show overtime probability option item if Overtime Probability is toggled off in fat client ', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel()
		var rawData = {
			Date: moment
				.utc()
				.zone(-60)
				.format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			ViewPossibilityPermission: true,
			CheckStaffingByIntraday: true,
			AbsenceProbabilityEnabled: false,
			OvertimeProbabilityEnabled: false,
			StaffingInfoAvailableDays: 14
		};
		viewModel.readData(rawData);
		equal(viewModel.showProbabilityOptionsToggleIcon(), false);
		equal(viewModel.overtimeProbabilityEnabled(), false);
	});

	test("should hide probability after selecting 'Hide probability' ", function() {
		var fakeParent = {
			ReloadSchedule: function() {}
		};
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel(null, fakeParent, null);
		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			StaffingInfoAvailableDays: 14,
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null },
				Periods: [
					{
						Title: 'Phone',
						TimeSpan: '09:30 - 16:45',
						StartTime: moment()
							.startOf('day')
							.add('hour', 9)
							.add('minute', 30)
							.format('YYYY-MM-DDTHH:mm:ss'),
						EndTime: moment()
							.add('hour', 16)
							.add('minute', 45)
							.format('YYYY-MM-DDTHH:mm:ss'),
						Summary: '7:15',
						StyleClassName: 'color_80FF80',
						Meeting: null,
						StartPositionPercentage: 0.1896551724137931034482758621,
						EndPositionPercentage: 1,
						Color: '128,255,128',
						IsOvertime: false
					}
				]
			},
			RequestPermission: {
				ShiftExchangePermission: false
			},
			ViewPossibilityPermission: true,
			AbsenceProbabilityEnabled: true,
			OvertimeProbabilityEnabled: true,
			TimeLine: [
				{
					Time: '09:15:00',
					TimeLineDisplay: '09:15',
					PositionPercentage: 0,
					TimeFixedFormat: null
				},
				{
					Time: '17:00:00',
					TimeLineDisplay: '17:00',
					PositionPercentage: 1,
					TimeFixedFormat: null
				}
			]
		};
		viewModel.readData(rawData);

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(
			viewModel.selectedDate().format('YYYY-MM-DD')
		);
		viewModel.toggleProbabilityOptionsPanel();
		viewModel.requestViewModel().model.onOptionSelected(constants.probabilityType.overtime);
		viewModel.updateProbabilityData(fakeProbabilityData);
		equal(viewModel.selectedProbabilityOptionValue(), constants.probabilityType.overtime);
		equal(viewModel.probabilities().length, 2);

		viewModel.toggleProbabilityOptionsPanel();
		viewModel.requestViewModel().model.onOptionSelected(constants.probabilityType.none);
		equal(viewModel.selectedProbabilityOptionValue(), constants.probabilityType.none);
		equal(viewModel.probabilities().length, 0);
	});

	test("should change probability option value to absence(1) after selecting 'Show absence probability' ", function() {
		var fakeParent = {
			ReloadSchedule: function() {}
		};
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel(null, fakeParent, null);
		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			},
			ViewPossibilityPermission: true,
			AbsenceProbabilityEnabled: true,
			CheckStaffingByIntraday: true
		};
		viewModel.readData(rawData);

		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(
			viewModel.selectedDate().format('YYYY-MM-DD')
		);
		viewModel.updateProbabilityData(fakeProbabilityData);
		viewModel.toggleProbabilityOptionsPanel();

		equal(viewModel.requestViewModel().model.checkedProbability(), constants.probabilityType.none);
		viewModel.requestViewModel().model.onOptionSelected(constants.probabilityType.absence);
		equal(viewModel.selectedProbabilityOptionValue(), constants.probabilityType.absence);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show overtime probability within timeline range', function() {
		Teleopti.MyTimeWeb.Common.TimeFormat = 'HH:mm';
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null },
				Periods: [
					{
						Title: 'Phone',
						TimeSpan: '09:30 - 16:45',
						StartTime: moment()
							.startOf('day')
							.add('hour', 9)
							.add('minute', 30)
							.format('YYYY-MM-DDTHH:mm:ss'),
						EndTime: moment()
							.add('hour', 16)
							.add('minute', 45)
							.format('YYYY-MM-DDTHH:mm:ss'),
						Summary: '7:15',
						StyleClassName: 'color_80FF80',
						Meeting: null,
						StartPositionPercentage: 0.1896551724137931034482758621,
						EndPositionPercentage: 1,
						Color: '128,255,128',
						IsOvertime: false
					}
				]
			},
			RequestPermission: {
				ShiftExchangePermission: false
			},
			ViewPossibilityPermission: true,
			OvertimeProbabilityEnabled: true,
			TimeLine: [
				{
					Time: '09:15:00',
					TimeLineDisplay: '09:15',
					PositionPercentage: 0,
					TimeFixedFormat: null
				},
				{
					Time: '17:00:00',
					TimeLineDisplay: '17:00',
					PositionPercentage: 1,
					TimeFixedFormat: null
				}
			]
		};
		viewModel.readData(rawData);

		viewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(
			viewModel.selectedDate().format('YYYY-MM-DD')
		);
		viewModel.updateProbabilityData(fakeProbabilityData);
		equal(viewModel.probabilities().length, 2);
		equal(
			viewModel
				.probabilities()[0]
				.tooltips()
				.indexOf('09:30 - 12:00') > -1,
			true
		);
		equal(
			viewModel
				.probabilities()[1]
				.tooltips()
				.indexOf('12:00 - 16:45') > -1,
			true
		);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show correct absence possibility for cross day schedule', function() {
		Teleopti.MyTimeWeb.Common.TimeFormat = 'HH:mm';
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();

		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null },
				Periods: [
					{
						Title: 'Phone',
						TimeSpan: '09:30 - 16:45',
						StartTime: moment()
							.subtract('day', 1)
							.startOf('day')
							.add('hour', 22)
							.add('minute', 30)
							.format('YYYY-MM-DDTHH:mm:ss'),
						EndTime: moment()
							.add('hour', 16)
							.add('minute', 45)
							.format('YYYY-MM-DDTHH:mm:ss'),
						Summary: '18:15',
						StyleClassName: 'color_80FF80',
						Meeting: null,
						StartPositionPercentage: 0.1896551724137931034482758621,
						EndPositionPercentage: 1,
						Color: '128,255,128',
						IsOvertime: false
					}
				]
			},
			RequestPermission: {
				ShiftExchangePermission: false
			},
			AbsenceProbabilityEnabled: true,
			OvertimeProbabilityEnabled: true,
			ViewPossibilityPermission: true,
			TimeLine: [
				{
					Time: '00:00:00',
					TimeLineDisplay: '00:00',
					PositionPercentage: 0,
					TimeFixedFormat: null
				},
				{
					Time: '17:00:00',
					TimeLineDisplay: '17:00',
					PositionPercentage: 1,
					TimeFixedFormat: null
				}
			]
		};
		viewModel.readData(rawData);

		viewModel.selectedProbabilityOptionValue(constants.probabilityType.overtime);
		var fakeProbabilityData = fakeProbabilitiesDataLowBeforeTwelveAndHighAfter(
			viewModel.selectedDate().format('YYYY-MM-DD')
		);
		viewModel.updateProbabilityData(fakeProbabilityData);
		equal(viewModel.probabilities().length, 2);
		equal(
			viewModel
				.probabilities()[0]
				.tooltips()
				.indexOf('00:00 - 12:00') > -1,
			true
		);
		equal(
			viewModel
				.probabilities()[1]
				.tooltips()
				.indexOf('12:00 - 16:45') > -1,
			true
		);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should show probability icon when scheduled', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null },
				HasNotScheduled: false
			},
			StaffingInfoAvailableDays: 14,
			ViewPossibilityPermission: true,
			CheckStaffingByIntraday: true,
			AbsenceProbabilityEnabled: true
		};
		viewModel.readData(rawData);

		equal(viewModel.showProbabilityOptionsToggleIcon(), true);
	});

	test('should show probability icon on not scheduled day', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null },
				HasNotScheduled: true
			},
			ViewPossibilityPermission: true,
			CheckStaffingByIntraday: true,
			AbsenceProbabilityEnabled: true,
			StaffingInfoAvailableDays: 14
		};
		viewModel.readData(rawData);

		equal(viewModel.showProbabilityOptionsToggleIcon(), true);
	});

	test('should not show probability option icon when OvertimeProbability and AbsenceProbability are disabled', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel()
		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			OvertimeProbabilityEnabled: false,
			AbsenceProbabilityEnabled: false,
			ViewPossibilityPermission: true,
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null }
			}
		};
		viewModel.readData(rawData);
		equal(viewModel.showProbabilityOptionsToggleIcon(), false);
	});

	test('should show message icon when asmEnabled', function() {
		var viewModel = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		var rawData = {
			Date: moment().format('YYYY-MM-DD'),
			Schedule: {
				FixedDate: null,
				Summary: {
					Color: null,
					Title: null,
					TimeSpan: null
				},
				Header: { Title: null },
				HasNotScheduled: false
			},
			AsmEnabled: true
		};
		viewModel.readData(rawData);

		equal(viewModel.asmEnabled(), true);
	});
});
