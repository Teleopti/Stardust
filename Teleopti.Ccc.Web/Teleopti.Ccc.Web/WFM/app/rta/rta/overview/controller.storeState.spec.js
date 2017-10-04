'use strict';

describe('RtaOverviewController open/close site', function () {

	beforeEach(module('wfm.rta'));
	beforeEach(module('wfm.rtaTestShared'));

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
		$sessionStorage;
	beforeEach(inject(function (_FakeRtaBackend_, _ControllerBuilder_, _$state_, _$sessionStorage_) {
		$controllerBuilder = _ControllerBuilder_;
		$fakeBackend = _FakeRtaBackend_;
		$sessionStorage = _$sessionStorage_;
		spyOn(_$state_, 'go');
		$controllerBuilder.setup('RtaOverviewController39082');
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
