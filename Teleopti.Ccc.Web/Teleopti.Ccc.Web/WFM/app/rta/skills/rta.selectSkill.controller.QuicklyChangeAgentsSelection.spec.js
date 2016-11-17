'use strict';
describe('RtaSelectSkillQuickSelectionCtrl', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder,
		$timeout;
	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
		module(function($provide) {
			$provide.service('$stateParams', function() {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function(_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_, _$timeout_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;
		$timeout = _$timeout_;

		scope = $controllerBuilder.setup('RtaSelectSkillQuickSelectionCtrl');

		$fakeBackend.clear();

		spyOn($state, 'go');

	}));

	it('should get organization', function() {
		$fakeBackend.withOrganization(
			{
				Id: 'LondonGuid',
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
			});

		$controllerBuilder.createController();

		expect(scope.sites.length).toEqual(2);
		expect(scope.sites[0].Id).toEqual('LondonGuid');
		expect(scope.sites[0].Name).toEqual('London');
		expect(scope.sites[0].Teams.length).toEqual(2);
		expect(scope.sites[0].Teams).toEqual([{
			Id: '1',
			Name: 'Team Preferences'
		}, {
			Id: '2',
			Name: 'Team Students'
		}]);
	});

	it('should go to agents on site', function() {
		$fakeBackend.withOrganization(
			{
				Id: 'LondonGuid',
				Teams: [{
					Id: 'TeamGuid'
				}]
			});

		$controllerBuilder.createController()
		.apply(function(){
			scope.sites[0].isChecked = true;
			scope.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			siteIds: ['LondonGuid']
		});
	});

	it('should go to agents on sites', function() {
		$fakeBackend.withOrganization(
			{
				Id: 'LondonGuid',
				Teams: [{
					Id: 'TeamLondon'
				}]
			})
			.withOrganization(
				{
					Id: 'ParisGuid',
					Teams: [{
						Id: 'TeamParis'
					}]
				});

		$controllerBuilder.createController()
		.apply(function(){
			scope.sites[0].isChecked = true;
			scope.sites[1].isChecked = true;
			scope.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			siteIds: ['LondonGuid', 'ParisGuid']
		});
	});

	xit('should go to agents on team', function() {
		$fakeBackend.withOrganization(
			{
				Id: 'LondonGuid',
				Teams: [{
					Id: 'LondonTeam1'
				},
				{
					Id: 'LondonTeam2'
				},
			]
			});

		$controllerBuilder.createController()
		.apply(function(){
			scope.selectedTeams = ['LondonTeam1'];
			scope.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			teamIds: ['LondonTeam1']
		});
	});
});
