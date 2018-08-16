describe('<shift-editor>', function () {
	'use strict';

	var $rootScope,
		$compile,
		$document,
		fakeActivityService,
		fakeShiftEditorService,
		fakeTeamSchedule,
		mockSignalRBackendServer = {},
		fakeNoticeService;

	beforeEach(module('wfm.templates', 'wfm.teamSchedule'));
	beforeEach(module(function ($provide) {
		$provide.service('Toggle', function () { return { WfmTeamSchedule_ShowInformationForUnderlyingSchedule_74952: true } });
		$provide.service('CurrentUserInfo', function () {
			return {
				CurrentUserInfo: function () {
					return {
						DefaultTimeZone: "Europe/Berlin",
						DateFormatLocale: "sv-SE"
					};
				}
			};
		});
		$provide.service('TimezoneDataService', function () {
			return {
				getAll: function () {
					return {
						then: function (callback) {
							callback({
								Timezones: [
									{
										IanaId: "Asia/Hong_Kong",
										Name: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
									}, {
										IanaId: "Europe/Berlin",
										Name: "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
									}
								]
							});
						}
					};
				}
			};
		});
		$provide.service('ActivityService', function () {
			fakeActivityService = new FakeActivityService();
			return fakeActivityService;
		});
		$provide.service('ShiftEditorService', function () {
			fakeShiftEditorService = new FakeShiftEditorService();
			return fakeShiftEditorService;
		});
		$provide.service('signalRSVC', setupMockSignalRService);

		fakeTeamSchedule = new FakeTeamSchedule();
		$provide.service('TeamSchedule', function () { return fakeTeamSchedule; });

		fakeNoticeService = new FakeNoticeService();
		$provide.service('NoticeService', function () { return fakeNoticeService; });
	}));
	beforeEach(inject(function (_$rootScope_, _$compile_, _$document_) {
		$rootScope = _$rootScope_;
		$compile = _$compile_;
		$document = _$document_;
	}));
	beforeEach(function () { moment.locale('sv'); });
	afterEach(function () { moment.locale('en'); });

	it("should render correctly", function () {
		var schedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-05-28 08:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		};
		fakeTeamSchedule.has(schedule);

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-28', 'Europe/Berlin');
		var element = panel[0];

		expect(element.querySelector('.timezone').innerText.trim()).toEqual('UTC+01:00');
		expect(element.querySelector('.name').innerText.trim()).toEqual('Annika Andersson');
		expect(element.querySelector('.date').innerText.trim()).toEqual('2018-05-28');
		expect(!!element.querySelector('.btn-save')).toBeTruthy();
		expect(!!element.querySelector('.btn-back')).toBeTruthy();
		expect(!!element.querySelector('.btn-refresh').disabled).toBeTruthy();
		expect(!!element.querySelector('.text-danger')).toBeFalsy();
	});

	it("should highlight the selected date time labels", function () {
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-28', 'Europe/Berlin');
		var element = panel[0];

		var timeLabels = element.querySelectorAll(".interval .label");
		expect(timeLabels[0].className.indexOf("highlight") >= 0).toBeFalsy();
		expect(timeLabels[25].className.indexOf("highlight") >= 0).toBeTruthy();
		expect(timeLabels[49].className.indexOf("highlight") >= 0).toBeFalsy();
	});

	it("should show earth icon unless the agent timezone is not same with selected timezone", function () {
		var schedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-06-07",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-06-07 08:45",
				"End": "2018-06-07 10:45",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Asia/Hong_Kong" }
		};
		fakeTeamSchedule.has(schedule);

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-07', 'Europe/Berlin');
		var element = panel[0];
		expect(!!element.querySelector(".mdi-earth")).toBeTruthy();

		panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-07', 'Asia/Hong_Kong');
		expect(!!panel[0].querySelector(".mdi-earth")).toBeFalsy();;
	})

	it('should show underlying info icon if schedule has underlying activities', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			Name: 'Agent 1',
			Date: '2018-05-16',
			UnderlyingScheduleSummary: {
				"PersonalActivities": [{
					"Description": "personal activity",
					"Start": scheduleDate + ' 10:00',
					"End": scheduleDate + ' 11:00'
				}]
			},
			"Timezone": { "IanaId": "Europe/Berlin" }
		});


		var scheduleDate = "2018-05-16";
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", '2018-05-16', "Europe/Berlin");

		var element = panel[0];
		expect(element.querySelectorAll('.underlying-info').length).toBe(1);
	});

	it('should show schedule correctly', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-05-28 08:45",
				"End": "2018-05-28 10:45",
				"Minutes": 120,
				"IsOvertime": false
			}, {
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-05-28 10:45",
				"End": "2018-05-28 11:00",
				"Minutes": 15,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-28', 'Europe/Berlin');
		var element = panel[0];

		var shiftLayers = element.querySelectorAll('.shift-layer');
		expect(shiftLayers[0].style.width).toEqual('120px');
		expect(shiftLayers[0].style.left).toEqual(((24 + 8) * 60 + 45) + 'px');
		expect(shiftLayers[1].style.width).toEqual('15px');
		expect(shiftLayers[1].style.left).toEqual(((24 + 10) * 60 + 45) + 'px');

	});

	it('should show schedule correctly on DST', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-03-25",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-03-25 01:00",
				"End": "2018-03-25 03:00",
				"Minutes": 120,
				"IsOvertime": false
			}, {
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-03-25 03:00",
				"End": "2018-03-25 05:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-03-25', 'Europe/Berlin');
		var element = panel[0];

		var shiftLayers = element.querySelectorAll('.shift-layer');
		expect(shiftLayers[0].style.width).toEqual('60px');
		expect(shiftLayers[0].style.left).toEqual((24 + 1) * 60 + 'px');
		expect(shiftLayers[1].style.width).toEqual('120px');
		expect(shiftLayers[1].style.left).toEqual((24 + 2) * 60 + 'px');

	});

	it('should able to select an activity', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-05-28 08:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-05-28", "Europe/Berlin");

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeTruthy();
	});

	it('should not allow select intraday absence', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-06-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-06-28 08:00",
				"Minutes": 240,
				"IsOvertime": false
			},
			{
				"ShiftLayerIds": null,
				"ParentPersonAbsences": ['eba97a99-8d36-47c4-b326-a90d006bac52'],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-06-28 09:00",
				"Minutes": 60,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-28", "Europe/Berlin");

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[1].click();

		expect(shiftLayers[1].className.indexOf('selected') >= 0).toBeFalsy();
		expect(shiftLayers[1].className.indexOf('disabled') >= 0).toBeTruthy();
	});

	it('should not allow select meeting', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-06-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-06-28 08:00",
				"Minutes": 240,
				"IsOvertime": false
			},
			{
				"ShiftLayerIds": null,
				"ParentPersonAbsences": null,
				"Color": "#ffffff",
				"Description": "Administration",
				"Start": "2018-06-28 09:00",
				"Minutes": 60,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-28", "Europe/Berlin");

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[1].click();

		expect(shiftLayers[1].className.indexOf('selected') >= 0).toBeFalsy();
		expect(shiftLayers[1].className.indexOf('disabled') >= 0).toBeTruthy();
	});

	it('should can select only one activity', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-05-28 08:00",
				"Minutes": 120,
				"IsOvertime": false
			}, {
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49623"],
				"Color": "#808080",
				"Description": "Phone",
				"Start": "2018-05-28 10:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-05-28", "Europe/Berlin");

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();
		shiftLayers[1].click();

		expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeFalsy();
		expect(shiftLayers[1].className.indexOf('selected') >= 0).toBeTruthy();
	});

	it('should show divide line if personal activity interset another same type regular activity', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ActivityId": "472e02c8-1a84-4064-9a3b-9b5e015ab3c6",
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#808080",
				"Description": "E-mail",
				"Start": "2018-05-28 08:00",
				"Minutes": 60,
				"IsOvertime": false
			}, {
				"ActivityId": "472e02c8-1a84-4064-9a3b-9b5e015ab3c6",
				"ShiftLayerIds": ["71678e5a-ac3f-4daa-9577-a83800e49623"],
				"Color": "#808080",
				"Description": "E-mail",
				"Start": "2018-05-28 09:00",
				"Minutes": 60,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-05-28", "Europe/Berlin");

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		expect(shiftLayers[1].className.indexOf('divide-line') >= 0).toBeTruthy();
	});

	it('should clear selection when click the selected activity again', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-05-28 08:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-05-28", "Europe/Berlin");

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();
		shiftLayers[0].click();

		expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeFalsy();
		var activityInfoEl = panel[0].querySelector(".activity-info");
		expect(!!activityInfoEl.innerText.trim()).toBeFalsy();
	});

	it('should show border color correctly based on the selected activity color', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-05-28 08:00",
				"Minutes": 120,
				"IsOvertime": false
			}, {
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49623"],
				"Color": "#8080c0",
				"Description": "Phone",
				"Start": "2018-05-28 10:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-05-28", "Europe/Berlin");

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();
		expect(shiftLayers[0].className.indexOf('border-dark') >= 0).toBeTruthy();
		shiftLayers[1].click();
		expect(shiftLayers[1].className.indexOf('border-light') >= 0).toBeTruthy();
	});

	it('should show activity information correctly when select an activity', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-05-28 08:00",
				"End": "2018-05-28 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": "0ffeb898-11bf-43fc-8104-9b5e015ab3c2"

			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-05-28", "Europe/Berlin");

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var timespanEl = panel[0].querySelector(".timespan");
		var typeEl = panel[0].querySelector(".activity-selector md-select-value");

		expect(timespanEl.innerText.trim()).toBe("2018-05-28 08:00 - 2018-05-28 09:00");
		expect(typeEl.innerText.trim()).toBe("Phone");
	});

	it('should show time period of activity information correctly on DST when select an activity', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-03-25",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-03-25 00:30",
				"End": "2018-03-25 03:30",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-03-25", "Europe/Berlin");

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var timespanEl = panel[0].querySelector(".timespan");
		expect(timespanEl.innerText.trim()).toBe("2018-03-25 00:30 - 2018-03-25 03:30");
	});

	it('should list all activity type when select an activity', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-06-15",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 09:00",
				"Minutes": 60,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var typeOptions = panel[0].querySelectorAll('.activity-selector md-option');
		expect(typeOptions.length).toBe(5);

	});

	it('should set correct activity type when select an activity', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-06-15",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": "0ffeb898-11bf-43fc-8104-9b5e015ab3c2"
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var selectedTypeEl = panel[0].querySelector('.activity-selector md-select-value');
		var typeColorEl = panel[0].querySelector('.activity-selector md-select-value div .type-color');
		expect(selectedTypeEl.innerText.trim()).toBe('Phone');
		expect(typeColorEl.style.backgroundColor).toBe('rgb(255, 255, 255)');
	});

	it('should change shift layer activity', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-06-15",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");
		var vm = panel.isolateScope().vm;

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();
		expect(shiftLayers[0].className.indexOf('border-dark') >= 0).toBeTruthy();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		expect(shiftLayers[0].style.backgroundColor).toBe('rgb(255, 0, 0)');
		expect(shiftLayers[0].className.indexOf('border-light') >= 0).toBeTruthy();
		expect(vm.scheduleVm.ShiftLayers[0].CurrentActivityId).toEqual('5c1409de-a0f1-4cd4-b383-9b5e015ab3c6');
		expect(vm.scheduleVm.ShiftLayers[0].Description).toEqual('Invoice');

		shiftLayers[0].click();
		shiftLayers[0].click();

		var currentActivityEl = panel[0].querySelector(".activity-selector md-select-value");
		expect(currentActivityEl.innerText.trim()).toBe("Invoice");
	});

	it('should disable save button when there is nothing changed ', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-06-15",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");
		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var saveButton = panel[0].querySelector(".btn-save");
		expect(saveButton.disabled).toBeTruthy();
	});

	it('should disable save button when change is back', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-06-15",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");
		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		typeEls[2].click();

		var saveButton = panel[0].querySelector(".btn-save");
		expect(saveButton.disabled).toBeTruthy();
	});

	it('should enable save button when has changes although some of those changes are back', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-06-15",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			},
			{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Email",
				"Start": "2018-06-15 09:00",
				"End": "2018-06-15 10:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");
		var shiftLayers = panel[0].querySelectorAll(".shift-layer");

		shiftLayers[0].click();
		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();
		shiftLayers[0].click();

		shiftLayers[1].click();
		typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();
		shiftLayers[1].click();

		shiftLayers[0].click();
		typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[2].click();

		var saveButton = panel[0].querySelector(".btn-save");
		expect(saveButton.disabled).toBeFalsy();
	});

	it('should able to extend start time with a proper value after resizing an selected activity', function () {		
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-08-13",
			"WorkTimeMinutes": 60,
			"ContractTimeMinutes": 60,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-08-13 08:00",
				"End": "2018-08-13 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-08-13", "Europe/Berlin");
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelector(".shift-layer");
		shiftLayer.click();

		var interact = vm.scheduleVm.ShiftLayers[0].interact;

		interact.fire({
			type: 'resizemove',
			target: shiftLayer,
			rect: {
				width: 120
			},
			deltaRect: { left: -60 }
		});
		interact.fire({
			type: 'resizeend',
			target: shiftLayer
		});
		
		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual("2018-08-13 07:00 - 2018-08-13 09:00");
	});

	it('should able to shorten start time with a proper value after resizing an selected activity', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-08-16",
			"WorkTimeMinutes": 60,
			"ContractTimeMinutes": 60,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-08-16 08:00",
				"End": "2018-08-16 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-08-16", "Europe/Berlin");
		var vm = panel.isolateScope().vm;
		
		var shiftLayer = panel[0].querySelector(".shift-layer");
		shiftLayer.click();

		var interact = vm.scheduleVm.ShiftLayers[0].interact;
		interact.fire({
			type: 'resizemove',
			target: shiftLayer,
			rect: {
				width: 27
			},
			deltaRect: { left: 33 }
		});

		interact.fire({
			type: 'resizeend',
			target: shiftLayer
		});

		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual("2018-08-16 08:35 - 2018-08-16 09:00");
	});

	it('should able to extend end time with a proper value after resizing an selected activity', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-08-13",
			"WorkTimeMinutes": 60,
			"ContractTimeMinutes": 60,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-08-13 08:00",
				"End": "2018-08-13 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-08-13", "Europe/Berlin");
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelector(".shift-layer");
		shiftLayer.click();

		var interact = vm.scheduleVm.ShiftLayers[0].interact;
		interact.fire({
			type: 'resizemove',
			target: shiftLayer,
			rect: {
				width: 122.5
			},
			deltaRect: { left: 0 }
		});

		interact.fire({
			type: 'resizeend',
			target: shiftLayer
		});

		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual("2018-08-13 08:00 - 2018-08-13 10:00");
	});

	it('should able to shorten end time with a proper value after resizing an selected activity', function () {
		fakeTeamSchedule.has({
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-08-13",
			"WorkTimeMinutes": 60,
			"ContractTimeMinutes": 60,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-08-13 08:00",
				"End": "2018-08-13 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-08-13", "Europe/Berlin");
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelector(".shift-layer");
		shiftLayer.click();

		var interact = vm.scheduleVm.ShiftLayers[0].interact;
		interact.fire({
			type: 'resizemove',
			target: shiftLayer,
			rect: {
				width: 29
			},
			deltaRect: { left: 0 }
		});

		interact.fire({
			type: 'resizeend',
			target: shiftLayer
		});

		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual("2018-08-13 08:00 - 2018-08-13 08:30");
	});

	it('should save changes with correct data', function () {
		var date = "2018-06-15";
		var personId = "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22";
		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}, {
				"ShiftLayerIds": ["88878e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Invoice",
				"Start": "2018-06-15 09:00",
				"End": "2018-06-15 10:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6'
			},
			{
				"ShiftLayerIds": ["99978e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 10:00",
				"End": "2018-06-15 11:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			},
			{
				"ShiftLayerIds": null,
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 10:00",
				"End": "2018-06-15 11:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");
		var vm = panel.isolateScope().vm;

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [{ ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6', ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'] }],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	it('should save changes with correct data when change activity type for part of base activity and should reload schedule after saving changes', function () {
		var date = "2018-06-15";
		var personId = "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22";
		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 10:00",
				"Minutes": 120,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}, {
				"ShiftLayerIds": ["91678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#FFa2a2",
				"Description": "E-mail",
				"Start": "2018-06-15 10:00",
				"End": "2018-06-15 11:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
			},
			{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 11:00",
				"End": "2018-06-15 12:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var scope = $rootScope.$new();
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin", scope);
		var vm = panel.isolateScope().vm;
		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[2].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["xxxxxx-a0f1-4cd4-b383-9b5e015ab3c6"],
				"Color": "#ffffff",
				"Description": "Invoice",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 10:00",
				"Minutes": 120,
				"IsOvertime": false,
				"ActivityId": '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6'
			}, {
				"ShiftLayerIds": ["91678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#FFa2a2",
				"Description": "E-mail",
				"Start": "2018-06-15 10:00",
				"End": "2018-06-15 11:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
			},
			{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 11:00",
				"End": "2018-06-15 12:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();


		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [{
				ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
				ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
				StartTime: '2018-06-15 11:00',
				EndTime: '2018-06-15 12:00',
				IsNew: true
			}],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});

		expect(vm.scheduleVm.ShiftLayers[0].ShiftLayerIds[0]).toEqual('xxxxxx-a0f1-4cd4-b383-9b5e015ab3c6');
	});

	it('should save changes with correct data when change activity type for part of an activity that has underlying layers', function () {
		var date = "2018-06-15";
		var personId = "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22";
		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["layer1"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 10:00",
				"Minutes": 120,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}, {
				"ShiftLayerIds": ["layer2"],
				"Color": "#FFa2a2",
				"Description": "E-mail",
				"Start": "2018-06-15 10:00",
				"End": "2018-06-15 11:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
			},
			{
				"ShiftLayerIds": ["layer1", "layer3"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 11:00",
				"End": "2018-06-15 12:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");
		var vm = panel.isolateScope().vm;
		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[2].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [{
				ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
				ShiftLayerIds: ['layer1'],
				StartTime: '2018-06-15 11:00',
				EndTime: '2018-06-15 12:00',
				IsNew: true
			}],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	it('should save changes with correct data based on loggon user timezone when change activity type for part of base activity ', function () {
		var date = "2018-06-15";
		var personId = "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22";
		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 10:00",
				"Minutes": 120,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}, {
				"ShiftLayerIds": ["91678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#FFa2a2",
				"Description": "E-mail",
				"Start": "2018-06-15 10:00",
				"End": "2018-06-15 11:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
			},
			{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 11:00",
				"End": "2018-06-15 12:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Asia/Hong_Kong");
		var vm = panel.isolateScope().vm;
		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[2].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [{
				ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
				ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
				StartTime: '2018-06-15 11:00',
				EndTime: '2018-06-15 12:00',
				IsNew: true
			}],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	it('should save changes with correct data when change activity type for intersect same type activities', function () {
		var date = "2018-06-15";
		var personId = "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22";
		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["11678e5a-ac3f-4daa-9577-a83800e49622", "61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 10:00",
				"Minutes": 120,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");
		var vm = panel.isolateScope().vm;
		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [{ ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6', ShiftLayerIds: ["11678e5a-ac3f-4daa-9577-a83800e49622", "61678e5a-ac3f-4daa-9577-a83800e49622"] }],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	it('should save changes with correct data when change activity type for an activity which overlaps another same type activity completely', function () {
		var date = "2018-06-15";
		var personId = "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22";
		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["11678e5a-ac3f-4daa-9577-a83800e49622", "61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 10:00",
				"Minutes": 120,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
				"TopShiftLayerId": '11678e5a-ac3f-4daa-9577-a83800e49622'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");
		var vm = panel.isolateScope().vm;
		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [{ ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6', ShiftLayerIds: ["11678e5a-ac3f-4daa-9577-a83800e49622"] }],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	it('should save changes with correct data when change activity type for part of an activity that another part of this activity was covered by another activity completely ', function () {
		var date = "2018-06-15";
		var personId = "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22";
		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["layer1"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 10:00",
				"Minutes": 120,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}, {
				"ShiftLayerIds": ["layer2"],
				"Color": "#FFa2a2",
				"Description": "E-mail",
				"Start": "2018-06-15 10:00",
				"End": "2018-06-15 11:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
			},
			{
				"ShiftLayerIds": ["layer3", "layer1"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 11:00",
				"End": "2018-06-15 12:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
				"TopShiftLayerId": "layer3"
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");
		var vm = panel.isolateScope().vm;
		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[2].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [{
				ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
				ShiftLayerIds: ['layer3']
			}],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	it('should show error message if schedule was changed by others when saving changes', function () {
		var date = "2018-06-28";
		var personId = "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22";
		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["11678e5a-ac3f-4daa-9577-a83800e49622", "61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-28 08:00",
				"End": "2018-06-28 10:00",
				"Minutes": 120,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
				"TopShiftLayerId": '11678e5a-ac3f-4daa-9577-a83800e49622'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});
		var scope = $rootScope.$new();
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-28", "Europe/Berlin", scope);
		scope.$apply();

		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"StartDate": "D2018-06-28T00:00:00",
				"EndDate": "D2018-06-28T00:00:00",
				"TrackId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx"
			}
		]);

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();
		scope.$apply();

		var saveButton = panel[0].querySelector('.btn-save');
		expect(saveButton.disabled).toBeFalsy();
		saveButton.click();

		var errorEl = panel[0].querySelector(".text-danger");
		expect(!!errorEl).toBeTruthy();
		expect(saveButton.disabled).toBeTruthy();

	});

	it('should enable refresh button when schedule was changed by others', function () {
		var date = "2018-08-06";
		var personId = "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22";
		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["11678e5a-ac3f-4daa-9577-a83800e49622", "61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-08-06 08:00",
				"End": "2018-08-06 10:00",
				"Minutes": 120,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
				"TopShiftLayerId": '11678e5a-ac3f-4daa-9577-a83800e49622'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});
		var scope = $rootScope.$new();
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-08-06", "Europe/Berlin", scope);
		scope.$apply();

		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"StartDate": "D2018-08-05T20:00:00",
				"EndDate": "D2018-08-06T10:00:00",
				"TrackId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx"
			}
		]);
		scope.$apply();

		var refreshButton = panel[0].querySelector('.btn-refresh');
		expect(refreshButton.disabled).toBeFalsy();

	});

	it('should disable refresh button when schedule was changed by itself and enable save button after changing back to the previous type', function () {
		var date = "2018-06-28";
		var personId = "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22";
		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["11678e5a-ac3f-4daa-9577-a83800e49622", "61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-28 08:00",
				"End": "2018-06-28 10:00",
				"Minutes": 120,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
				"TopShiftLayerId": '11678e5a-ac3f-4daa-9577-a83800e49622'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});
		var scope = $rootScope.$new();
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-28", "Europe/Berlin", scope);
		scope.$apply();

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();
		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[2].click();
		shiftLayers[0].click();
		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		var vm = panel.isolateScope().vm;
		var newSchedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-06-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-06-28 08:00",
				"Minutes": 120,
				"IsOvertime": false,
				"ActivityId": "472e02c8-1a84-4064-9a3b-9b5e015ab3c6"
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		};
		fakeTeamSchedule.has(newSchedule);
		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"StartDate": "D2018-06-28T00:00:00",
				"EndDate": "D2018-06-28T00:00:00",
				"TrackId": vm.trackId
			}
		]);
		scope.$apply();

		shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var refreshButton = panel[0].querySelector('.btn-refresh');
		expect(saveButton.disabled).toBeFalsy();
		expect(refreshButton.disabled).toBeTruthy();

	});

	it('should get latest schedule and not show error message when click refresh data button and should be able to save changes after changed something', function () {
		var schedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-06-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-06-28 08:00",
				"Minutes": 120,
				"IsOvertime": false
			},
			{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#8080c0",
				"Description": "E-mail",
				"Start": "2018-06-28 10:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		};
		fakeTeamSchedule.has(schedule);

		var scope = $rootScope.$new();
		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-28", "Europe/Berlin", scope);

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var newSchedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-06-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-06-28 08:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		};
		fakeTeamSchedule.has(newSchedule);

		var vm = panel.isolateScope().vm;
		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"StartDate": "D2018-06-28T00:00:00",
				"EndDate": "D2018-06-28T00:00:00",
				"TrackId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx"
			}
		]);
		scope.$apply();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		var refreshButton = panel[0].querySelector('.btn-refresh');
		refreshButton.click();

		var errorEl = panel[0].querySelector('.text-danger');
		expect(!!errorEl).toBeFalsy();

		saveButton = panel[0].querySelector('.btn-save');
		expect(saveButton.disabled).toBeTruthy();
		expect(refreshButton.disabled).toBeTruthy();

		shiftLayers = panel[0].querySelectorAll(".shift-layer");
		expect(shiftLayers.length).toEqual(1);
		shiftLayers[0].click();

		typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		saveButton = panel[0].querySelector('.btn-save');
		expect(saveButton.disabled).toBeFalsy();

	});

	it('should show succeed notification when saving changes successfully', function () {
		var date = "2018-06-15";
		var personId = "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22";
		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");
		var vm = panel.isolateScope().vm;

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeNoticeService.successMessage).toEqual('SuccessfulMessageForSavingScheduleChanges');
	});

	it('should show error notification when saving changes failed', function () {
		var date = "2018-06-15";
		var personId = "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22";
		fakeTeamSchedule.has({
			"PersonId": personId,
			"Name": "Annika Andersson",
			"Date": date,
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-06-15 08:00",
				"End": "2018-06-15 09:00",
				"Minutes": 60,
				"IsOvertime": false,
				"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		});

		var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-15", "Europe/Berlin");
		var vm = panel.isolateScope().vm;

		var shiftLayers = panel[0].querySelectorAll(".shift-layer");
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		fakeShiftEditorService.setSavingApplyResponseData([{
			PersonId: personId, ErrorMessages: ['Error happens']
		}]);

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(vm.scheduleVm.ShiftLayers[0].CurrentActivityId).toEqual('5c1409de-a0f1-4cd4-b383-9b5e015ab3c6');
		expect(!!fakeNoticeService.successMessage).toEqual(false);
		expect(fakeNoticeService.errorMessage).toEqual('Error happens');
	});

	describe("in locale en-UK", function () {
		beforeEach(function () { moment.locale('en-UK'); });
		afterEach(function () { moment.locale('en'); });

		it('should show time period of activity information correctly when select an activity', function () {
			fakeTeamSchedule.has({
				"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"Name": "Annika Andersson",
				"Date": "2018-06-11",
				"WorkTimeMinutes": 240,
				"ContractTimeMinutes": 240,
				"Projection": [{
					"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
					"Color": "#ffffff",
					"Description": "Phone",
					"Start": "2018-06-11 05:30",
					"End": "2018-06-11 07:30",
					"Minutes": 120,
					"IsOvertime": false
				}],
				"Timezone": { "IanaId": "Europe/Berlin" }
			});
			var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-06-11", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");
			shiftLayers[0].click();

			var timespanEl = panel[0].querySelector(".timespan");
			expect(timespanEl.innerText.trim()).toBe("06/11/2018 5:30 AM - 06/11/2018 7:30 AM");

		});

		it("should render date correctly", function () {
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-12', 'Europe/Berlin');
			var element = panel[0];

			expect(element.querySelector('.date').innerText.trim()).toEqual('06/12/2018');
		});

		it('should show time span correctly after resizing a selected activity', function () {
			fakeTeamSchedule.has({
				"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"Name": "Annika Andersson",
				"Date": "2018-08-13",
				"WorkTimeMinutes": 60,
				"ContractTimeMinutes": 60,
				"Projection": [{
					"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
					"Color": "#ffffff",
					"Description": "Phone",
					"Start": "2018-08-13 08:00",
					"End": "2018-08-13 09:00",
					"Minutes": 60,
					"IsOvertime": false,
					"ActivityId": '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}],
				"Timezone": { "IanaId": "Europe/Berlin" }
			});

			var scope = $rootScope.$new();
			var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", "2018-08-13", "Europe/Berlin", scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelector(".shift-layer");
			shiftLayer.click();

			var interact = vm.scheduleVm.ShiftLayers[0].interact;

			interact.fire({
				type: 'resizemove',
				target: shiftLayer,
				rect: {
					width: 120
				},
				deltaRect: { left: -60 }
			});
			interact.fire({
				type: 'resizeend',
				target: shiftLayer
			});
			scope.$apply();

			var timespanEl = panel[0].querySelector(".timespan");
			expect(timespanEl.innerText.trim()).toEqual("08/13/2018 7:00 AM - 08/13/2018 9:00 AM");
		});
	});

	describe("in locale fa", function () {
		beforeEach(function () { moment.locale('fa'); });
		afterEach(function () { moment.locale('en'); });

		it("should render schedule date correctly", function () {
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-24', 'Europe/Berlin');
			var element = panel[0];

			expect(element.querySelector('.date').innerText.trim()).toEqual('۲۴/۰۵/۲۰۱۸');
		});
	});

	function setUp(personId, date, timezone, scope) {
		scope = $rootScope.$new();
		scope.personId = personId;
		scope.date = date;
		scope.timezone = timezone;

		var element = $compile('<shift-editor date="date" timezone="timezone" person-id="personId"></shift-editor>')(scope);
		scope.$apply();

		return element;
	}

	function FakeActivityService() {
		var activities = [
			{
				"Id": "472e02c8-1a84-4064-9a3b-9b5e015ab3c6",
				"Name": "E-mail",
				"Color": "#FFa2a2"
			},
			{
				"Id": "5c1409de-a0f1-4cd4-b383-9b5e015ab3c6",
				"Name": "Invoice",
				"Color": "#FF0000"
			},
			{
				"Id": "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
				"Name": "Phone",
				"Color": "#ffffff"
			},
			{
				"Id": "84db44f4-22a8-44c7-b376-a0a200da613e",
				"Name": "Sales",
				"Color": "#FFCCA2"
			},
			{
				"Id": "35e33821-862f-461c-92db-9f0800a8d095",
				"Name": "Social Media",
				"Color": "#FFA2CC"
			}
		];
		this.fetchAvailableActivities = function () {
			return {
				then: function (callback) {
					callback(activities);
				}
			}
		}
	}

	function FakeShiftEditorService() {
		this.lastRequestData = {};
		var applyResponse = { data: [] };
		this.setSavingApplyResponseData = function (data) {
			applyResponse.data = data;
		}
		this.changeActivityType = function (date, personId, layers, trackCommandInfo) {
			this.lastRequestData = {
				Date: date,
				PersonId: personId,
				Layers: layers,
				TrackedCommandInfo: trackCommandInfo
			};

			return {
				then: function (callback) { callback(applyResponse); }
			}
		}
	}


	function setupMockSignalRService() {
		mockSignalRBackendServer.subscriptions = [];

		return {
			subscribeBatchMessage: function (options, messageHandler, timeout) {
				mockSignalRBackendServer.subscriptions.push(options);
				mockSignalRBackendServer.notifyClients = messageHandler;
			}
		};
	}


	function FakeTeamSchedule() {
		var schedules = [];
		this.has = function (schedule) {
			if (!schedules.length || (schedules[0].PersonId == schedule.PersonId && schedules[0].Date == schedule.Date)) {
				schedules = [];
				schedules.push(schedule);
			}

		}
		this.getSchedules = function () {
			return {
				then: function (callback) {
					callback({ Schedules: schedules });
				}
			};

		}
	}

	function setupMockSignalRService() {
		mockSignalRBackendServer.subscriptions = [];

		return {
			subscribeBatchMessage: function (options, messageHandler, timeout) {
				mockSignalRBackendServer.subscriptions.push(options);
				mockSignalRBackendServer.notifyClients = messageHandler;
			}
		};
	}

	function FakeNoticeService() {
		this.successMessage = '';
		this.errorMessage = '';
		this.warningMessage = '';
		this.success = function (message, time, destroyOnStateChange) {
			this.successMessage = message;
		}
		this.error = function (message, time, destroyOnStateChange) {
			this.errorMessage = message;
		}
		this.warning = function (message, time, destroyOnStateChange) {
			this.warningMessage = message;
		}
	}

});

describe('#shiftEditorController#', function () {
	var $controller, stateParams;
	beforeEach(function () {
		module('wfm.teamSchedule');
		module(function ($provide) {
			stateParams = {
				personId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				timezone: 'Europe/Berlin',
				date: '2018-05-28'
			};

			$provide.service('$stateParams', function () { return stateParams; });
		});
	});
	beforeEach(inject(function (_$controller_) {
		$controller = _$controller_;
	}));

	it('should set up correctly', function () {
		var target = setUp();
		expect(target.personId).toEqual('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22');
		expect(target.timezone).toEqual('Europe/Berlin');
		expect(target.date).toEqual('2018-05-28');
	});

	function setUp() {
		return $controller("ShiftEditorViewController");
	}

});
