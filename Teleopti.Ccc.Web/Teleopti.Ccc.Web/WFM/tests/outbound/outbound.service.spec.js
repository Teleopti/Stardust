'use strict';

describe('OutboundService Test', function () {

	var $q,
		$rootScope,
		$httpBackend,
		outboundService,
		miscService
	;

	beforeEach(function () {
		module('outboundServiceModule');
		module(function($provide) {
			$provide.service('outboundActivityService', function () {
				return new FakeOutboundActivityService();
			});
		});
	});


	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _outboundService_, _miscService_) {

		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		outboundService = _outboundService_;
		miscService = _miscService_;
		$httpBackend.resetExpectations();	
	}));

	it('Get campaign should work', function() {
		$httpBackend.whenGET('../api/Outbound/Campaign/1').respond(200, JSON.stringify(presetCampaigns()[0]));

		var campaign;

		outboundService.getCampaign('1', function(data) {
			campaign = data;
		});

		$httpBackend.flush();

		expect(campaign).toBeDefined();
		expect(campaign.Name).toEqual('C1');
	});

	it('Create empty working hour should work', function() {

		var startTime = new Date().setHours(8, 0, 0, 0);
		var endTime = new Date().setHours(12, 0, 0, 0);

		var workingHour = outboundService.createEmptyWorkingPeriod(startTime, endTime);

		expect(workingHour.StartTime).toEqual(startTime);
		expect(workingHour.EndTime).toEqual(endTime);
		expect(workingHour.WeekDaySelections.length).toEqual(7);

		angular.forEach(workingHour.WeekDaySelections, function(s) {
			expect(s.Checked).not.toBeTruthy();
		});
	});

	it('Calculate campaign person hour should work', function() {

		var campaign = {
			CallListLen : 2400,
			TargetRate : 100,
			RightPartyConnectRate : 100,
			RightPartyAverageHandlingTime : 600,
			ConnectRate: 10,
			UnproductiveTime: 0,
			ConnectAverageHandlingTime: 0
		};

		var personHour = outboundService.calculateCampaignPersonHour(campaign);
		expect(personHour).toEqual(400);

	});

	it('Create campaign should work', function() {
		var campaign = {
			Name: 'NEW',
			WorkingHours: []
		};

		$httpBackend.when('POST', '../api/Outbound/Campaign',
			function(postData) {
				var jsonData = JSON.parse(postData);				
				expect(jsonData.Name).toBe(campaign.Name);
				expect(jsonData.Id).not.toBeDefined();
				return true;
			}
		).respond(200, true);
		
		outboundService.addCampaign(campaign);
		$httpBackend.flush();
	});


	it('Normalization of campaign working hours should work', function() {

		var startTime = new Date().setHours(8, 0, 0, 0);
		var endTime = new Date().setHours(14, 0, 0, 0);

		var campaign = {
			Name: 'NEW',
			WorkingHours: [
				{
					StartTime: startTime,
					EndTime: endTime,
					WeekDaySelections: [
						{ WeekDay: 0, Checked: false },
						{ WeekDay: 1, Checked: false },
						{ WeekDay: 2, Checked: false },
						{ WeekDay: 3, Checked: true },
						{ WeekDay: 4, Checked: false },
						{ WeekDay: 5, Checked: false },
						{ WeekDay: 6, Checked: true },
					]
				}
			]
		};

		$httpBackend.when('POST', '../api/Outbound/Campaign',
			function (postData) {
				var jsonData = JSON.parse(postData);				
				expect(jsonData.WorkingHours.length).toEqual(2);
				expect(jsonData.WorkingHours[0].StartTime).toEqual('08:00');
				expect(jsonData.WorkingHours[0].EndTime).toEqual('14:00');
				expect(jsonData.WorkingHours[0].WeekDay).toEqual(3);
				expect(jsonData.WorkingHours[1].WeekDay).toEqual(6);
				return true;
			}
		).respond(200, true);

		outboundService.addCampaign(campaign);

		$httpBackend.flush();
	});

	it('Denormalization of campaign working hours should work', function () {
		$httpBackend.whenGET('../api/Outbound/Campaign/1').respond(200, JSON.stringify(presetCampaigns()[0]));
		$httpBackend.whenGET('../api/Outbound/Campaign/2').respond(200, JSON.stringify(presetCampaigns()[1]));

		var campaign1;
		var campaign2;

		outboundService.getCampaign('1', function (data) {
			campaign1 = data;
		});
		
		outboundService.getCampaign('2', function (data) {
			campaign2 = data;
		});

		$httpBackend.flush();

		expect(campaign1.WorkingHours.length).toEqual(1);
		expect(campaign2.WorkingHours.length).toEqual(2);

		angular.forEach(campaign1.WorkingHours[0].WeekDaySelections, function(s) {
			if (s.WeekDay == 1 || s.WeekDay == 3) {
				expect(s.Checked).toBeTruthy();
			} else {
				expect(s.Checked).not.toBeTruthy();
			}
		});

		angular.forEach(campaign2.WorkingHours[0].WeekDaySelections, function (s) {
			if (s.WeekDay == 1 ) {
				expect(s.Checked).toBeTruthy();
			} else {
				expect(s.Checked).not.toBeTruthy();
			}
		});
	});

	it('should have expected phase stastistics', function () {
		$httpBackend.when('POST', '../api/Outbound/Campaign/Statistics',
			function() {
				return true;
			}).respond(200, JSON.stringify({ "Planned": 1, "PlannedWarning": 2, "Scheduled": 3, "ScheduledWarning": 4, "OnGoing": 5, "OnGoingWarning": 6, "Done": 7 }));

		var result;
		outboundService.getCampaignStatistics(null, function success(data) {
			result = data;
		});

		$httpBackend.flush();

		expect(result.Planned).toEqual(1);
		expect(result.PlannedWarning).toEqual(2);
		expect(result.Scheduled).toEqual(3);
		expect(result.ScheduledWarning).toEqual(4);
		expect(result.OnGoing).toEqual(5);
		expect(result.OnGoingWarning).toEqual(6);
		expect(result.Done).toEqual(7);
	});

	it('misService getDateFromServer should work correctly', function() {
		var input = '2015-10-01Z';
		var result = miscService.getDateFromServer(input);
		expect(result.getDate()).toEqual(1);
		expect(result.getHours()).toEqual(0);
	});


	function presetCampaigns() {
		return [
			{
				Id: '1',
				Name: 'C1',
				WorkingHours: [
					{ WeekDay: 1, StartTime: new Date().setHours(8, 0, 0, 0), EndTime: new Date().setHours(12, 0, 0, 0) },
					{ WeekDay: 3, StartTime: new Date().setHours(8, 0, 0, 0), EndTime: new Date().setHours(12, 0, 0, 0) }
				]
			},
			{
				Id: '2',
				Name: 'C2',
				WorkingHours: [
					{ WeekDay: 1, StartTime: new Date().setHours(8, 0, 0, 0), EndTime: new Date().setHours(12, 0, 0, 0) },
					{ WeekDay: 4, StartTime: new Date().setHours(8, 0, 0, 0), EndTime: new Date().setHours(14, 0, 0, 0) }
				]
			}
		];

	}

	 
	function FakeOutboundActivityService() {

		this.listActivity = function() {
			return [
				{ Name: 'A1', Id: '1' },
				{ Name: 'A2', Id: '2' }
			];
		};

		this.refresh = function() {};


	}

	

});