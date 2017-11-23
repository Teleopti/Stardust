'use strict';
describe('RtaFilterController', function () {
	var $interval,
		$httpBackend,
		$state,
		scope,
		$fakeBackend,
		$controllerBuilder,
		$timeout,
		vm;
	var stateParams = {};
	var lastGoParams = {};

	beforeEach(module('wfm.rta'));
	beforeEach(module('wfm.rtaTestShared'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.factory('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _FakeRtaBackend_, _ControllerBuilder_, _$timeout_) {
		$interval = _$interval_;
		$state = _$state_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;
		$timeout = _$timeout_;

		scope = $controllerBuilder.setup('RtaFilterController39082');

		$fakeBackend.clear();
		
		lastGoParams = {};
		spyOn($state, 'go').and.callFake(function (_, params) {
			lastGoParams = params;
		});
	}));

	it('should get organization', function () {
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

		var c = $controllerBuilder.createController();
		vm = c.vm;

		expect(vm.sites.length).toEqual(2);
		expect(vm.sites[0].Id).toEqual('LondonGuid');
		expect(vm.sites[0].Name).toEqual('London');
		expect(vm.sites[0].Teams.length).toEqual(2);
		expect(vm.sites[0].Teams[0].Id).toEqual('1');
		expect(vm.sites[0].Teams[0].Name).toEqual('Team Preferences');
		expect(vm.sites[0].Teams[0].isChecked).toEqual(false);
		expect(vm.sites[0].Teams[1].Id).toEqual('2');
		expect(vm.sites[0].Teams[1].Name).toEqual('Team Students');
		expect(vm.sites[0].Teams[1].isChecked).toEqual(false);
	});

	it('should get organization by skill', function () {
		stateParams.skillIds = "phoneGuid";
		$fakeBackend.withOrganizationOnSkills({
			Id: 'LondonGuid',
			Name: 'London',
			Teams: [{
				Id: '1',
				Name: 'Team Preferences'
			}, {
				Id: '2',
				Name: 'Team Students'
			}]
		}, 'phoneGuid');

		var c = $controllerBuilder.createController();
		vm = c.vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Id).toEqual('LondonGuid');
		expect(vm.sites[0].Name).toEqual('London');
		expect(vm.sites[0].Teams.length).toEqual(2);
		expect(vm.sites[0].Teams[0].Id).toEqual('1');
		expect(vm.sites[0].Teams[0].Name).toEqual('Team Preferences');
		expect(vm.sites[0].Teams[0].isChecked).toEqual(false);
		expect(vm.sites[0].Teams[1].Id).toEqual('2');
		expect(vm.sites[0].Teams[1].Name).toEqual('Team Students');
		expect(vm.sites[0].Teams[1].isChecked).toEqual(false);
	});

	it('should get organization by skill area', function () {
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
			}, 'emailGuid, phoneGuid');

		var c = $controllerBuilder.createController();
		vm = c.vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Id).toEqual('LondonGuid');
		expect(vm.sites[0].Name).toEqual('London');
		expect(vm.sites[0].Teams.length).toEqual(2);
		expect(vm.sites[0].Teams[0].Id).toEqual('1');
		expect(vm.sites[0].Teams[0].Name).toEqual('Team Preferences');
		expect(vm.sites[0].Teams[0].isChecked).toEqual(false);
		expect(vm.sites[0].Teams[1].Id).toEqual('2');
		expect(vm.sites[0].Teams[1].Name).toEqual('Team Students');
		expect(vm.sites[0].Teams[1].isChecked).toEqual(false);
	});

	it('should select all teams when selecting site', function () {
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
		c.apply(function () {
			vm.sites[0].toggle();
		});


		expect(vm.sites[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[1].isChecked).toBe(true);
	});

	it('should unselect all teams when unselecting site', function () {
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
		c.apply(function () {
			vm.sites[0].toggle();
			vm.sites[0].toggle();
		});

		expect(vm.sites[0].isChecked).toBe(false);
		expect(vm.sites[0].Teams[0].isChecked).toBe(false);
		expect(vm.sites[0].Teams[1].isChecked).toBe(false);
	});

	it('should unselect site when unselecting all teams and site was in stateParams', function () {
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
		c.apply(function () {
			vm.sites[0].Teams[0].toggle();
		})
			.apply(function () {
				vm.sites[0].Teams[1].toggle();
			});

		expect(vm.sites[0].isChecked).toBe(false);
	});

	it('should only unselect one team when a site was selected', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			},]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.sites[0].toggle();
		})
			.apply(function () {
				vm.sites[0].Teams[1].toggle();
			});

		expect(vm.sites[0].isChecked).toBe(false);
		expect(vm.sites[0].isMarked).toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[1].isChecked).toBe(false);
	});

	it('should go to agents on site', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'TeamGuid'
			}]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].toggle();
			vm.goToAgents();
		});

		expect(lastGoParams.siteIds).toEqual(['LondonGuid']);
	});

	it('should go to agents on sites', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'TeamLondon'
			}]
		})
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: true,
				Teams: [{
					Id: 'TeamParis'
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].toggle();
			vm.sites[1].toggle();
			vm.goToAgents();
		});

		expect(lastGoParams.siteIds).toEqual(['LondonGuid', 'ParisGuid']);
	});

	it('should go to agents on team', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			},]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].Teams[0].toggle();
			vm.goToAgents();
		});

		expect(lastGoParams.teamIds).toEqual(['LondonTeam1']);
	});

	it('should go to agents on teams', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}]
		})
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: true,
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].Teams[0].toggle();
			vm.sites[1].Teams[0].toggle();
			vm.goToAgents();
		});

		expect(lastGoParams.teamIds).toEqual(['LondonTeam1', 'ParisTeam1']);
	});

	it('should go to agents on site and team', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}]
		})
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: true,
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].toggle();
			vm.sites[1].Teams[0].toggle();
			vm.goToAgents();
		});

		expect(lastGoParams.siteIds).toEqual(['LondonGuid']);
		expect(lastGoParams.teamIds).toEqual(['ParisTeam1']);
	});

	it('should go to agents when unselecting team', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}]
		})
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: true,
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].Teams[0].toggle();
			vm.sites[0].Teams[0].toggle();
			vm.sites[1].Teams[0].toggle();
			vm.goToAgents();
		});

		expect(lastGoParams.teamIds).toEqual(['ParisTeam1']);
	});

	it('should go to agents when unselecting site', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'LondonTeam1'
			}]
		})
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: true,
				Teams: [{
					Id: 'ParisTeam1',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].toggle();
			vm.sites[1].toggle();
			vm.sites[0].toggle();
		})
			.apply(function () {
				vm.goToAgents();
			});

		expect(lastGoParams.siteIds).toEqual(['ParisGuid']);
	});

	it('should unselect site when preselected', function () {
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
		c.apply(function () {
			vm.sites[0].toggle();
		});

		expect(vm.sites[0].isChecked).toBe(false);
	});

	it('should select team', function () {
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
		c.apply(function () {
			vm.sites[0].toggle();
			vm.sites[0].Teams[0].toggle();
		});

		expect(vm.sites[0].isMarked).toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(false);
		expect(vm.sites[0].Teams[1].isChecked).toBe(true);
	});


	it('should not redirect when selection has not changed', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'LondonTeam1'
			}]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.sites[0].toggle();
			vm.sites[0].toggle();
		})
			.apply(function () {
				vm.goToAgents();
			});

		expect(lastGoParams).toEqual({});
	});

	it('should select site and teams when preexisting selection for site', function () {
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
		expect(vm.sites[0].Teams[0].isChecked).toEqual(true);
		expect(vm.sites[0].Teams[1].isChecked).toEqual(true);
	});

	it('should unselect site when preselected and team is unselected', function () {
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
		c.apply(function () {
			vm.sites[0].Teams[0].toggle();
		});

		expect(vm.sites[0].isChecked).toEqual(false);
		expect(vm.sites[0].isMarked).toEqual(true);
		expect(vm.sites[0].Teams[0].isChecked).toEqual(false);
		expect(vm.sites[0].Teams[1].isChecked).toEqual(true);
	});

	it('should unselect site and team', function () {
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
		c.apply(function () {
			vm.sites[0].toggle();
			vm.sites[0].Teams[0].toggle();
		})

		expect(vm.sites[0].isChecked).toEqual(false);
		expect(vm.sites[0].isMarked).toEqual(true);
		expect(vm.sites[0].Teams[0].isChecked).toEqual(false);
		expect(vm.sites[0].Teams[1].isChecked).toEqual(true);
	});

	it('should go to agents when selecting new site', function () {
		stateParams.siteIds = ['ParisGuid'];
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'LondonTeam1'
			}]
		})
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: true,
				Teams: [{
					Id: 'ParisTeam1',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].toggle();
			vm.goToAgents();
		});
		
		expect(lastGoParams.siteIds).toEqual(['LondonGuid', 'ParisGuid']);
	});

	it('should select site and teams when in site in stateParams', function () {
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

	it('should select team when team in stateParams', function () {
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

		expect(vm.sites[0].isChecked).toBe(false);
		expect(vm.sites[0].isMarked).toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
	});

	it('should select teams when teams in stateParams', function () {
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

	it('should select site and team when in stateParams', function () {
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
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
		expect(vm.sites[1].isChecked).toBe(false);
		expect(vm.sites[1].Teams[0].isChecked).toBe(true);
	});

	it('should go to agents when selecting new site and deselecting some teams in the old one', function () {
		stateParams.siteIds = ['ParisGuid'];
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'LondonTeam1'
			}]
		})
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: true,
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].Teams[0].toggle();
			vm.sites[1].Teams[0].toggle();
			vm.goToAgents();
		});

		expect(lastGoParams.siteIds).toEqual(['LondonGuid']);
		expect(lastGoParams.teamIds).toEqual(['ParisTeam2']);
	});

	it('should select site when all teams are selected', function () {
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
		c.apply(function () {
			vm.sites[0].Teams[0].toggle();
			vm.sites[0].Teams[1].toggle();
		});
		
		expect(vm.sites[0].isChecked).toEqual(true);
	});

	it('should mark site when some teams are selected', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			},]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.sites[0].Teams[0].toggle();
		});

		expect(vm.sites[0].isMarked).toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[1].isChecked).toBe(false);
		expect(vm.sites[0].isChecked).toBe(false);
	});

	it('should unselect site when all teams are unselected', function () {
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
		c.apply(function () {
			vm.sites[0].toggle();
			vm.sites[0].Teams[0].toggle();
			vm.sites[0].Teams[1].toggle();
		})

		expect(vm.sites[0].isChecked).toBe(false);
	});

	it('should go to agents on site when all teams are selected', function () {
		$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: true,
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].Teams[0].toggle();
			vm.sites[0].Teams[1].toggle();
			vm.goToAgents();

		});

		expect(lastGoParams.siteIds).toEqual(['ParisGuid']);
	});

	it('should go to agents on team when site was preselected', function () {
		stateParams.siteIds = ['ParisGuid'];
		$fakeBackend
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: true,
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].Teams[1].toggle();
			vm.goToAgents();
		});

		expect(lastGoParams.teamIds).toEqual(['ParisTeam1']);
	});

	it('should redirect to initial state after unselecting all', function () {
		stateParams.siteIds = ['LondonGuid'];
		stateParams.teamIds = ['ParisTeam2'];
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'LondonTeam1'
			}]
		})
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: true,
				Teams: [{
					Id: 'ParisTeam1',
				}, {
					Id: 'ParisTeam2',
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[1].Teams[1].toggle();
			vm.sites[0].toggle();
			vm.goToAgents();
		});

		expect(lastGoParams.siteIds).toEqual(undefined);
		expect(lastGoParams.teamIds).toEqual(undefined);
	});

	it('should unselect site when the only team under it is unselected', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.sites[0].toggle();
			vm.sites[0].Teams[0].toggle();
		})

		expect(vm.sites[0].isChecked).toBe(false);
	});

	it('should go to agents with skill when on agents view', function () {
		$state.current.name = "rta-agents";
		$fakeBackend.withSkill({
			Id: "phoneSkillGuid"
		})

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillChange({
				Id: "phoneSkillGuid"
			});
		});

		expect(lastGoParams.skillIds).toEqual(['phoneSkillGuid']);
		expect(lastGoParams.skillAreaId).toEqual(undefined);
	});

	it('should go to agents with skillArea when on agents view', function () {
		$state.current.name = "rta-agents";
		$fakeBackend.withSkillAreas([{
			Id: "phoneAndEmailGuid"
		}]);

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillAreaChange({
				Id: "phoneAndEmailGuid"
			});
		});

		expect(lastGoParams.skillIds).toEqual(undefined);
		expect(lastGoParams.skillAreaId).toEqual('phoneAndEmailGuid');
	});

	it('should go to agents by skillArea and clear skill from stateParams when on agents view', function () {
		$state.current.name = "rta-agents";
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
				},]
			}])

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillAreaChange({
				Id: "phoneAndEmailGuid"
			});
		});

		expect(lastGoParams.skillIds).toEqual(undefined);
		expect(lastGoParams.skillAreaId).toEqual('phoneAndEmailGuid');
	});

	it('should go to agents by skill and clear skillArea from stateParams when on agents view', function () {
		$state.current.name = "rta-agents";
		stateParams.skillAreaId = "phoneAndEmailGuid";
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
				},]
			}])

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillChange({
				Id: "phoneSkillGuid"
			});
		});

		expect(lastGoParams.skillIds).toEqual(['phoneSkillGuid']);
		expect(lastGoParams.skillAreaId).toEqual(undefined);
	});

	it('should go to agents by skill and clear site from stateParams when on agents view', function () {
		$state.current.name = "rta-agents";
		stateParams.skillAreaId = "phoneAndEmailGuid";
		stateParams.siteIds = ['londonGuid'];
		$fakeBackend
			.withSkill({
				Id: "phoneGuid"
			})
			.withSkillAreas([{
				Id: "phoneAndEmailGuid",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				},]
			}])
			.withOrganizationOnSkills({
				Id: 'londonGuid',
				Teams: [{
					Id: 'londonTeamGuid',
				}]
			}, "phoneGuid")
			.withOrganizationOnSkills({
				Id: 'londonGuid',
				Teams: [{
					Id: 'londonTeamGuid',
				}]
			}, "phoneGuid, emailGuid");

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillChange({
				Id: "phoneGuid"
			});
		});

		expect(lastGoParams.skillIds).toEqual(['phoneGuid']);
		expect(lastGoParams.skillAreaId).toEqual(undefined);
	});

	it('should go to agents by skill area and clear site from stateParams when on agents view', function () {
		$state.current.name = "rta-agents";
		stateParams.skillIds = "phoneGuid";
		stateParams.siteIds = ['londonGuid'];
		$fakeBackend
			.withSkill({
				Id: "phoneGuid"
			})
			.withSkillAreas([{
				Id: "phoneAndEmailGuid",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				},]
			}])
			.withOrganizationOnSkills({
				Id: 'londonGuid',
				Teams: [{
					Id: 'londonTeamGuid',
				}]
			}, "phoneGuid")
			.withOrganizationOnSkills({
				Id: 'londonGuid',
				Teams: [{
					Id: 'londonTeamGuid',
				}]
			}, "phoneGuid, emailGuid");

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.selectedSkillAreaChange({
				Id: "phoneAndEmailGuid"
			});
		});

		expect(lastGoParams.skillIds).toEqual(undefined);
		expect(lastGoParams.skillAreaId).toEqual('phoneAndEmailGuid');
	});

	xit('should go to agents on team when partial permission for site', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: false,
			Teams: [{
				Id: 'TeamGuid'
			}]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].toggle();
			vm.goToAgents();
		});

		expect(lastGoParams.teamIds).toEqual(['TeamGuid']);
	});

	xit('should go to agents on site and team when selecting sites and full permission for site1 and partial permission for site2', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'TeamLondonGuid'
			}]
		})
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: false,
				Teams: [{
					Id: 'TeamParisGuid'
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].toggle();
			vm.sites[1].toggle();
			vm.goToAgents();
		});

		expect(lastGoParams.siteIds).toEqual(['LondonGuid']);
		expect(lastGoParams.teamIds).toEqual(['TeamParisGuid']);
	});

	xit('should go to agents on site and team when selecting team and sitefull permission for site1 and partial permission for site2', function () {
		$fakeBackend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'TeamLondonGuid'
			}]
		})
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: false,
				Teams: [{
					Id: 'TeamParisGuid'
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.openPicker = true;
			vm.sites[0].Teams[0].toggle();
			vm.sites[1].toggle();
			vm.goToAgents();
		});

		expect(lastGoParams.siteIds).toEqual(['LondonGuid']);
		expect(lastGoParams.teamIds).toEqual(['TeamParisGuid']);
	});

});
