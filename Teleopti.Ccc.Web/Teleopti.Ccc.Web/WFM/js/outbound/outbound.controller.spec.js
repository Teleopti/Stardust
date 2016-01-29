'use strict';
describe('OutboundCreateCtrl', function() {
	var $q,
		$rootScope,
		$controller,
		outboundService,
		stateService,
		outboundNotificationService
	;

	beforeEach(function () {
		module('wfm.outbound');
		
		outboundService = new fakeOutboundService();
		stateService = new fakeStateService();

		module(function($provide) {
			$provide.service('outboundService', function () {
				return outboundService;
			});

			$provide.service('$state', function () {
				return stateService;
			});
		});
	});

	beforeEach(inject(function (_$q_, _$rootScope_, _$controller_, _outboundNotificationService_) {

		$q = _$q_;
		$rootScope = _$rootScope_;
		$controller = _$controller_;
		outboundNotificationService = _outboundNotificationService_;
	}));


	it('Name should be empty to start with', function() {
		var test = setUpTarget();
		expect(test.scope.campaign.Name).not.toBeDefined();

	});

	it('Working hours should be empty to start with', function() {
		var test = setUpTarget();
		expect(test.scope.campaign.WorkingHours).toBeDefined();
		expect(test.scope.campaign.WorkingHours.length).toEqual(0);
	});

	it('Activity should be empty to start with', function() {
		var test = setUpTarget();
		expect(test.scope.campaign.Activity).toBeDefined();
		expect(test.scope.campaign.Activity.Id).not.toBeDefined();
	});

	it('Start Day and End Day should be set to start with', function() {
		var test = setUpTarget();
		expect(test.scope.campaign.StartDate).toBeDefined();
		expect(test.scope.campaign.EndDate).toBeDefined();
	});

	it('Default to invalid', function() {
		var test = setUpTarget();
		expect(test.scope.isFormValidForPage).not.toBeDefined();
	});

	it('Should watch for campaign person hour', function() {
		var test = setUpTarget();

		test.scope.form = { $error: {} };
		test.scope.campaign.calculatedPersonHour = 1;

		test.scope.$digest();
		expect(test.scope.estimatedWorkload).toEqual(1);
	});

	it('Should reset after valid submission', function() {
		var test = setUpTarget();
		test.scope.form = { $valid: true };
		completeValidCampaign(test.scope.campaign);
		test.scope.addCampaign();
		expect(test.scope.campaign.WorkingHours.length).toEqual(0);
	});

	it('Invalid date range should fail', function () {

		var test = setUpTarget();
		test.scope.form = { $valid: true, $error: { required: [] } };
		completeValidCampaign(test.scope.campaign);
		test.scope.campaign.spanningPeriodErrors = [];
		test.scope.$digest();
		expect(test.scope.isCampaignDurationValid()).toBeTruthy();
		test.scope.campaign.spanningPeriodErrors = ['something wrong'];
		test.scope.$digest();
		expect(test.scope.isCampaignDurationValid()).not.toBeTruthy();
	});

	it('At least one working hour has been selected for any weekday for the campaign to be valid', function() {		
		var test = setUpTarget();
		test.scope.form = { $valid: true, $error: { required: [] } };
		completeValidCampaign(test.scope.campaign);
		angular.forEach(test.scope.campaign.WorkingHours[0].WeekDaySelections, function (e) {
			e.Checked = true;
		});
		test.scope.$digest();
		expect(test.scope.isWorkingHoursValid()).toBeTruthy();

		angular.forEach(test.scope.campaign.WorkingHours[0].WeekDaySelections, function (e) {
			e.Checked = false;
		});
		test.scope.$digest();
		expect(test.scope.isWorkingHoursValid()).not.toBeTruthy();
	});

	it('Should move to campaign edit upon successful submission', function() {
		var test = setUpTarget();
		test.scope.form = { $valid: true, $error: { required: [] } };
		completeValidCampaign(test.scope.campaign);
		test.scope.addCampaign();
		expect(stateService.getWhere()).toEqual('outbound.edit');
	});


	function setUpTarget() {
		var scope = $rootScope.$new();
		var target = $controller('OutboundCreateCtrl', {
			$scope: scope,
			$state: stateService,
			outboundService: outboundService,
			outboundNotificationService: outboundNotificationService
		});
		return { target: target, scope: scope };
	}


	function fakeStateService() {

		var where;

		this.reset = function () { where = null; };
		this.getWhere = function () { return where; };

		this.go = function (gowhere) {
			where = gowhere;
		}
	}


	function fakeOutboundService() {

		var campaigns = [];

		this.getCampaign = function (campaignId, successCb, errorCb) {
			return campaigns;
		};

		this.checkPermission = function () {
			return {
				then: function(cb) { cb(); }
			};
		}

		this.addCampaign = function (campaign, successCb, errorCb) {
			campaigns.push(campaign);
			successCb(campaign);
		};

		this.calculateCampaignPersonHour = function (campaign) {
			return campaign.calculatedPersonHour;
		}
	}


	function completeValidCampaign(campaign) {
		var weekDaySelections = [];
		for (var i = 0; i < 7; i++) {
			weekDaySelections.push({
				WeekDay: i,
				Checked: true
			});
		}

		var workingHour = {
			StartTime: new Date().setHours(12, 0, 0, 0),
			EndTime: new Date().setHours(13, 0, 0, 0),
			WeekDaySelections: weekDaySelections
		}
		campaign.WorkingHours = [workingHour];
	}
});
