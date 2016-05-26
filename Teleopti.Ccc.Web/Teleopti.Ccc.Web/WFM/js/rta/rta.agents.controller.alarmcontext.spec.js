'use strict';
describe('RtaAgentsCtrl', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
		module(function ($provide) {
			$provide.service('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		scope = $controllerBuilder.setup('RtaAgentsCtrl');

		$fakeBackend.clear();

	}));

	[
		"2016-05-26T00:30:00",
		"2016-05-26T11:30:00",
		"2016-05-26T15:30:00",
		"2016-05-26T23:30:00"
	].forEach(function(value) {

		var time = moment(value);

		it('should display time line for ' + time.format('HH:mm'), function() {

			stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
			$fakeBackend
				.withTime(time.format('YYYY-MM-DDTHH:mm:ss'))
				.withAgent({
					PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
					TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				});

			$controllerBuilder.createController();

			expect(scope.timeline.length).toEqual(4);
			expect(scope.timeline[0].Time).toEqual(time.startOf('hour').format('HH:mm'));
			expect(scope.timeline[1].Time).toEqual(time.add(1, 'hour').format('HH:mm'));
			expect(scope.timeline[2].Time).toEqual(time.add(1, 'hour').format('HH:mm'));
			expect(scope.timeline[3].Time).toEqual(time.add(1, 'hour').format('HH:mm'));
		});

	});


});
