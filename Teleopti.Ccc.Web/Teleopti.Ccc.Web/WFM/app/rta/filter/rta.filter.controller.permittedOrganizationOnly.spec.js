'use strict';
describe('RtaFilterController', function () {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder,
		$timeout,
		vm;

	var stateParams = {};

	beforeEach(module('wfm.rta'));
	beforeEach(function () {
		module(function ($provide) {
			$provide.factory('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_, _$timeout_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;
		$timeout = _$timeout_;

		scope = $controllerBuilder.setup('RtaFilterController');
		$fakeBackend.clear();
		$fakeBackend.withToggle("RTA_MonitorAgentsInPermittedOrganizationOnly_40660");
		spyOn($state, 'go');
	}));

	it('should get permitted organization', function () {
		$fakeBackend
			.withOrganization({
				Id: 'londonGuid',
				Name: 'London',
				Teams: [{
					Id: '1',
					Name: 'Team Preferences'
				}, {
					Id: '2',
					Name: 'Team Students'
				}]
			})
			.withOrganization({
				Id: 'ParisGuid',
				Name: 'Paris',
				Teams: [{
					Id: '3',
					Name: 'Team Red'
				}, {
					Id: '4',
					Name: 'Team Green'
				}]
			})
			.withPermittedSites(['londonGuid'])
			.withPermittedTeams(["1"]);

		var c = $controllerBuilder.createController();
		vm = c.vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Teams.length).toEqual(1);
		expect(vm.sites[0].Teams[0].Id).toEqual('1');
		expect(vm.sites[0].Teams[0].Name).toEqual('Team Preferences');
	});

	it('should get permitted organization for skill', function () {
		stateParams.skillIds = "phoneGuid";
		$fakeBackend
			.withOrganizationOnSkills({
				Id: 'londonGuid',
				Name: 'London',
				Teams: [{
					Id: '1',
					Name: 'Team Preferences'
				}, {
					Id: '2',
					Name: 'Team Students'
				}]
			}, "phoneGuid")
			.withOrganizationOnSkills({
				Id: 'ParisGuid',
				Name: 'Paris',
				Teams: [{
					Id: '3',
					Name: 'Team Red'
				}, {
					Id: '4',
					Name: 'Team Green'
				}]
			}, "phoneGuid")
			.withSkill({
				Id: "phoneGuid"
			})
			.withPermittedSites(['londonGuid'])
			.withPermittedTeams(["1"]);

		var c = $controllerBuilder.createController();
		vm = c.vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Teams.length).toEqual(1);
		expect(vm.sites[0].Teams[0].Id).toEqual('1');
		expect(vm.sites[0].Teams[0].Name).toEqual('Team Preferences');
	});

	it('should get permitted organization by skill area', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}])
			.withOrganizationOnSkills({
				Id: 'LondonGuid',
				Name: 'London',
				Teams: [{
					Id: '1',
					Name: 'Team Preferences'
				}, {
					Id: '2',
					Name: 'Team Students'
				}]
			}, 'emailGuid, phoneGuid')
			.withOrganizationOnSkills({
				Id: 'ParisGuid',
				Name: 'Paris',
				Teams: [{
					Id: '3',
					Name: 'Team Paris 1'
				}, {
					Id: '4',
					Name: 'Team Paris 2'
				}]
			}, 'emailGuid, phoneGuid')
			.withPermittedSites(['LondonGuid'])
			.withPermittedTeams(['1']);

		var c = $controllerBuilder.createController();
		vm = c.vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Id).toEqual('LondonGuid');
		expect(vm.sites[0].Name).toEqual('London');
		expect(vm.sites[0].Teams.length).toEqual(1);
		expect(vm.sites[0].Teams[0].Id).toEqual('1');
		expect(vm.sites[0].Teams[0].Name).toEqual('Team Preferences');
	});


});
