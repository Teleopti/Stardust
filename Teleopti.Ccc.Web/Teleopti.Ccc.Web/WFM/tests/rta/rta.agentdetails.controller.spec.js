'use strict';
describe('RtaAgentDetailsCtrl', function() {
	var $q,
		$rootScope,
		$interval,
		$httpBackend,
		$controller,
		$resource,
		$state,
		$sessionStorage,
		scope;

	var stateParams = {};
	var rtaSvrc = {};
	var personDetails = {};
	var activityAdherence = [];

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
		personDetails = {
			Name: "Ashley Andeen"
		};

		activityAdherence = [{
			Name: "Phone",
			StartTime: "2014-10-06T08:00:00",
			ActualStartTime: "2014-10-06T08:00:00",
			TimeInAdherence: "00:30:00",
			TimeOutOfAdherence: "01:30:00"
		}];
	});

	beforeEach(function() {
		module(function($provide) {
			$provide.service('RtaService', function() {
				return rtaSvrc;
			});
			$provide.service('$stateParams', function() {
				return stateParams;
			});
		});
	});

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_, _$interval_, _$controller_, _$resource_, _$state_, _$sessionStorage_) {
		$controller = _$controller_;
		scope = _$rootScope_.$new();
		$q = _$q_;
		$interval = _$interval_;
		$rootScope = _$rootScope_;
		$resource = _$resource_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;

		rtaSvrc.getPersonDetails = $resource('../Agents/PersonDetails?personId=:personId', {
			personId: '@personId'
		}, {
			query: {
				method: 'GET',
				params: {},
				isArray: false
			}
		});

		rtaSvrc.getAdherenceDetails = $resource('../Adherence/ForDetails?personId=:personId', {
			personId: '@personId'
		}, {
			query: {
				method: 'GET',
				params: {},
				isArray: true
			}
		});

		$httpBackend.whenGET("../Agents/PersonDetails?personId=11610fe4-0130-4568-97de-9b5e015b2564")
			.respond(200, personDetails);
		$httpBackend.whenGET("../Adherence/ForDetails?personId=11610fe4-0130-4568-97de-9b5e015b2564")
			.respond(200, activityAdherence);
	}));

	var createController = function() {
		$controller('RtaAgentDetailsCtrl', {
			$scope: scope
		});
		scope.$digest();
		$httpBackend.flush();
	}

	it('should get name for agent', function() {
		stateParams.personId = "11610fe4-0130-4568-97de-9b5e015b2564";
		personDetails = {
			Name: "Ashley Andeen"
		};

		createController();

		expect(scope.Name).toEqual("Ashley Andeen");
	});

	fit('should get details for agent', function() {
		stateParams.personId = "11610fe4-0130-4568-97de-9b5e015b2564";
		activityAdherence = [{
			Name: "Phone",
			StartTime: "2014-10-06T08:00:00",
			ActualStartTime: "2014-10-06T08:00:00",
			TimeInAdherence: "00:30:00",
			TimeOutOfAdherence: "01:30:00"
		}]

		createController();

		expect(scope.adherence[0].Name).toEqual("Phone");
		expect(scope.adherence[0].StartTime).toEqual("2014-10-06T08:00:00");
		expect(scope.adherence[0].ActualStartTime).toEqual("2014-10-06T08:00:00");
		expect(scope.adherence[0].TimeInAdherence).toEqual("00:30:00");
		expect(scope.adherence[0].TimeOutOfAdherence).toEqual("01:30:00");
	});


});
