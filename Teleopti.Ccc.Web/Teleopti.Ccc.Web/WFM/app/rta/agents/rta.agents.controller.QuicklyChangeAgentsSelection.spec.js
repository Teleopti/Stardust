'use strict';
describe('RtaAgentsController', function() {
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

		scope = $controllerBuilder.setup('RtaAgentsController');

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

		vm = $controllerBuilder.createController().vm;
		expect(vm.sites.length).toEqual(2);
		expect(vm.sites[0].Id).toEqual('LondonGuid');
		expect(vm.sites[0].Name).toEqual('London');
		expect(vm.sites[0].Teams.length).toEqual(2);
		expect(vm.sites[0].Teams).toEqual([{
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
			}, {
				Id: 'LondonTeam2'
			}]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.forTest_selectSite(vm.sites[0]);
		});

		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[1].isChecked).toBe(true);
	});

	it('should unselect all teams when unselecting site', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.forTest_selectSite(vm.sites[0]);
			vm.forTest_selectSite(vm.sites[0]);
		});

		expect(vm.teamChecked(vm.sites[0], vm.sites[0].Teams[0])).toBe(false);
		expect(vm.teamChecked(vm.sites[0], vm.sites[0].Teams[1])).toBe(false);
	});

	it('should unselect site when unselecting all teams and site was in stateParams', function() {
		stateParams.siteIds = ['LondonGuid'];
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				vm.teamsSelected = ['LondonTeam2'];
			})
			.apply(function() {
				vm.teamsSelected = [];
			});

		expect(vm.sites[0].isChecked).toBe(false);
	});

	it('should select team when selecting site', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'TeamGuid'
			}]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.forTest_selectSite(vm.sites[0]);
		});

		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
	});

	it('should only unselect one team when a site was selected', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}, ]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				vm.forTest_selectSite(vm.sites[0]);
			})
			.apply(function() {
				vm.teamsSelected = ['LondonTeam1'];
			});

		expect(vm.teamChecked(vm.sites[0], vm.sites[0].Teams[0])).toBe(true);
		expect(vm.teamChecked(vm.sites[0], vm.sites[0].Teams[1])).toBe(false);
	});

	it('should go to agents on site', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'TeamGuid'
			}]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.forTest_selectSite(vm.sites[0]);
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['LondonGuid'],
			teamIds: []
		}, {
			reload: true,
			notify: true
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

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.forTest_selectSite(vm.sites[0]);
			vm.forTest_selectSite(vm.sites[1]);
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['LondonGuid', 'ParisGuid'],
			teamIds: []
		}, {
			reload: true,
			notify: true
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

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.teamsSelected = ['LondonTeam1'];
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: [],
			teamIds: ['LondonTeam1']
		}, {
			reload: true,
			notify: true
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

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.teamsSelected = ['LondonTeam1', 'ParisTeam1'];
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: [],
			teamIds: ['LondonTeam1', 'ParisTeam1']
		}, {
			reload: true,
			notify: true
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

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.forTest_selectSite(vm.sites[0]);
			vm.teamsSelected = ['ParisTeam1'];
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['LondonGuid'],
			teamIds: ['ParisTeam1']
		}, {
			reload: true,
			notify: true
		});
	});

	it('should go to agents when unselecting team', function() {
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

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.teamsSelected = ['LondonTeam1', 'ParisTeam1'];
			vm.teamsSelected = ['ParisTeam1'];
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: [],
			teamIds: ['ParisTeam1']
		}, {
			reload: true,
			notify: true
		});
	});

	it('should go to agents when unselecting site', function() {
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

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				vm.forTest_selectSite(vm.sites[0]);
				vm.forTest_selectSite(vm.sites[1]);
				vm.forTest_selectSite(vm.sites[0]);
			})
			.apply(function() {
				vm.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['ParisGuid'],
			teamIds: []
		}, {
			reload: true,
			notify: true
		});
	});

	it('should unselect site when preselected', function() {
		stateParams.siteIds = ['ParisGuid'];
		$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});;

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.forTest_selectSite(vm.sites[0]);
		});

		expect(vm.sites[0].isChecked).toBe(false);
	});

	it('should select team', function() {
		$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});;

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.teamsSelected = ['ParisTeam1'];
		});

		expect(vm.sites[0].isChecked).not.toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[1].isChecked).toBe(false);
	});


	it('should not redirect when selection has not changed', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				vm.forTest_selectSite(vm.sites[0]);
			})
			.apply(function() {
				vm.forTest_selectSite(vm.sites[0]);
			})
			.apply(function() {
				vm.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: [],
			teamIds: []
		}, {
			reload: true,
			notify: true
		});
	});

	it('should select site and teams when preexisting selection for site', function() {
		stateParams.siteIds = ['ParisGuid'];
		$fakeBackend.withOrganization({
			Id: 'ParisGuid',
			Teams: [{
				Id: 'ParisTeam1',
			}, {
				Id: 'ParisTeam2',
			}]
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites[0].isChecked).toEqual(true);
		expect(vm.teamChecked(vm.sites[0], vm.sites[0].Teams[0])).toEqual(true);
		expect(vm.teamChecked(vm.sites[0], vm.sites[0].Teams[1])).toEqual(true);
	});

	it('should unselect site when preselected and team is unselected', function() {
		stateParams.siteIds = ['ParisGuid'];
		$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.teamsSelected = ['ParisTeam2'];
		});

		expect(vm.sites[0].isChecked).not.toEqual(true);
		expect(vm.sites[0].Teams[0].isChecked).toEqual(false);
		expect(vm.sites[0].Teams[1].isChecked).toEqual(true);
	});

	it('should unselect site and team', function() {
		$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				vm.forTest_selectSite(vm.sites[0]);
			})
			.apply(function() {
				vm.teamsSelected = ['ParisTeam2'];
			});

		expect(vm.sites[0].isChecked).toEqual(false);
		expect(vm.sites[0].Teams[0].isChecked).toEqual(false);
		expect(vm.sites[0].Teams[1].isChecked).toEqual(true);
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

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				vm.forTest_selectSite(vm.sites[0]);
			})
			.apply(function() {
				vm.goToAgents();
			});
		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['LondonGuid', 'ParisGuid'],
			teamIds: []
		}, {
			reload: true,
			notify: true
		});
	});

	it('should select site and teams when in site in stateParams', function() {
		stateParams.siteIds = ['LondonGuid'];
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}]
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
	});

	it('should select team when team in stateParams', function() {
		stateParams.teamIds = ['LondonTeam1'];
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}]
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites[0].isChecked).not.toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
	});

	it('should select teams when teams in stateParams', function() {
		stateParams.teamIds = ['LondonTeam1', 'LondonTeam2'];
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}, {
				Id: 'LondonTeam3'
			}]
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites[0].isChecked).not.toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[1].isChecked).toBe(true);

	});

	it('should select site and team when in stateParams', function() {
		stateParams.siteIds = ['LondonGuid'];
		stateParams.teamIds = ['ParisTeam1']
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
				}, {
					Id: 'ParisTeam2',
				}]
			});;

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites[0].isChecked).toBe(true);
		expect(vm.teamChecked(vm.sites[0], vm.sites[0].Teams[0])).toBe(true);
		expect(vm.sites[1].isChecked).not.toBe(true);
		expect(vm.sites[1].Teams[0].isChecked).toBe(true);
	});

	it('should go to agents when selecting new site and deselecting some teams in the old one', function() {
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
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				vm.teamsSelected = ['LondonTeam1', 'ParisTeam2'];
			})
			.apply(function() {
				vm.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['LondonGuid'],
			teamIds: ['ParisTeam2']
		}, {
			reload: true,
			notify: true
		});
	});

	it('should select site when all teams are selected', function() {
		$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.teamsSelected = ['ParisTeam1', 'ParisTeam2'];
		});
		expect(vm.sites[0].isChecked).toEqual(true);
	});

	it('should not select site when some teams are selected', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}, ]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.teamsSelected = ['LondonTeam1'];
		});

		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[1].isChecked).not.toBe(true);
		expect(vm.sites[0].isChecked).not.toBe(true);
	});

	it('should unselect site when all teams are unselected', function() {
		$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				vm.forTest_selectSite(vm.sites[0]);
			})
			.apply(function() {
				vm.teamsSelected = ['ParisTeam2'];
			})
			.apply(function() {
				vm.teamsSelected = [];
			});

		expect(vm.sites[0].isChecked).toBe(false);
	});

	it('should go to agents on site when all teams are selected', function() {
		$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				vm.teamsSelected = ['ParisTeam1', 'ParisTeam2'];
			})
			.apply(function() {
				vm.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: ['ParisGuid'],
			teamIds: []
		}, {
			reload: true,
			notify: true
		});
	});

	it('should go to agents on team when site was selected', function() {
		stateParams.siteIds = ['ParisGuid'];
		$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				vm.teamsSelected = ['ParisTeam1'];
			})
			.apply(function() {
				vm.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: [],
			teamIds: ['ParisTeam1']
		}, {
			reload: true,
			notify: true
		});
	});

	it('should redirect to initial state after unselecting all', function() {
		stateParams.siteIds = ['LondonGuid'];
		stateParams.teamIds = ['ParisTeam2'];
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
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				vm.teamsSelected = [];
				vm.forTest_selectSite(vm.sites[0]);
			})
			.apply(function() {
				vm.goToAgents();
			});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			siteIds: [],
			teamIds: []
		}, {
			reload: true,
			notify: true
		});
	});

	it('should unselect site when the only team under it is unselected', function() {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				vm.forTest_selectSite(vm.sites[0])
			})
			.apply(function() {
				vm.teamsSelected = [];
			});

		expect(vm.sites[0].isChecked).toBe(false);
	});

	it('should go to agents with skill', function() {
		$fakeBackend.withSkill({
			Id: "phoneSkillGuid"
		})

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.selectedSkillChange({
				Id: "phoneSkillGuid"
			});
		});;

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			skillIds: 'phoneSkillGuid',
			skillAreaId: undefined,
			siteIds: [],
			teamIds: []
		}, {
			reload: true,
			notify: true
		});
	});

	it('should go to agents with skillArea', function() {
		$fakeBackend.withSkillAreas([{
			Id: "phoneAndEmailGuid"
		}]);

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.selectedSkillAreaChange({
				Id: "phoneAndEmailGuid"
			});
		});

		expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
			skillAreaId: 'phoneAndEmailGuid',
			skillIds: [],
			siteIds: [],
			teamIds: []
		}, {
			reload: true,
			notify: true
		});
	});

	it('should keep skill in selection', function() {
		stateParams.skillIds = ["phoneSkillGuid"];
		$fakeBackend.withSkill({
			Id: "phoneSkillGuid"
		})

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkill.Id).toBe("phoneSkillGuid");
	});

	it('should keep skillArea in selection', function() {
		stateParams.skillAreaId = "phoneAndEmailGuid";
		$fakeBackend.withSkillAreas([{
			Id: "phoneAndEmailGuid"
		}])

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkillArea.Id).toBe("phoneAndEmailGuid");
	});

	it('should go to agents by skillArea and clear skill from stateParams', function() {
		stateParams.skillIds = ["phoneSkillGuid"];
		$fakeBackend
			.withSkill({
				Id: "phoneSkillGuid"
			})
			.withSkillAreas([{
				Id: "phoneAndEmailGuid",
				Skills: [{
					Id: "phoneSkillGuid"
				}, {
					Id: "emailSkillGuid"
				}, ]
			}])

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.selectedSkillAreaChange({
				Id: "phoneAndEmailGuid"
			});

			expect($state.go).toHaveBeenCalledWith('rta.select-skill', {
				skillAreaId: 'phoneAndEmailGuid',
				skillIds: [],
				siteIds: [],
				teamIds: []
			}, {
				reload: true,
				notify: true
			});
		});
	});

});
