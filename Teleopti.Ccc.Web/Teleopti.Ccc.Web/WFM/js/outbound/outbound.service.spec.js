﻿'use strict';

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
			StartDate: new Date(),
			EndDate: new Date(),
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
			StartDate: new Date(),
			EndDate: new Date(),
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

	it('misService getDateFromServer should work correctly', function() {
		var input = '2015-10-01Z';
		var result = miscService.getDateFromServer(input);
		expect(result.getDate()).toEqual(1);
		expect(result.getHours()).toEqual(0);
	});

	it('misService get all weekends in period should work correctly', function() {
		var period = {
			PeriodStart: moment('2015-09-01').toDate(),
			PeriodEnd: moment('2015-09-30').toDate()
		};

		var result = miscService.getAllWeekendsInPeriod(period);
		expect(result.length).toEqual(4);

		expect(moment(result[0].WeekendStart).format('YYYY-MM-DD')).toEqual('2015-09-05');
		expect(moment(result[0].WeekendEnd).format('YYYY-MM-DD')).toEqual('2015-09-06');

		expect(moment(result[3].WeekendStart).format('YYYY-MM-DD')).toEqual('2015-09-26');
		expect(moment(result[3].WeekendEnd).format('YYYY-MM-DD')).toEqual('2015-09-27');
	});


	it('misService parse number string should work correctly', function () {				
		expect(miscService.parseNumberString("123,456.7", false)).toEqual(123456.7);
		expect(miscService.parseNumberString("123456.7", false)).toEqual(123456.7);
		expect(miscService.parseNumberString("123,,456.7", false)).toBeFalsy();
		expect(miscService.parseNumberString("123,45,6.7", false)).toBeFalsy();
		expect(miscService.parseNumberString("123456.7", true)).toBeFalsy();
		expect(miscService.parseNumberString("123456.7,8", false)).toBeFalsy();
		expect(miscService.parseNumberString("23,456,789.123", false)).toEqual(23456789.123);
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