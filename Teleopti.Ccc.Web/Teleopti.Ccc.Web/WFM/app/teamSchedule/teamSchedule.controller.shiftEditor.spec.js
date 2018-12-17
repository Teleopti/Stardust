describe('<shift-editor>', function () {
	'use strict';

	var $rootScope,
		$compile,
		fakeActivityService,
		fakeShiftEditorService,
		fakeTeamSchedule,
		mockSignalRBackendServer = {},
		fakeNoticeService;

	beforeEach(module('wfm.templates', 'wfm.teamSchedule'));
	beforeEach(
		module(function ($provide) {
			$provide.service('Toggle', function () {
				return { };
			});
			
			$provide.service('TimezoneDataService', function () {
				return {
					getAll: function () {
						return {
							then: function (callback) {
								callback({
									Timezones: [
										{
											IanaId: 'Asia/Hong_Kong',
											Name: '(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi'
										},
										{
											IanaId: 'Europe/Berlin',
											Name: '(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna'
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
			$provide.service('TeamSchedule', function () {
				return fakeTeamSchedule;
			});

			fakeNoticeService = new FakeNoticeService();
			$provide.service('NoticeService', function () {
				return fakeNoticeService;
			});
		})
	);

	beforeEach(
		inject(function (_$rootScope_, _$compile_, CurrentUserInfo) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
			CurrentUserInfo.SetCurrentUserInfo({
				DefaultTimeZone: 'Europe/Berlin',
				DateFormatLocale: 'sv-SE'
			});
		})
	);
	beforeEach(function () {
		moment.locale('sv');
	});
	afterEach(function () {
		moment.locale('en');
	});

	it('should render correctly', function () {
		var schedule = {
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-05-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-05-28 08:00',
					Minutes: 120,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
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

	it('should highlight the selected date time labels', function () {
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-28', 'Europe/Berlin');
		var element = panel[0];

		var timeLabels = element.querySelectorAll('.interval .label');
		expect(timeLabels[0].className.indexOf('highlight') >= 0).toBeFalsy();
		expect(timeLabels[25].className.indexOf('highlight') >= 0).toBeTruthy();
		expect(timeLabels[49].className.indexOf('highlight') >= 0).toBeFalsy();
	});

	it('should show earth icon unless the agent timezone is not same with selected timezone', function () {
		var schedule = {
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-07',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-06-07 08:45',
					End: '2018-06-07 10:45',
					Minutes: 120,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Asia/Hong_Kong' }
		};
		fakeTeamSchedule.has(schedule);

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-07', 'Europe/Berlin');
		var element = panel[0];
		expect(!!element.querySelector('.mdi-earth')).toBeTruthy();

		panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-07', 'Asia/Hong_Kong');
		expect(!!panel[0].querySelector('.mdi-earth')).toBeFalsy();
	});

	it('should show underlying info icon if schedule has underlying activities', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Agent 1',
			Date: '2018-05-16',
			UnderlyingScheduleSummary: {
				PersonalActivities: [
					{
						Description: 'personal activity',
						Start: scheduleDate + ' 10:00',
						End: scheduleDate + ' 11:00'
					}
				]
			},
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scheduleDate = '2018-05-16';
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-16', 'Europe/Berlin');

		var element = panel[0];
		expect(element.querySelectorAll('.underlying-info').length).toBe(1);
	});

	it('should show schedule correctly', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-05-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-05-28 08:45',
					End: '2018-05-28 10:45',
					Minutes: 120,
					IsOvertime: false
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-05-28 10:45',
					End: '2018-05-28 11:00',
					Minutes: 15,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-28', 'Europe/Berlin');
		var element = panel[0];

		var shiftLayers = element.querySelectorAll('.shift-layer');
		expect(shiftLayers[0].style.width).toEqual('120px');
		expect(shiftLayers[0].style.left).toEqual((24 + 8) * 60 + 45 + 'px');
		expect(shiftLayers[1].style.width).toEqual('15px');
		expect(shiftLayers[1].style.left).toEqual((24 + 10) * 60 + 45 + 'px');
	});

	it('should show schedule correctly on DST', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-03-25',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-03-25 01:00',
					End: '2018-03-25 03:00',
					Minutes: 120,
					IsOvertime: false
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-03-25 03:00',
					End: '2018-03-25 05:00',
					Minutes: 120,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
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
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-05-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-05-28 08:00',
					Minutes: 120,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-28', 'Europe/Berlin');

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeTruthy();
	});

	it('should not allow select intraday absence', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-06-28 08:00',
					Minutes: 240,
					IsOvertime: false
				},
				{
					ShiftLayerIds: null,
					ParentPersonAbsences: ['eba97a99-8d36-47c4-b326-a90d006bac52'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-06-28 09:00',
					Minutes: 60,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-28', 'Europe/Berlin');

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[1].click();

		expect(shiftLayers[1].className.indexOf('selected') >= 0).toBeFalsy();
		expect(shiftLayers[1].className.indexOf('disabled') >= 0).toBeTruthy();
	});

	it('should not allow select meeting', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-06-28 08:00',
					Minutes: 240,
					IsOvertime: false
				},
				{
					ShiftLayerIds: null,
					ParentPersonAbsences: null,
					Color: '#ffffff',
					Description: 'Administration',
					Start: '2018-06-28 09:00',
					Minutes: 60,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-28', 'Europe/Berlin');

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[1].click();

		expect(shiftLayers[1].className.indexOf('selected') >= 0).toBeFalsy();
		expect(shiftLayers[1].className.indexOf('disabled') >= 0).toBeTruthy();
	});

	it('should can select only one activity', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-05-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-05-28 08:00',
					Minutes: 120,
					IsOvertime: false
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49623'],
					Color: '#808080',
					Description: 'Phone',
					Start: '2018-05-28 10:00',
					Minutes: 120,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-28', 'Europe/Berlin');

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();
		shiftLayers[1].click();

		expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeFalsy();
		expect(shiftLayers[1].className.indexOf('selected') >= 0).toBeTruthy();
	});

	it('should show divide line if personal activity interset another same type regular activity', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-05-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6',
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#808080',
					Description: 'E-mail',
					Start: '2018-05-28 08:00',
					Minutes: 60,
					IsOvertime: false
				},
				{
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6',
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49623'],
					Color: '#808080',
					Description: 'E-mail',
					Start: '2018-05-28 09:00',
					Minutes: 60,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-28', 'Europe/Berlin');

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers[1].className.indexOf('divide-line') >= 0).toBeTruthy();
	});

	it('should clear selection when click the selected activity again', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-05-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-05-28 08:00',
					Minutes: 120,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-28', 'Europe/Berlin');

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();
		shiftLayers[0].click();

		expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeFalsy();
		var activityInfoEl = panel[0].querySelector('.activity-info');
		expect(!!activityInfoEl.innerText.trim()).toBeFalsy();
	});

	it('should show border color correctly based on the selected activity color', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-05-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-05-28 08:00',
					Minutes: 120,
					IsOvertime: false
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49623'],
					Color: '#303030',
					Description: 'Phone',
					Start: '2018-05-28 10:00',
					Minutes: 120,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-28', 'Europe/Berlin');

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();
		expect(shiftLayers[0].className.indexOf('border-dark') >= 0).toBeTruthy();
		shiftLayers[1].click();
		expect(shiftLayers[1].className.indexOf('border-light') >= 0).toBeTruthy();
	});

	it('should show activity information correctly when select an activity', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-05-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-05-28 08:00',
					End: '2018-05-28 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-05-28', 'Europe/Berlin');

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var timespanEl = panel[0].querySelector('.timespan');
		var typeEl = panel[0].querySelector('.activity-selector md-select-value');

		expect(timespanEl.innerText.trim()).toBe('2018-05-28 08:00 - 2018-05-28 09:00');
		expect(typeEl.innerText.trim()).toBe('Phone');
	});

	it('should show time period of activity information correctly on DST when select an activity', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-03-25',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-03-25 00:30',
					End: '2018-03-25 03:30',
					Minutes: 120,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-03-25', 'Europe/Berlin');

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var timespanEl = panel[0].querySelector('.timespan');
		expect(timespanEl.innerText.trim()).toBe('2018-03-25 00:30 - 2018-03-25 03:30');
	});

	it('should list all activity type when select an activity', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-15',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 09:00',
					Minutes: 60,
					IsOvertime: false
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var typeOptions = panel[0].querySelectorAll('.activity-selector md-option');
		expect(typeOptions.length).toBe(7);
	});

	it('should set correct activity type when select an activity', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-15',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var selectedTypeEl = panel[0].querySelector('.activity-selector md-select-value');
		var typeColorEl = panel[0].querySelector('.activity-selector md-select-value div .type-color');
		expect(selectedTypeEl.innerText.trim()).toBe('Phone');
		expect(typeColorEl.style.backgroundColor).toBe('rgb(255, 255, 255)');
	});

	it('should change shift layer activity', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-15',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin', scope);

		var vm = panel.isolateScope().vm;

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();
		expect(shiftLayers[0].className.indexOf('border-dark') >= 0).toBeTruthy();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		scope.$apply();

		expect(shiftLayers[0].style.backgroundColor).toBe('rgb(255, 0, 0)');
		expect(shiftLayers[0].className.indexOf('border-light') >= 0).toBeTruthy();
		expect(vm.scheduleVm.ShiftLayers[0].Current.ActivityId).toEqual('5c1409de-a0f1-4cd4-b383-9b5e015ab3c6');
		expect(vm.scheduleVm.ShiftLayers[0].Current.Description).toEqual('Invoice');

		shiftLayers[0].click();
		shiftLayers[0].click();

		var currentActivityEl = panel[0].querySelector('.activity-selector md-select-value');
		expect(currentActivityEl.innerText.trim()).toBe('Invoice');
	});

	it('should disable save button when there is nothing changed ', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-15',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');
		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var saveButton = panel[0].querySelector('.btn-save');
		expect(saveButton.disabled).toBeTruthy();
	});

	it('should disable save button when change is back', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-15',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');
		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		typeEls[2].click();

		var saveButton = panel[0].querySelector('.btn-save');
		expect(saveButton.disabled).toBeTruthy();
	});

	it('should enable save button when has changes although some of those changes are back', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-15',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Email',
					Start: '2018-06-15 09:00',
					End: '2018-06-15 10:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');
		var shiftLayers = panel[0].querySelectorAll('.shift-layer');

		shiftLayers[0].click();
		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();
		shiftLayers[0].click();

		shiftLayers[1].click();
		typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[4].click();
		shiftLayers[1].click();

		shiftLayers[0].click();
		typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[2].click();

		var saveButton = panel[0].querySelector('.btn-save');
		expect(saveButton.disabled).toBeFalsy();
	});

	it('should able to extend start time with a proper value after resizing an selected activity', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-13',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-13 08:00',
					End: '2018-08-13 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-13', 'Europe/Berlin');

		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelector('.shift-layer');
		shiftLayer.click();

		fireResize(vm, shiftLayer, 120, -60);

		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual('2018-08-13 07:00 - 2018-08-13 09:00');
	});

	it('should able to shorten start time with a proper value after resizing an selected activity', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-16',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-16 08:00',
					End: '2018-08-16 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-16', 'Europe/Berlin');
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelector('.shift-layer');
		shiftLayer.click();

		fireResize(vm, shiftLayer, 27, 33);

		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual('2018-08-16 08:35 - 2018-08-16 09:00');
	});

	it('should able to extend end time with a proper value after resizing an selected activity', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-13',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-13 08:00',
					End: '2018-08-13 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-13', 'Europe/Berlin');
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelector('.shift-layer');
		shiftLayer.click();

		fireResize(vm, shiftLayer, 122.5, 0);

		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual('2018-08-13 08:00 - 2018-08-13 10:00');
	});

	it('should able to shorten end time with a proper value after resizing an selected activity', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-13',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-13 08:00',
					End: '2018-08-13 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-13', 'Europe/Berlin');
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelector('.shift-layer');
		shiftLayer.click();

		fireResize(vm, shiftLayer, 29, 0);
		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual('2018-08-13 08:00 - 2018-08-13 08:30');
	});

	it('should shorten start time of the next activity when extending the selected activity from the end time', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-16',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-16 08:00',
					End: '2018-08-16 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Email',
					Start: '2018-08-16 09:00',
					End: '2018-08-16 10:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-16', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 90, 0);

		shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		var nextShiftLayer = panel[0].querySelectorAll('.shift-layer')[1];

		expect(shiftLayer.style.width).toEqual('90px');
		expect(nextShiftLayer.style.width).toEqual('30px');
		expect(nextShiftLayer.style.transform).toEqual('translate(30px, 0px)');
		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual('2018-08-16 08:00 - 2018-08-16 09:30');
		expect(vm.scheduleVm.ShiftLayers[1].Current.TimeSpan).toEqual('2018-08-16 09:30 - 2018-08-16 10:00');
	});

	it('should shorten end time of the previous activity when extending the selected activity from the start time', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-17',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-17 08:00',
					End: '2018-08-17 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Email',
					Start: '2018-08-17 09:00',
					End: '2018-08-17 10:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-17', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[1];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 90, -30);

		var previousShiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		shiftLayer = panel[0].querySelectorAll('.shift-layer')[1];

		expect(previousShiftLayer.style.width).toEqual('30px');
		expect(shiftLayer.style.width).toEqual('90px');
		expect(shiftLayer.style.transform).toEqual('translate(-30px, 0px)');
		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual('2018-08-17 08:00 - 2018-08-17 08:30');
		expect(vm.scheduleVm.ShiftLayers[1].Current.TimeSpan).toEqual('2018-08-17 08:30 - 2018-08-17 10:00');
	});

	it('should extending start time of the next activity when shorten the selected activity from the end time ', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-17',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-17 08:00',
					End: '2018-08-17 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Email',
					Start: '2018-08-17 09:00',
					End: '2018-08-17 10:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-17', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 30, 0);

		var nextShiftLayer = panel[0].querySelectorAll('.shift-layer')[1];
		shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];

		expect(shiftLayer.style.width).toEqual('30px');
		expect(nextShiftLayer.style.width).toEqual('90px');
		expect(nextShiftLayer.style.transform).toEqual('translate(-30px, 0px)');
		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual('2018-08-17 08:00 - 2018-08-17 08:30');
		expect(vm.scheduleVm.ShiftLayers[1].Current.TimeSpan).toEqual('2018-08-17 08:30 - 2018-08-17 10:00');
	});

	it('should shorten start time of the next activity when extending the selected activity from the end time', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-16',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-16 08:00',
					End: '2018-08-16 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Email',
					Start: '2018-08-16 09:00',
					End: '2018-08-16 10:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-16', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 90, 0);

		shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		var nextShiftLayer = panel[0].querySelectorAll('.shift-layer')[1];

		expect(shiftLayer.style.width).toEqual('90px');
		expect(nextShiftLayer.style.width).toEqual('30px');
		expect(nextShiftLayer.style.transform).toEqual('translate(30px, 0px)');
		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual('2018-08-16 08:00 - 2018-08-16 09:30');
		expect(vm.scheduleVm.ShiftLayers[1].Current.TimeSpan).toEqual('2018-08-16 09:30 - 2018-08-16 10:00');
	});

	it('should extending end time of the previous activity when shorten the selected activity from the start time', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-17',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-17 08:00',
					End: '2018-08-17 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Email',
					Start: '2018-08-17 09:00',
					End: '2018-08-17 10:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-17', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[1];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 90, -30);

		shiftLayer.click();

		var previousShiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		previousShiftLayer.click();

		fireResize(vm, previousShiftLayer, 60, -30);

		previousShiftLayer.click();
		previousShiftLayer.click();

		fireResize(vm, previousShiftLayer, 120, 0);

		previousShiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		shiftLayer = panel[0].querySelectorAll('.shift-layer')[1];

		expect(shiftLayer.style.width).toEqual('30px');
		expect(shiftLayer.style.transform).toEqual('translate(30px, 0px)');
		expect(previousShiftLayer.style.width).toEqual('120px');
		expect(previousShiftLayer.style.transform).toEqual('translate(-30px, 0px)');

		expect(vm.scheduleVm.ShiftLayers[0].Current.TimeSpan).toEqual('2018-08-17 07:30 - 2018-08-17 09:30');
		expect(vm.scheduleVm.ShiftLayers[1].Current.TimeSpan).toEqual('2018-08-17 09:30 - 2018-08-17 10:00');
	});

	it('should remove activities when it covered completely after extending the selected activity by end time', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-17',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-17 08:00',
					End: '2018-08-17 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['ccc78e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Sales',
					Start: '2018-08-17 09:00',
					End: '2018-08-17 10:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
				},
				{
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Email',
					Start: '2018-08-17 10:00',
					End: '2018-08-17 11:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-17', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 150, 0);

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toEqual(2);
		expect(shiftLayers[1].style.width).toEqual('30px');
		expect(shiftLayers[1].style.transform).toEqual('translate(30px, 0px)');
	});

	it('should remove activities when it covered completely after extending the selected activity by start time', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-17',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-17 07:00',
					End: '2018-08-17 08:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-17 08:00',
					End: '2018-08-17 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Email',
					Start: '2018-08-17 09:00',
					End: '2018-08-17 10:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-17', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[2];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 150, -90);

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toEqual(2);
		expect(shiftLayers[0].style.width).toEqual('30px');
	});

	it('should keep the activity on top what is changed to lunch/short break when extending an activity to pass it', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-21',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#FFCCA2',
					Description: 'Sales',
					Start: '2018-08-21 06:00',
					End: '2018-08-21 07:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-21 07:00',
					End: '2018-08-21 08:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var phoneLayer = panel[0].querySelectorAll('.shift-layer')[1];
		phoneLayer.click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[5].click();

		var salesLayer = panel[0].querySelectorAll('.shift-layer')[0];
		salesLayer.click();

		fireResize(vm, salesLayer, 180, 0);

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toEqual(3);

		expect(shiftLayers[0].style.width).toEqual('60px');
		expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 204, 162)');
		expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

		expect(shiftLayers[1].style.width).toEqual('60px');
		expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

		expect(shiftLayers[2].style.width).toEqual('60px');
		expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 204, 162)');

	});

	it('should merge with the previous layer after changing its activity to the activity of the previous layer', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-30',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#FFCCA2',
					Description: 'Sales',
					Start: '2018-08-30 06:00',
					End: '2018-08-30 07:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-30 07:00',
					End: '2018-08-30 08:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-30', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var phoneLayer = panel[0].querySelectorAll('.shift-layer')[1];
		phoneLayer.click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[3].click();

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toEqual(1);
		expect(shiftLayers[0].style.width).toEqual('120px');
		expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 204, 162)');
	});

	it('should merge with the next layer after changing its activity to the activity of the next layer', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-30',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#FFCCA2',
					Description: 'Sales',
					Start: '2018-08-30 06:00',
					End: '2018-08-30 07:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-30 07:00',
					End: '2018-08-30 08:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-30', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var salesLayer = panel[0].querySelectorAll('.shift-layer')[0];
		salesLayer.click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[2].click();

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toEqual(1);
		expect(shiftLayers[0].style.width).toEqual('120px');
		expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
	});

	it('should not merge with the next layer after changing its activity to the activity of the next layer, but next layer is overtime', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-09-05',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#FFCCA2',
					Description: 'Sales',
					Start: '2018-09-05 06:00',
					End: '2018-09-05 07:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
				},
				{
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-09-05 07:00',
					End: '2018-09-05 08:00',
					Minutes: 60,
					IsOvertime: true,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-09-05', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var salesLayer = panel[0].querySelectorAll('.shift-layer')[0];
		salesLayer.click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[2].click();

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toEqual(2);
		expect(shiftLayers[0].style.width).toEqual('60px');
		expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');

		expect(shiftLayers[1].style.width).toEqual('60px');
		expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 255)');
	});

	it('should merge with the beside layers after changing its activity to the activity of the beside layers', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-31',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#FFCCA2',
					Description: 'Sales',
					Start: '2018-08-31 06:00',
					End: '2018-08-31 07:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-31 07:00',
					End: '2018-08-31 08:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#FFCCA2',
					Description: 'Sales',
					Start: '2018-08-31 08:00',
					End: '2018-08-31 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-31', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var phoneLayer = panel[0].querySelectorAll('.shift-layer')[1];
		phoneLayer.click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[3].click();

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');

		expect(shiftLayers.length).toEqual(1);
		expect(shiftLayers[0].style.width).toEqual('180px');
		expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 204, 162)');

		var timespanEl = panel[0].querySelector('.timespan');
		expect(timespanEl.innerText.trim()).toBe('2018-08-31 06:00 - 2018-08-31 09:00');
	});

	it('should merge to one layer and make the length of merged layer correctly', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-09-25',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#FFCCA2',
					Description: 'Sales',
					Start: '2018-09-25 06:00',
					End: '2018-09-25 07:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-09-25 07:00',
					End: '2018-09-25 08:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-09-25', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');

		shiftLayers[0].click();

		fireResize(vm, shiftLayers[0], 30, 30);

		shiftLayers[0].click();

		shiftLayers[1].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[3].click();

		shiftLayers = panel[0].querySelectorAll('.shift-layer');

		expect(shiftLayers.length).toBe(1);
		expect(shiftLayers[0].style.width).toEqual("90px");
		expect(shiftLayers[0].style.backgroundColor).toEqual("rgb(255, 204, 162)");

		var timespanEl = panel[0].querySelector('.timespan');
		expect(timespanEl.innerText.trim()).toBe('2018-09-25 06:30 - 2018-09-25 08:00');
	
	});

	it('should reject if extending an activity from the end time exceed 36 hours', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-31',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-31 07:00',
					End: '2018-08-31 08:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffa2a2',
					Description: 'Email',
					Start: '2018-08-31 08:00',
					End: '2018-08-31 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-31', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 60 * 37, 0);

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toBe(2);

		expect(shiftLayers[0].style.width).toEqual("60px");
		expect(shiftLayers[0].style.backgroundColor).toEqual("rgb(255, 255, 255)");

		expect(shiftLayers[1].style.width).toEqual("60px");
		expect(shiftLayers[1].style.backgroundColor).toEqual("rgb(255, 162, 162)");

		var timespanEl = panel[0].querySelector('.timespan');
		expect(timespanEl.innerText.trim()).toBe('2018-08-31 07:00 - 2018-08-31 08:00');
	})

	it('should reject if extending an activity from the start time exceed 36 hours', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-31',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-31 07:00',
					End: '2018-08-31 08:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffa2a2',
					Description: 'Email',
					Start: '2018-08-31 08:00',
					End: '2018-08-31 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-31', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 60 * 36, -60 * 35);

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toBe(2);

		expect(shiftLayers[0].style.width).toEqual("60px");
		expect(shiftLayers[0].style.backgroundColor).toEqual("rgb(255, 255, 255)");
		expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

		expect(shiftLayers[1].style.width).toEqual("60px");
		expect(shiftLayers[1].style.backgroundColor).toEqual("rgb(255, 162, 162)");

		var timespanEl = panel[0].querySelector('.timespan');
		expect(timespanEl.innerText.trim()).toBe('2018-08-31 07:00 - 2018-08-31 08:00');
	})

	it('should reject if the activity belongs to date is changed', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-08-31',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-31 07:00',
					End: '2018-08-31 08:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffa2a2',
					Description: 'Email',
					Start: '2018-08-31 08:00',
					End: '2018-08-31 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-31', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 60 * 9, -60 * 8);

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toBe(2);

		expect(shiftLayers[0].style.width).toEqual("60px");
		expect(shiftLayers[0].style.backgroundColor).toEqual("rgb(255, 255, 255)");
		expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

		expect(shiftLayers[1].style.width).toEqual("60px");
		expect(shiftLayers[1].style.backgroundColor).toEqual("rgb(255, 162, 162)");

		var timespanEl = panel[0].querySelector('.timespan');
		expect(timespanEl.innerText.trim()).toBe('2018-08-31 07:00 - 2018-08-31 08:00');
	});

	it('should resize activity if the belongs to date not changed in DST', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-09-18',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-09-18 02:00',
					End: '2018-09-18 03:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffa2a2',
					Description: 'Email',
					Start: '2018-09-18 03:00',
					End: '2018-09-18 04:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Asia/Hong_Kong' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-09-18', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 60 * 4, -60 * 3);

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toBe(2);

		expect(shiftLayers[0].style.width).toEqual("240px");
		expect(shiftLayers[0].style.backgroundColor).toEqual("rgb(255, 255, 255)");
		expect(shiftLayers[0].style.transform).toEqual('translate(-180px, 0px)');

		expect(shiftLayers[1].style.width).toEqual("60px");
		expect(shiftLayers[1].style.backgroundColor).toEqual("rgb(255, 162, 162)");

		var timespanEl = panel[0].querySelector('.timespan');
		expect(timespanEl.innerText.trim()).toBe('2018-09-17 23:00 - 2018-09-18 03:00');
	});

	it('can not resize the personal activity', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-31',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-31 07:00',
						End: '2018-08-31 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						IsPersonalActivity: true,
						IsFloatOnTop: true
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-31', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			expect(shiftLayer.className.indexOf('non-resizable') >= 0).toBeTruthy();
	});

	it('can not resize meeting', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-31',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-31 07:00',
						End: '2018-08-31 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						IsMeeting: true,
						IsFloatOnTop: true
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-31', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			expect(shiftLayer.className.indexOf('non-resizable') >= 0).toBeTruthy();
		});

	it('can not resize the intraday activity', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-31',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-31 07:00',
						End: '2018-08-31 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						ParentPersonAbsences: ['abeeb355-cb4d-4e3f-86c1-a94d0056e29c'],
						IsFloatOnTop: true
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-31', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			expect(shiftLayer.className.indexOf('non-resizable') >= 0).toBeTruthy();
		});

	it('can not resize the overtime activity', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-31',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-31 07:00',
						End: '2018-08-31 08:00',
						Minutes: 60,
						IsOvertime: true,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						IsFloatOnTop: true
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-31', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			expect(shiftLayer.className.indexOf('non-resizable') >= 0).toBeTruthy();
		});

	it('should merge with the same type activities even the new period did not cover it completely', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-09-12',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-09-12 09:00',
					End: '2018-09-12 10:00',
					Minutes: 60,
					IsOvertime: true,
					FloatOnTop: true,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#000000',
					Description: 'Email',
					Start: '2018-09-12 10:00',
					End: '2018-09-12 11:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-09-12 11:00',
					End: '2018-09-12 12:00',
					Minutes: 60,
					IsOvertime: true,
					FloatOnTop: true,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-09-12', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 150, 0);

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toEqual(1);

		expect(shiftLayers[0].style.width).toEqual('180px');
		expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
		expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');
	});

	it('can shorten an activity from the end time if the next activity is an overtime activity and have a gap between it', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-09-12',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [

				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-09-12 07:00',
					End: '2018-09-12 08:00',
					Minutes: 60,
					IsOvertime: true,
					FloatOnTop: true,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#000000',
					Description: 'Email',
					Start: '2018-09-12 10:00',
					End: '2018-09-12 11:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}

			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-09-12', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var shiftLayer = panel[0].querySelectorAll('.shift-layer')[1];
		shiftLayer.click();

		fireResize(vm, shiftLayer, 30, 30);

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toEqual(2);

		expect(shiftLayers[0].style.width).toEqual('60px');
		expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');

		expect(shiftLayers[1].style.width).toEqual('30px');
		expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(0, 0, 0)');
		expect(shiftLayers[1].style.transform).toEqual('translate(30px, 0px)');
	});

	it('should not merge if the beside activity is a personal activity', function () {
		fakeTeamSchedule.has({
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-09-17',
			WorkTimeMinutes: 60,
			ContractTimeMinutes: 60,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-09-17 09:00',
					End: '2018-09-17 10:00',
					Minutes: 60,
					IsOvertime: false,
					FloatOnTop: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#000000',
					Description: 'Email',
					Start: '2018-09-17 10:00',
					End: '2018-09-17 11:00',
					Minutes: 60,
					IsOvertime: false,
					IsPersonalActivity: true,
					FloatOnTop: true,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-09-17 11:00',
					End: '2018-09-17 12:00',
					Minutes: 60,
					IsOvertime: false,
					FloatOnTop: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-09-17', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;

		var emailLayer = panel[0].querySelectorAll('.shift-layer')[1];
		emailLayer.click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[2].click();

		emailLayer.click();

		var phoneLayer = panel[0].querySelectorAll('.shift-layer')[0];
		phoneLayer.click();

		fireResize(vm, phoneLayer, 210, 0);

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toEqual(3);

		expect(shiftLayers[0].style.width).toEqual('60px');
		expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
		expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

		expect(shiftLayers[1].style.width).toEqual('60px');
		expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 255)');

		expect(shiftLayers[2].style.width).toEqual('90px');
		expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 255)');
	});

	it('should do nothing if changing the activity from the start time and the step less than 5 mins', function() {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-09-14',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-09-14 07:00',
						End: '2018-09-14 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-09-14 08:00',
						End: '2018-09-14 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-09-14 09:00',
						End: '2018-09-14 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-09-14', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[2];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 58, 2);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);
			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');
			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 162, 162)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');
		});

	it('should do nothing if changing the activity from the end time and the step less than 5 mins', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-09-14',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-09-14 07:00',
						End: '2018-09-14 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-09-14 08:00',
						End: '2018-09-14 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-09-14 09:00',
						End: '2018-09-14 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-09-14', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 58, 0);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);
			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');
			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 162, 162)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');
		});

	describe('# keep the lunch/short break on the top when changing the selected activity start time#', function () {

		it('should keep lunch activity when it covered completely', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-17',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-17 07:00',
						End: '2018-08-17 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-17 08:00',
						End: '2018-08-17 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-17 09:00',
						End: '2018-08-17 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-17', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[2];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 120, -60);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);
			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');
			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 162, 162)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');
		});

		it('should not do any changes when the previous activity type is same as selected activity', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-17',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					,
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-17 07:00',
						End: '2018-08-17 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-17 08:00',
						End: '2018-08-17 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-17 09:00',
						End: '2018-08-17 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-17', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[2];
			shiftLayer.click();


			fireResize(vm, shiftLayer, 120, -90);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);

			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 162, 162)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 162, 162)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');
		});

		it('should change the start time of same type activity that before the lunch to new start time', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-21',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 05:00',
						End: '2018-08-21 06:00',
						Minutes: 30,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-21 06:00',
						End: '2018-08-21 07:00',
						Minutes: 30,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[4];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 330, -270);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);
			expect(shiftLayers[0].style.width).toEqual('210px');
			expect(shiftLayers[0].style.transform).toEqual('translate(-150px, 0px)');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');
			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');
		});

		it('should change first activity to new start time', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-21',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[2];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 210, -150);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);
			expect(shiftLayers[0].style.width).toEqual('90px');
			expect(shiftLayers[0].style.transform).toEqual('translate(-30px, 0px)');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');
			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');
		});

		it('should add a new activity when it go pass the lunch', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-30',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-30 07:00',
						End: '2018-08-30 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-30 08:00',
						End: '2018-08-30 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-30', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[1];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 180, -120);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);
			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');
		});

		it('should replace with a new activity', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-21',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#FFCCA2',
						Description: 'Sales',
						Start: '2018-08-21 06:00',
						End: '2018-08-21 07:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[3];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 240, -180);


			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);

			expect(shiftLayers[0].style.width).toEqual('120px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');
			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');
		});

		it('should add a new activity and resize the activity what is not covered completely', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-21',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#FFCCA2',
						Description: 'Sales',
						Start: '2018-08-21 06:00',
						End: '2018-08-21 07:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[3];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 210, -150);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(4);
			expect(shiftLayers[0].style.width).toEqual('30px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 204, 162)');
			expect(shiftLayers[1].style.width).toEqual('90px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 0)');
			expect(shiftLayers[3].style.width).toEqual('60px');
			expect(shiftLayers[3].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[3].style.transform).toEqual('translate(0px, 0px)');
		});

		it('should add same number of activities as the number of lunch/shourt break that were passed', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-21',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#FFCCA2',
						Description: 'Sales',
						Start: '2018-08-21 05:00',
						End: '2018-08-21 06:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ff0500',
						Description: 'Short Break',
						Start: '2018-08-21 06:00',
						End: '2018-08-21 06:15',
						Minutes: 15,
						IsOvertime: false,
						FloatOnTop: true,
						ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#FFCCA2',
						Description: 'Sales',
						Start: '2018-08-21 06:15',
						End: '2018-08-21 07:00',
						Minutes: 45,
						IsOvertime: false,
						ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 09:30',
						Minutes: 30,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 09:30',
						End: '2018-08-21 10:00',
						Minutes: 30,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[6];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 270, -240);


			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(6);
			expect(shiftLayers[0].style.width).toEqual('30px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 204, 162)');
			expect(shiftLayers[1].style.width).toEqual('30px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[2].style.width).toEqual('15px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 5, 0)');
			expect(shiftLayers[3].style.width).toEqual('105px');
			expect(shiftLayers[3].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[4].style.width).toEqual('60px');
			expect(shiftLayers[4].style.backgroundColor).toEqual('rgb(255, 255, 0)');
			expect(shiftLayers[5].style.width).toEqual('60px');
			expect(shiftLayers[5].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[5].style.transform).toEqual('translate(-30px, 0px)');
		});

		it('should fill with the previous activity of the lunch', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-24',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[2];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 30, 30);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(4);

			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('30px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 255)');

			expect(shiftLayers[3].style.width).toEqual('30px');
			expect(shiftLayers[3].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[3].style.transform).toEqual('translate(30px, 0px)');

		});

		it('should not do any changes if the previous activity of the lunch is a same type activity by shorten start time', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-24',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[2];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 30, 30);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);

			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(0, 0, 0)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');

		});

		it('should show whole activity info after it split by lunch/short break', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-28',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-28 05:00',
						End: '2018-08-28 06:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ff0500',
						Description: 'Short Break',
						Start: '2018-08-28 06:00',
						End: '2018-08-28 07:00',
						Minutes: 60,
						IsOvertime: false,
						FloatOnTop: true,
						ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-28 07:00',
						End: '2018-08-28 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-28 08:00',
						End: '2018-08-28 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-28 09:00',
						End: '2018-08-28 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-28', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[4];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 330, -270);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(5);
			expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeTruthy();
			expect(shiftLayers[1].className.indexOf('selected') >= 0).toBeFalsy();
			expect(shiftLayers[2].className.indexOf('selected') >= 0).toBeTruthy();
			expect(shiftLayers[3].className.indexOf('selected') >= 0).toBeFalsy();
			expect(shiftLayers[4].className.indexOf('selected') >= 0).toBeTruthy();
			var timespanEl = panel[0].querySelector('.timespan');
			expect(timespanEl.innerText.trim()).toBe('2018-08-28 04:30 - 2018-08-28 10:00');
		});

		it('should keep activity info same with the selected activity when create a new activity', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-29',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-29 07:00',
						End: '2018-08-29 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-29 08:00',
						End: '2018-08-29 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-29 09:00',
						End: '2018-08-29 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-29', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[2];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 210, -150);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(vm.scheduleVm.ShiftLayers[0].ShiftLayerIds[0]).toEqual('91678e5a-ac3f-4daa-9577-a83800e49622');
		});

		it('should merge with the beside layers after changing its activity to the activity of the beside layers', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-31',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-31 05:00',
						End: '2018-08-31 06:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-31 06:00',
						End: '2018-08-31 07:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-31 07:00',
						End: '2018-08-31 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-31 08:00',
						End: '2018-08-31 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-31 09:00',
						End: '2018-08-31 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-31', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[4];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 240, -180);

			var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
			typeEls[0].click();

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);

			expect(shiftLayers[0].style.width).toEqual('180px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 162, 162)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 162, 162)');

			var timespanEl = panel[0].querySelector('.timespan');
			expect(timespanEl.innerText.trim()).toBe('2018-08-31 05:00 - 2018-08-31 10:00');
		});

		it('can extend an activity if the last activity is an overtime activity and have a gap between it', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-09-12',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-09-12 06:00',
						End: '2018-09-12 07:00',
						Minutes: 60,
						IsOvertime: true,
						FloatOnTop: true,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#FFCCA2',
						Description: 'Sales',
						Start: '2018-09-12 09:00',
						End: '2018-09-12 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-09-12 10:00',
						End: '2018-09-12 11:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-09-12 11:00',
						End: '2018-09-12 11:30',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-09-12', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[3];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 180, -150);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(4);

			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

			expect(shiftLayers[1].style.width).toEqual('90px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(0, 0, 0)');

			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[3].style.width).toEqual('30px');
			expect(shiftLayers[3].style.backgroundColor).toEqual('rgb(0, 0, 0)');
		});
	});

	describe('# keep the lunch/short break on the top when changing the selected activity end time#', function () {

		it('should fill with the next activity of the lunch', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-24',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();


			fireResize(vm, shiftLayer, 30, 0);
			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(4);

			expect(shiftLayers[0].style.width).toEqual('30px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

			expect(shiftLayers[1].style.width).toEqual('30px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(0, 0, 0)');

			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[3].style.width).toEqual('60px');
			expect(shiftLayers[3].style.backgroundColor).toEqual('rgb(0, 0, 0)');


		});

		it('should not do any change if the next activity of the lunch is a same type activity', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-21',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 150, 0);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);

			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');
		});

		it('should not change the activity time when the beside activity of lunch is short break', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-09-12',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-09-12 07:00',
						End: '2018-09-12 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ff0500',
						Description: 'Short Break',
						Start: '2018-09-12 08:00',
						End: '2018-09-12 08:30',
						Minutes: 15,
						IsOvertime: false,
						FloatOnTop: true,
						ActivityId: 'sbs33821-862f-461c-92db-9f0800a8d056'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-09-12 08:30',
						End: '2018-09-12 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},

				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-09-12', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 30, 0);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);

			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

			expect(shiftLayers[1].style.width).toEqual('30px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 5, 0)');

			expect(shiftLayers[2].style.width).toEqual('30px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 0)');
		});

		it('should add a new activity when it go pass the lunch', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-30',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-30 07:00',
						End: '2018-08-30 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-30 08:00',
						End: '2018-08-30 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-30', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 180, 0);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);
			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 255)');
		});

		it('should keep lunch activity when it covered completely', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-24',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-24 07:00',
						End: '2018-08-24 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-24 08:00',
						End: '2018-08-24 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-24 09:00',
						End: '2018-08-24 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-24', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 120, 0);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);
			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 162, 162)');
		});

		it('should replace with a new activity', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-21',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 180, 0);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);

			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('60px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 255)');
		});

		it('should add a new activity and resize the activity what is not covered completely', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-21',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 07:30',
						Minutes: 30,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 07:30',
						End: '2018-08-21 08:00',
						Minutes: 30,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 150, 0);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(4);
			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('30px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 255)');

			expect(shiftLayers[3].style.width).toEqual('30px');
			expect(shiftLayers[3].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[3].style.transform).toEqual('translate(30px, 0px)');
		});

		it('should change last activity to new start time', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-21',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 210, 0);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);
			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('90px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');
		});

		it('should change the end time of same type activity that next to the lunch to new end time', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-21',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 07:30',
						End: '2018-08-21 08:00',
						Minutes: 30,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-21 10:00',
						End: '2018-08-21 10:30',
						Minutes: 30,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 10:30',
						End: '2018-08-21 11:00',
						Minutes: 30,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 240, 0);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);

			expect(shiftLayers[0].style.width).toEqual('30px');
			expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 255, 255)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('150px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 255, 255)');
			expect(shiftLayers[2].style.transform).toEqual('translate(0px, 0px)');
		});

		it('should add same number of activities as the number of lunch/shourt break that were passed', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-21',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#FFCCA2',
						Description: 'Sales',
						Start: '2018-08-21 05:00',
						End: '2018-08-21 06:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ff0500',
						Description: 'Short Break',
						Start: '2018-08-21 06:00',
						End: '2018-08-21 06:15',
						Minutes: 15,
						IsOvertime: false,
						FloatOnTop: true,
						ActivityId: 'sbs33821-862f-461c-92db-9f0800a8d056'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#FFCCA2',
						Description: 'Sales',
						Start: '2018-08-21 06:15',
						End: '2018-08-21 07:00',
						Minutes: 45,
						IsOvertime: false,
						ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 07:00',
						End: '2018-08-21 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-21 08:00',
						End: '2018-08-21 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-21 09:00',
						End: '2018-08-21 09:30',
						Minutes: 30,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-08-21 09:30',
						End: '2018-08-21 10:00',
						Minutes: 30,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-21', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 270, 0);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(6);
			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 204, 162)');

			expect(shiftLayers[1].style.width).toEqual('15px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 5, 0)');

			expect(shiftLayers[2].style.width).toEqual('105px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 204, 162)');

			expect(shiftLayers[3].style.width).toEqual('60px');
			expect(shiftLayers[3].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[4].style.width).toEqual('30px');
			expect(shiftLayers[4].style.backgroundColor).toEqual('rgb(255, 204, 162)');

			expect(shiftLayers[5].style.width).toEqual('30px');
			expect(shiftLayers[5].style.backgroundColor).toEqual('rgb(0, 0, 0)');
		});

		it('should show whole activity info after it split by lunch/short break', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-28',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-28 05:00',
						End: '2018-08-28 06:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ff0500',
						Description: 'Short Break',
						Start: '2018-08-28 06:00',
						End: '2018-08-28 07:00',
						Minutes: 60,
						IsOvertime: false,
						FloatOnTop: true,
						ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-28 07:00',
						End: '2018-08-28 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-28 08:00',
						End: '2018-08-28 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-28 09:00',
						End: '2018-08-28 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-28', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 330, 0);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(5);
			expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeTruthy();
			expect(shiftLayers[1].className.indexOf('selected') >= 0).toBeFalsy();
			expect(shiftLayers[2].className.indexOf('selected') >= 0).toBeTruthy();
			expect(shiftLayers[3].className.indexOf('selected') >= 0).toBeFalsy();
			expect(shiftLayers[4].className.indexOf('selected') >= 0).toBeTruthy();
			var timespanEl = panel[0].querySelector('.timespan');
			expect(timespanEl.innerText.trim()).toBe('2018-08-28 05:00 - 2018-08-28 10:30');
		});

		it('should merge with the previous layers after changing its activity to the activity of the beside layers', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-31',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-31 05:00',
						End: '2018-08-31 06:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-08-31 06:00',
						End: '2018-08-31 07:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-31 07:00',
						End: '2018-08-31 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-31 08:00',
						End: '2018-08-31 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},
					{
						ShiftLayerIds: ['71678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffa2a2',
						Description: 'Email',
						Start: '2018-08-31 09:00',
						End: '2018-08-31 10:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-31', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 240, 0);

			var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
			typeEls[0].click();

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(3);

			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(255, 162, 162)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('180px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(255, 162, 162)');

			var timespanEl = panel[0].querySelector('.timespan');
			expect(timespanEl.innerText.trim()).toBe('2018-08-31 05:00 - 2018-08-31 10:00');
		});

		it('can extend an activity if the last activity is an overtime activity and have a gap between it', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-09-12',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#000000',
						Description: 'Email',
						Start: '2018-09-12 06:00',
						End: '2018-09-12 07:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
					},
					{
						ShiftLayerIds: ['81678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffff00',
						Description: 'Lunch',
						Start: '2018-09-12 07:00',
						End: '2018-09-12 08:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '1ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						FloatOnTop: true
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#FFCCA2',
						Description: 'Sales',
						Start: '2018-09-12 08:00',
						End: '2018-09-12 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '84db44f4-22a8-44c7-b376-a0a200da613e'
					},
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-09-12 11:00',
						End: '2018-09-12 11:30',
						Minutes: 60,
						IsOvertime: true,
						FloatOnTop: true,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					},


				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-09-12', 'Europe/Berlin');
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelectorAll('.shift-layer')[0];
			shiftLayer.click();

			fireResize(vm, shiftLayer, 210, 0);

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			expect(shiftLayers.length).toEqual(4);

			expect(shiftLayers[0].style.width).toEqual('60px');
			expect(shiftLayers[0].style.backgroundColor).toEqual('rgb(0, 0, 0)');
			expect(shiftLayers[0].style.transform).toEqual('translate(0px, 0px)');

			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[1].style.backgroundColor).toEqual('rgb(255, 255, 0)');

			expect(shiftLayers[2].style.width).toEqual('90px');
			expect(shiftLayers[2].style.backgroundColor).toEqual('rgb(0, 0, 0)');

			expect(shiftLayers[3].style.width).toEqual('30px');
			expect(shiftLayers[3].style.backgroundColor).toEqual('rgb(255, 255, 255)');
		});
	});

	it('should save changes with correct data', function () {
		var date = '2018-06-15';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['88878e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Invoice',
					Start: '2018-06-15 09:00',
					End: '2018-06-15 10:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['99978e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 10:00',
					End: '2018-06-15 11:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: null,
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 10:00',
					End: '2018-06-15 11:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');
		var vm = panel.isolateScope().vm;

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [
				{
					ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622']
				}
			],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	it('should save changes with correct data when change activity type for part of base activity and should reload schedule after saving changes', function () {
		var date = '2018-06-15';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 10:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#FFa2a2',
					Description: 'E-mail',
					Start: '2018-06-15 10:00',
					End: '2018-06-15 11:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 11:00',
					End: '2018-06-15 12:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin', scope);
		var vm = panel.isolateScope().vm;
		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[2].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['xxxxxx-a0f1-4cd4-b383-9b5e015ab3c6'],
					Color: '#ffffff',
					Description: 'Invoice',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 10:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#FFa2a2',
					Description: 'E-mail',
					Start: '2018-06-15 10:00',
					End: '2018-06-15 11:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 11:00',
					End: '2018-06-15 12:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [
				{
					ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					StartTime: '2018-06-15 11:00',
					EndTime: '2018-06-15 12:00',
					IsNew: true
				}
			],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});

		expect(vm.scheduleVm.ShiftLayers[0].ShiftLayerIds[0]).toEqual('xxxxxx-a0f1-4cd4-b383-9b5e015ab3c6');
	});

	it('should save changes with correct data when change activity type for part of an activity that has underlying layers', function () {
		var date = '2018-06-15';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['layer1'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 10:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['layer2'],
					Color: '#FFa2a2',
					Description: 'E-mail',
					Start: '2018-06-15 10:00',
					End: '2018-06-15 11:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['layer1', 'layer3'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 11:00',
					End: '2018-06-15 12:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');
		var vm = panel.isolateScope().vm;
		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[2].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [
				{
					ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
					ShiftLayerIds: ['layer1'],
					StartTime: '2018-06-15 11:00',
					EndTime: '2018-06-15 12:00',
					IsNew: true
				}
			],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	it('should save changes with correct data based on loggon user timezone when change activity type for part of base activity ', function () {
		var date = '2018-06-15';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 10:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['91678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#FFa2a2',
					Description: 'E-mail',
					Start: '2018-06-15 10:00',
					End: '2018-06-15 11:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 11:00',
					End: '2018-06-15 12:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Asia/Hong_Kong');
		var vm = panel.isolateScope().vm;
		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[2].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [
				{
					ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					StartTime: '2018-06-15 11:00',
					EndTime: '2018-06-15 12:00',
					IsNew: true
				}
			],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	it('should save changes with correct data when change activity type for intersect same type activities', function () {
		var date = '2018-06-15';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['11678e5a-ac3f-4daa-9577-a83800e49622', '61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 10:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');
		var vm = panel.isolateScope().vm;
		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [
				{
					ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
					ShiftLayerIds: ['11678e5a-ac3f-4daa-9577-a83800e49622', '61678e5a-ac3f-4daa-9577-a83800e49622']
				}
			],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	it('should save changes with correct data when change activity type for an activity which overlaps another same type activity completely', function () {
		var date = '2018-06-15';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['11678e5a-ac3f-4daa-9577-a83800e49622', '61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 10:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
					TopShiftLayerId: '11678e5a-ac3f-4daa-9577-a83800e49622'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');
		var vm = panel.isolateScope().vm;
		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [
				{
					ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
					ShiftLayerIds: ['11678e5a-ac3f-4daa-9577-a83800e49622']
				}
			],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	it('should save changes with correct data when change activity type for part of an activity that another part of this activity was covered by another activity completely ', function () {
		var date = '2018-06-15';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['layer1'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 10:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['layer2'],
					Color: '#FFa2a2',
					Description: 'E-mail',
					Start: '2018-06-15 10:00',
					End: '2018-06-15 11:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['layer3', 'layer1'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 11:00',
					End: '2018-06-15 12:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
					TopShiftLayerId: 'layer3'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');
		var vm = panel.isolateScope().vm;
		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[2].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [
				{
					ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
					ShiftLayerIds: ['layer3']
				}
			],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	//TODO: should be fixed in story 76450
	xit('should save changes with correct data when changing neighboring activities to same type', function () {
		var date = '2018-06-15';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: ['88878e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Invoice',
					Start: '2018-06-15 09:00',
					End: '2018-06-15 10:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['99978e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 10:00',
					End: '2018-06-15 11:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				},
				{
					ShiftLayerIds: null,
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 10:00',
					End: '2018-06-15 11:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');
		var vm = panel.isolateScope().vm;

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[0].click();

		shiftLayers[1].click();
		typeEls[0].click();
		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeShiftEditorService.lastRequestData).toEqual({
			Date: date,
			PersonId: personId,
			Layers: [
				{
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6',
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622']
				},
				{
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6',
					ShiftLayerIds: ['88878e5a-ac3f-4daa-9577-a83800e49622']
				}

			],
			TrackedCommandInfo: { TrackId: vm.trackId }
		});
	});

	it('should show error message if schedule was changed by others when saving changes', function () {
		var date = '2018-06-28';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['11678e5a-ac3f-4daa-9577-a83800e49622', '61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-28 08:00',
					End: '2018-06-28 10:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
					TopShiftLayerId: '11678e5a-ac3f-4daa-9577-a83800e49622'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});
		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-28', 'Europe/Berlin', scope);
		scope.$apply();

		mockSignalRBackendServer.notifyClients([
			{
				DomainReferenceId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				StartDate: 'D2018-06-28T00:00:00',
				EndDate: 'D2018-06-28T00:00:00',
				TrackId: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx'
			}
		]);

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();
		scope.$apply();

		var saveButton = panel[0].querySelector('.btn-save');
		expect(saveButton.disabled).toBeFalsy();
		saveButton.click();

		var errorEl = panel[0].querySelector('.text-danger');
		expect(!!errorEl).toBeTruthy();
		expect(saveButton.disabled).toBeTruthy();
	});

	it('should enable refresh button when schedule was changed by others', function () {
		var date = '2018-08-06';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['11678e5a-ac3f-4daa-9577-a83800e49622', '61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-08-06 08:00',
					End: '2018-08-06 10:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
					TopShiftLayerId: '11678e5a-ac3f-4daa-9577-a83800e49622'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});
		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-06', 'Europe/Berlin', scope);
		scope.$apply();

		mockSignalRBackendServer.notifyClients([
			{
				DomainReferenceId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				StartDate: 'D2018-08-05T20:00:00',
				EndDate: 'D2018-08-06T10:00:00',
				TrackId: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx'
			}
		]);
		scope.$apply();

		var refreshButton = panel[0].querySelector('.btn-refresh');
		expect(refreshButton.disabled).toBeFalsy();
	});

	it('should disable refresh button when schedule was changed by itself and enable save button after changing back to the previous type', function () {
		var date = '2018-06-28';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['11678e5a-ac3f-4daa-9577-a83800e49622', '61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-28 08:00',
					End: '2018-06-28 10:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
					TopShiftLayerId: '11678e5a-ac3f-4daa-9577-a83800e49622'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});
		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-28', 'Europe/Berlin', scope);
		scope.$apply();

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();
		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[2].click();
		shiftLayers[0].click();
		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		var vm = panel.isolateScope().vm;
		var newSchedule = {
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-06-28 08:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		};
		fakeTeamSchedule.has(newSchedule);
		mockSignalRBackendServer.notifyClients([
			{
				DomainReferenceId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				StartDate: 'D2018-06-28T00:00:00',
				EndDate: 'D2018-06-28T00:00:00',
				TrackId: vm.trackId
			}
		]);
		scope.$apply();

		shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var refreshButton = panel[0].querySelector('.btn-refresh');
		expect(saveButton.disabled).toBeFalsy();
		expect(refreshButton.disabled).toBeTruthy();
	});

	it('should get latest schedule and not show error message when click refresh data button and should be able to save changes after changed something', function () {
		var schedule = {
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-06-28 08:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				},
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#8080c0',
					Description: 'Phone',
					Start: '2018-06-28 10:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		};
		fakeTeamSchedule.has(schedule);

		var scope = $rootScope.$new();
		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-28', 'Europe/Berlin', scope);

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var newSchedule = {
			PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
			Name: 'Annika Andersson',
			Date: '2018-06-28',
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'E-mail',
					Start: '2018-06-28 08:00',
					Minutes: 120,
					IsOvertime: false,
					ActivityId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		};
		fakeTeamSchedule.has(newSchedule);

		var vm = panel.isolateScope().vm;
		mockSignalRBackendServer.notifyClients([
			{
				DomainReferenceId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				StartDate: 'D2018-06-28T00:00:00',
				EndDate: 'D2018-06-28T00:00:00',
				TrackId: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx'
			}
		]);
		scope.$apply();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();
		var errorEl = panel[0].querySelector('.text-danger');
		expect(!!errorEl).toBeTruthy();

		var refreshButton = panel[0].querySelector('.btn-refresh');
		refreshButton.click();
		errorEl = panel[0].querySelector('.text-danger');
		expect(!!errorEl).toBeFalsy();

		saveButton = panel[0].querySelector('.btn-save');
		expect(saveButton.disabled).toBeTruthy();
		expect(refreshButton.disabled).toBeTruthy();

		shiftLayers = panel[0].querySelectorAll('.shift-layer');
		expect(shiftLayers.length).toEqual(1);
		shiftLayers[0].click();

		typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		saveButton = panel[0].querySelector('.btn-save');
		expect(saveButton.disabled).toBeFalsy();
	});

	it('should show succeed notification when saving changes successfully', function () {
		var date = '2018-06-15';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');
		var vm = panel.isolateScope().vm;

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(fakeNoticeService.successMessage).toEqual('SuccessfulMessageForSavingScheduleChanges');
	});

	it('should show error notification when saving changes failed', function () {
		var date = '2018-06-15';
		var personId = 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22';
		fakeTeamSchedule.has({
			PersonId: personId,
			Name: 'Annika Andersson',
			Date: date,
			WorkTimeMinutes: 240,
			ContractTimeMinutes: 240,
			Projection: [
				{
					ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
					Color: '#ffffff',
					Description: 'Phone',
					Start: '2018-06-15 08:00',
					End: '2018-06-15 09:00',
					Minutes: 60,
					IsOvertime: false,
					ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
				}
			],
			Timezone: { IanaId: 'Europe/Berlin' }
		});

		var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-15', 'Europe/Berlin');
		var vm = panel.isolateScope().vm;

		var shiftLayers = panel[0].querySelectorAll('.shift-layer');
		shiftLayers[0].click();

		var typeEls = panel[0].querySelectorAll('.activity-selector md-option');
		typeEls[1].click();

		fakeShiftEditorService.setSavingApplyResponseData([
			{
				PersonId: personId,
				ErrorMessages: ['Error happens']
			}
		]);

		var saveButton = panel[0].querySelector('.btn-save');
		saveButton.click();

		expect(vm.scheduleVm.ShiftLayers[0].Current.ActivityId).toEqual('5c1409de-a0f1-4cd4-b383-9b5e015ab3c6');
		expect(!!fakeNoticeService.successMessage).toEqual(false);
		expect(fakeNoticeService.errorMessage).toEqual('Error happens');
	});

	describe('in locale en-UK', function () {
		beforeEach(function () {
			moment.locale('en-UK');
		});
		afterEach(function () {
			moment.locale('en');
		});

		it('should show time period of activity information correctly when select an activity', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-06-11',
				WorkTimeMinutes: 240,
				ContractTimeMinutes: 240,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-06-11 05:30',
						End: '2018-06-11 07:30',
						Minutes: 120,
						IsOvertime: false
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-11', 'Europe/Berlin');

			var shiftLayers = panel[0].querySelectorAll('.shift-layer');
			shiftLayers[0].click();

			var timespanEl = panel[0].querySelector('.timespan');
			expect(timespanEl.innerText.trim()).toBe('06/11/2018 5:30 AM - 06/11/2018 7:30 AM');
		});

		it('should render date correctly', function () {
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-06-12', 'Europe/Berlin');
			var element = panel[0];

			expect(element.querySelector('.date').innerText.trim()).toEqual('06/12/2018');
		});

		it('should show time span correctly after resizing a selected activity', function () {
			fakeTeamSchedule.has({
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-08-13',
				WorkTimeMinutes: 60,
				ContractTimeMinutes: 60,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'Phone',
						Start: '2018-08-13 08:00',
						End: '2018-08-13 09:00',
						Minutes: 60,
						IsOvertime: false,
						ActivityId: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2'
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			});

			var scope = $rootScope.$new();
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', '2018-08-13', 'Europe/Berlin', scope);
			var vm = panel.isolateScope().vm;

			var shiftLayer = panel[0].querySelector('.shift-layer');
			shiftLayer.click();

			fireResize(vm, shiftLayer, 120, -60);

			var timespanEl = panel[0].querySelector('.timespan');
			expect(timespanEl.innerText.trim()).toEqual('08/13/2018 7:00 AM - 08/13/2018 9:00 AM');
		});
	});

	describe('in locale fa', function () {
		beforeEach(function () {
			moment.locale('fa');
		});
		afterEach(function () {
			moment.locale('en');
		});

		it('should render schedule date correctly', function () {
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

		var element = $compile('<shift-editor date="date" timezone="timezone" person-id="personId"></shift-editor>')(
			scope
		);

		scope.$apply();

		return element;
	}

	function fireResize(vm, el, width, translateX) {
		vm.onResizeMove({
			type: 'resizemove',
			target: el,
			rect: {
				width: width
			},
			deltaRect: { left: translateX }
		});

		vm.onResizeEnd({
			type: 'resizeend',
			target: el
		});
	}

	function FakeActivityService() {
		var activities = [
			{
				Id: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6',
				Name: 'E-mail',
				Color: '#FFa2a2',
				FloatOnTop: false
			},
			{
				Id: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
				Name: 'Invoice',
				Color: '#FF0000',
				FloatOnTop: false
			},
			{
				Id: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
				Name: 'Phone',
				Color: '#ffffff',
				FloatOnTop: false
			},
			{
				Id: '84db44f4-22a8-44c7-b376-a0a200da613e',
				Name: 'Sales',
				Color: '#FFCCA2',
				FloatOnTop: false
			},
			{
				Id: '35e33821-862f-461c-92db-9f0800a8d095',
				Name: 'Social Media',
				Color: '#FFA2CC',
				FloatOnTop: false
			},
			{
				Id: '12e33821-862f-461c-92db-9f0800a8d056',
				Name: 'Lunch',
				Color: '#ffff00',
				FloatOnTop: true
			},
			{
				Id: 'sbs33821-862f-461c-92db-9f0800a8d056',
				Name: 'Short Break',
				Color: '#ff0500',
				FloatOnTop: true
			}
		];
		this.fetchAvailableActivities = function () {
			return {
				then: function (callback) {
					callback(activities);
				}
			};
		};
	}

	function FakeShiftEditorService() {
		this.lastRequestData = {};
		var applyResponse = { data: [] };
		this.setSavingApplyResponseData = function (data) {
			applyResponse.data = data;
		};
		this.changeActivityType = function (date, personId, layers, trackCommandInfo) {
			this.lastRequestData = {
				Date: date,
				PersonId: personId,
				Layers: layers,
				TrackedCommandInfo: trackCommandInfo
			};

			return {
				then: function (callback) {
					callback(applyResponse);
				}
			};
		};
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
		var self = this;
		self.schedules = [];
		this.has = function (schedule) {
			self.schedules = [schedule];
		};
		this.getSchedules = function () {
			return {
				then: function (callback) {
					callback({ Schedules: self.schedules });
				}
			};
		};
	}

	function FakeNoticeService() {
		this.successMessage = '';
		this.errorMessage = '';
		this.warningMessage = '';
		this.success = function (message, time, destroyOnStateChange) {
			this.successMessage = message;
		};
		this.error = function (message, time, destroyOnStateChange) {
			this.errorMessage = message;
		};
		this.warning = function (message, time, destroyOnStateChange) {
			this.warningMessage = message;
		};
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

			$provide.service('$stateParams', function () {
				return stateParams;
			});
		});
	});
	beforeEach(
		inject(function (_$controller_) {
			$controller = _$controller_;
		})
	);

	it('should set up correctly', function () {
		var target = setUp();
		expect(target.personId).toEqual('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22');
		expect(target.timezone).toEqual('Europe/Berlin');
		expect(target.date).toEqual('2018-05-28');
	});

	function setUp() {
		return $controller('ShiftEditorViewController');
	}
});
