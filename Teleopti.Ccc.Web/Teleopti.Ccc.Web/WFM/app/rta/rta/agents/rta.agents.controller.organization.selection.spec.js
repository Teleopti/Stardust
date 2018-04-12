'use strict';
rtaTester.describe('RtaAgentsController', function (it, fit, xit, _,
													$state,
													$controllerBuilder,
													stateParams) {
	var vm;

	it('should get organization', function (t) {
		t.backend.withOrganization({
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

	it('should select all teams when selecting site', function (t) {
		t.backend.withOrganization({
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

	it('should unselect all teams when unselecting site', function (t) {
		t.backend.withOrganization({
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

	it('should unselect site when unselecting all teams and site was in stateParams', function (t) {
		stateParams.siteIds = ['LondonGuid'];
		t.backend.withOrganization({
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

	it('should only unselect one team when a site was selected', function (t) {
		t.backend.withOrganization({
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

	it('should go to agents on site', function (t) {
		t.backend.withOrganization({
			Id: 'LondonGuid',
			FullPermission: true,
			Teams: [{
				Id: 'TeamGuid'
			}]
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.sites[0].toggle();
		});

		expect(t.lastGoParams.siteIds).toEqual(['LondonGuid']);
	});

	it('should go to agents on sites', function (t) {
		t.backend.withOrganization({
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
			vm.sites[0].toggle();
			vm.sites[1].toggle();
		});

		expect(t.lastGoParams.siteIds).toEqual(['LondonGuid', 'ParisGuid']);
	});

	it('should go to agents on team', function (t) {
		t.backend.withOrganization({
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
			vm.sites[0].Teams[0].toggle();
		});

		expect(t.lastGoParams.teamIds).toEqual(['LondonTeam1']);
	});

	it('should go to agents on teams', function (t) {
		t.backend.withOrganization({
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
			vm.sites[0].Teams[0].toggle();
			vm.sites[1].Teams[0].toggle();
		});

		expect(t.lastGoParams.teamIds).toEqual(['LondonTeam1', 'ParisTeam1']);
	});

	it('should go to agents on site and team', function (t) {
		t.backend.withOrganization({
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
			vm.sites[0].toggle();
			vm.sites[1].Teams[0].toggle();
		});

		expect(t.lastGoParams.siteIds).toEqual(['LondonGuid']);
		expect(t.lastGoParams.teamIds).toEqual(['ParisTeam1']);
	});

	it('should go to agents when unselecting team', function (t) {
		t.backend.withOrganization({
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
			vm.sites[0].Teams[0].toggle();
			vm.sites[0].Teams[0].toggle();
			vm.sites[1].Teams[0].toggle();
		});

		expect(t.lastGoParams.teamIds).toEqual(['ParisTeam1']);
	});

	it('should go to agents when unselecting site', function (t) {
		t.backend.withOrganization({
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
			vm.sites[0].toggle();
			vm.sites[1].toggle();
			vm.sites[0].toggle();
		});

		expect(t.lastGoParams.siteIds).toEqual(['ParisGuid']);
	});

	it('should unselect site when preselected', function (t) {
		stateParams.siteIds = ['ParisGuid'];
		t.backend
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

	it('should select team', function (t) {
		t.backend
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

	it('should not redirect when selection has not changed', function (t) {
		t.backend.withOrganization({
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
		});

		expect(t.lastGoParams.skillAreaId).toBeUndefined();
		expect(t.lastGoParams.skillIds).toBeUndefined();
		expect(t.lastGoParams.siteIds).toBeUndefined();
		expect(t.lastGoParams.teamIds).toBeUndefined();
	});

	it('should select site and teams when preexisting selection for site', function (t) {
		stateParams.siteIds = ['ParisGuid'];
		t.backend.withOrganization({
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

	it('should unselect site when preselected and team is unselected', function (t) {
		stateParams.siteIds = ['ParisGuid'];
		t.backend
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

	it('should unselect site and team', function (t) {
		t.backend
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

	it('should select new site', function (t) {
		t.stateParams.siteIds = ['ParisGuid'];
		t.backend
			.withOrganization({
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
			vm.sites[0].toggle();
		});

		expect(t.lastGoParams.siteIds).toContain("LondonGuid");
		expect(t.lastGoParams.siteIds).toContain("ParisGuid");
	});

	it('should select site and teams when in site in stateParams', function (t) {
		stateParams.siteIds = ['LondonGuid'];
		t.backend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}]
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
	});

	it('should select team when team in stateParams', function (t) {
		stateParams.teamIds = ['LondonTeam1'];
		t.backend.withOrganization({
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

	it('should select teams when teams in stateParams', function (t) {
		stateParams.teamIds = ['LondonTeam1', 'LondonTeam2'];
		t.backend.withOrganization({
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

	it('should select site and team when in stateParams', function (t) {
		stateParams.siteIds = ['LondonGuid'];
		stateParams.teamIds = ['ParisTeam1']
		t.backend.withOrganization({
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

	it('should go to agents when selecting new site and deselecting some teams in the old one', function (t) {
		stateParams.siteIds = ['ParisGuid'];
		t.backend.withOrganization({
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
			vm.sites[0].Teams[0].toggle();
			vm.sites[1].Teams[0].toggle();
		});

		expect(t.lastGoParams.siteIds).toEqual(['LondonGuid']);
		expect(t.lastGoParams.teamIds).toEqual(['ParisTeam2']);
	});

	it('should select site when all teams are selected', function (t) {
		t.backend
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

	it('should mark site when some teams are selected', function (t) {
		t.backend.withOrganization({
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

	it('should unselect site when all teams are unselected', function (t) {
		t.backend
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

	it('should go to agents on site when all teams are selected', function (t) {
		t.backend
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
			vm.sites[0].Teams[0].toggle();
			vm.sites[0].Teams[1].toggle();
		});

		expect(t.lastGoParams.siteIds).toEqual(['ParisGuid']);
	});

	it('should go to agents on team when site was preselected', function (t) {
		stateParams.siteIds = ['ParisGuid'];
		t.backend
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
			vm.sites[0].Teams[1].toggle();
		});

		expect(t.lastGoParams.teamIds).toEqual(['ParisTeam1']);
	});

	it('should redirect to initial state after unselecting all', function (t) {
		stateParams.siteIds = ['LondonGuid'];
		stateParams.teamIds = ['ParisTeam2'];
		t.backend.withOrganization({
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
			vm.sites[1].Teams[1].toggle();
			vm.sites[0].toggle();
		});

		expect(t.lastGoParams.siteIds).toEqual(undefined);
		expect(t.lastGoParams.teamIds).toEqual(undefined);
	});

	it('should unselect site when the only team under it is unselected', function (t) {
		t.backend.withOrganization({
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

	it('should display site name from url', function (t) {
		stateParams.siteIds = ['id'];
		t.backend.withOrganization({
			Id: 'id',
			Name: 'Site',
		});

		var vm = t.createController();

		expect(vm.organizationPickerSelectionText).toContain("Site");
	});

});
