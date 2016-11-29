'use strict';
fdescribe('RtaAgentsCtrl', function() {
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

		scope = $controllerBuilder.setup('RtaAgentsCtrl');

		$fakeBackend.clear();
		spyOn($state, 'go');

		$fakeBackend.withToggle('RTA_QuicklyChangeAgentsSelection_40610');
	}));

	it('should get organization', function() {
		$fakeBackend.withOrganization({
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

	it('should select all teams when selecting site', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			},
			{
				Id: 'LondonTeam2'
			}]
		});

		$controllerBuilder.createController()
			.apply(function() {
				scope.sites[0].isChecked = true;
				scope.updateAllTeams(scope.sites[0].Id);
			});

		expect(scope.sites[0].Teams[0].isChecked).toBe(true);
		expect(scope.sites[0].Teams[1].isChecked).toBe(true);
	});

	it('should unselect all teams when unselecting site', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			},
			{
				Id: 'LondonTeam2'
			}]
		});

		$controllerBuilder.createController()
			.apply(function() {
				scope.sites[0].isChecked = true;
				scope.updateAllTeams(scope.sites[0].Id);
				scope.sites[0].isChecked = false;
				scope.updateAllTeams(scope.sites[0].Id);
			});

		expect(scope.sites[0].Teams[0].isChecked).toBe(false);
		expect(scope.sites[0].Teams[1].isChecked).toBe(false);
	});


	it('should go to agents on site', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'TeamGuid'
			}]
		});

		$controllerBuilder.createController()
			.apply(function() {
				scope.sites[0].isChecked = true;
				scope.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['LondonGuid']
		});
	});

	it('should go to agents on sites', function() {
		$fakeBackend.withOrganization({
				Id: 'LondonGuid',
				Teams: [{
					Id: 'TeamLondon'
				}]
			})
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'TeamParis'
				}]
			});

		$controllerBuilder.createController()
			.apply(function() {
				scope.sites[0].isChecked = true;
				scope.sites[1].isChecked = true;
				scope.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['LondonGuid', 'ParisGuid']
		});
	});

	it('should go to agents on team', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}, ]
		});

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedTeams = ['LondonTeam1'];
				scope.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			teamIds: ['LondonTeam1']
		});
	});

	it('should go to agents on teams', function() {
		$fakeBackend.withOrganization({
				Id: 'LondonGuid',
				Teams: [{
					Id: 'LondonTeam1'
				}, {
					Id: 'LondonTeam2'
				}]
			})
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});;

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedTeams = ['LondonTeam1', 'ParisTeam1'];
				scope.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			teamIds: ['LondonTeam1', 'ParisTeam1']
		});
	});

	it('should go to agents on site and team', function() {
		$fakeBackend.withOrganization({
				Id: 'LondonGuid',
				Teams: [{
					Id: 'LondonTeam1'
				}, {
					Id: 'LondonTeam2'
				}]
			})
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});;

		$controllerBuilder.createController()
			.apply(function() {
				scope.sites[0].isChecked = true;
				scope.selectedTeams = ['ParisTeam1'];
				scope.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['LondonGuid'],
			teamIds: ['ParisTeam1']
		});
	});

	it('should unselect team', function() {
		$fakeBackend.withOrganization({
				Id: 'LondonGuid',
				Teams: [{
					Id: 'LondonTeam1'
				}, {
					Id: 'LondonTeam2'
				}]
			})
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});;

		$controllerBuilder.createController()
			.apply(function() {
				scope.selectedTeams = ['LondonTeam1'];
				scope.selectedTeams = ['LondonTeam1', 'ParisTeam1'];
				scope.selectedTeams = ['ParisTeam1'];
				scope.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			teamIds: ['ParisTeam1']
		});
	});

	it('should unselect site', function() {
		$fakeBackend.withOrganization({
				Id: 'LondonGuid',
				Teams: [{
					Id: 'LondonTeam1'
				}]
			})
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}]
			});;

		$controllerBuilder.createController()
			.apply(function() {
				scope.sites[0].isChecked = true;
				scope.sites[1].isChecked = true;
				scope.sites[0].isChecked = false;
				scope.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['ParisGuid']
		});
	});

	it('should unselect site when preselected', function() {
		stateParams.siteIds = ['ParisGuid'];
		$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				},
				{
					Id: 'ParisTeam2',
				}
			]
			});;

		$controllerBuilder.createController()
			.apply(function() {
				scope.sites[0].isChecked = false;
			});

		expect(scope.sites[0].isChecked).toBe(false);
	});


	it('should not redirect when nothing is selected', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}]
		});

		$controllerBuilder.createController()
			.apply(function() {
				scope.sites[0].isChecked = true;
				scope.sites[0].isChecked = false;
				scope.goToAgents();
			});

		expect($state.go).not.toHaveBeenCalledWith('rta.select-skill', {});
	});

	it('should select site and teams when preexisting selection for site', function() {
		stateParams.siteIds = ['ParisGuid'];
		$fakeBackend.withOrganization({
			Id: 'ParisGuid',
			Teams: [{
				Id: 'ParisTeam1',
			},
			{
				Id: 'ParisTeam2',
			}]
		});

		$controllerBuilder.createController();
		expect(scope.selectedSites()).toEqual(['ParisGuid']);
		expect(scope.sites.length).toEqual(1);
		expect(scope.sites[0].Id).toEqual('ParisGuid');
		expect(scope.sites[0].isChecked).toEqual(true);
		expect(scope.sites[0].Teams.length).toEqual(2);
		expect(scope.sites[0].Teams[0].isChecked).toEqual(true);
		expect(scope.sites[0].Teams[1].isChecked).toEqual(true);
	});

	it('should select sites and teams when one site was preselected', function() {
		stateParams.siteIds = ['ParisGuid'];
		$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				},
				{
					Id: 'ParisTeam2',
				}]
			});

		$controllerBuilder.createController()
		.apply(function() {
			scope.teamsSelected = ['ParisTeam2'];
		});
		expect(scope.sites.length).toEqual(1);
		expect(scope.sites[0].Id).toEqual('ParisGuid');
		expect(scope.sites[0].isChecked).toEqual(true);
		expect(scope.sites[0].Teams.length).toEqual(2);
		expect(scope.sites[0].Teams[0].isChecked).toEqual(false);
		expect(scope.sites[0].Teams[1].isChecked).toEqual(true);
	});

	it('should go to agents when selecting new site', function() {
		stateParams.siteIds = ['ParisGuid'];
		$fakeBackend.withOrganization({
				Id: 'LondonGuid',
				Teams: [{
					Id: 'LondonTeam1'
				}]
			})
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}]
			});

		$controllerBuilder.createController()
		.apply(function() {
			scope.sites[0].isChecked = true;
			scope.goToAgents();
		});
		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['LondonGuid','ParisGuid']
		});
	});

	xit('should go to agents when selecting new site and deselcting part of teams in the old one', function() {
		stateParams.siteIds = ['ParisGuid'];
		$fakeBackend.withOrganization({
				Id: 'LondonGuid',
				Teams: [{
					Id: 'LondonTeam1'
				}]
			})
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				},
				{
					Id: 'ParisTeam2',
				}]
			});

		$controllerBuilder.createController()
		.apply(function() {
			scope.sites[0].isChecked = true;
			scope.teamsSelected = ['ParisTeam2'];
			scope.goToAgents();
		});
		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['LondonGuid'],
			teamIds: ['ParisTeam2']
		});
	});

	it('should select site when all teams are selected', function() {
			$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				},
				{
					Id: 'ParisTeam2',
				}]
			});

		$controllerBuilder.createController()
		.apply(function() {
			scope.teamsSelected = ['ParisTeam1', 'ParisTeam2'];
		});
		expect(scope.sites[0].isChecked).toEqual(true);
	});

	it('should select only team', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}, ]
		});

		$controllerBuilder.createController()
			.apply(function() {
				scope.teamsSelected = ['LondonTeam1'];
			});

		expect(scope.sites[0].Teams[0].isChecked).toBe(true);
		expect(scope.sites[0].Teams[1].isChecked).not.toBe(true);
		expect(scope.sites[0].isChecked).not.toBe(true);
	});

	it('should unselect site when all teams are unselected', function() {
			$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				},
				{
					Id: 'ParisTeam2',
				}]
			});

		$controllerBuilder.createController()
		.apply(function() {
			scope.teamsSelected = ['ParisTeam1', 'ParisTeam2'];
			scope.teamsSelected = [];
		});
		expect(scope.sites[0].isChecked).toEqual(false);
	});

	xit('should go to agents on site when all teams are selected', function() {
			$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				},
				{
					Id: 'ParisTeam2',
				}]
			});

		$controllerBuilder.createController()
		.apply(function() {
			scope.teamsSelected = ['ParisTeam1', 'ParisTeam2'];
			scope.goToAgents();
		});
		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['ParisGuid']
		});
	});
});
