'use strict';

describe('RtaOverviewController open/close site', function () {

	beforeEach(module('wfm.rta'));

	var $stateParams;
	var $fakeBackend;

	beforeEach(function () {
		module(function ($provide) {
			$provide.factory('$stateParams', function () {
				$stateParams = {};
				return $stateParams;
			});
			$provide.factory('skills', function () {
				return $fakeBackend.skills;
			});
			$provide.factory('skillAreas', function () {
				return $fakeBackend.skillAreas;
			});
		});
	});

	var
		$controllerBuilder,
		$scope,
		$state,
		$sessionStorage;
	beforeEach(inject(function (_FakeRtaBackend_, _ControllerBuilder_, _$state_, _$sessionStorage_) {
		$controllerBuilder = _ControllerBuilder_;
		$fakeBackend = _FakeRtaBackend_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		spyOn($state, 'go');
		$state.current.name = 'rta-refact';
		$scope = $controllerBuilder.setup('RtaOverviewController39082');
	}));

	afterEach(function () {
		$fakeBackend.clear();
		$sessionStorage.$reset();
	});

	it('should store selected site', function () {
		$stateParams.siteIds = ['parisId'];
		$fakeBackend
			.withSiteAdherence({
				Id: 'parisId'
			});
		$controllerBuilder.createController();
		expect($sessionStorage.rtaState.siteIds).toEqual(['parisId']);
	});

	it('should store selected site and skill being selected', function () {
		$stateParams.siteIds = ['parisId'];
		$fakeBackend
			.withSiteAdherence({
				Id: 'parisId'
			})
			.withSkill({
				Id: 'skillId'
			});
		var c = $controllerBuilder.createController();

		c.apply(function () {
			c.vm.selectSkillOrSkillArea({ Id: 'skillId' })
		});

		expect($sessionStorage.rtaState.siteIds).toEqual(['parisId']);
		expect($sessionStorage.rtaState.skillIds).toEqual('skillId');
	});

});
