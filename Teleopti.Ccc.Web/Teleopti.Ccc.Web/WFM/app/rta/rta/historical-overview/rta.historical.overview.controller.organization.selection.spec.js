'use strict';

rtaTester.describe('RtaHistoricalOverviewController', function (it, fit, xit) {

	it('should get organization', function (t) {
		t.backend.withOrganization({
			Id: 'LondonGuid',
			Name: 'London',
			Teams: [{
				Id: '1',
				Name: 'Team Preferences'
			}]
		});

		var vm = t.createController();

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Id).toEqual('LondonGuid');
		expect(vm.sites[0].Name).toEqual('London');
		expect(vm.sites[0].Teams.length).toEqual(1);
		expect(vm.sites[0].Teams[0].Id).toEqual('1');
		expect(vm.sites[0].Teams[0].Name).toEqual('Team Preferences');
		expect(vm.sites[0].Teams[0].isChecked).toEqual(false);
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

		var vm = t.createController();
		t.apply(function () {
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

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
			vm.sites[0].toggle();
		});

		expect(vm.sites[0].isChecked).toBe(false);
		expect(vm.sites[0].Teams[0].isChecked).toBe(false);
		expect(vm.sites[0].Teams[1].isChecked).toBe(false);
	});

	it('should unselect site when unselecting all teams and site was in stateParams', function (t) {
		t.stateParams.siteIds = ['LondonGuid'];
		t.backend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}]
		});

		var vm = t.createController();
		t.apply(function () {
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

		var vm = t.createController();
		t.apply(function () {
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

	it('should go to historical overview based on site', function (t) {
		t.backend.withOrganization({
			Id: 'LondonGuid'
		});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
		});

		expect(t.lastGoParams.siteIds).toEqual(['LondonGuid']);
	});

	it('should go to historical overview based on sites', function (t) {
		t.backend.withOrganization({
			Id: 'LondonGuid'
		})
			.withOrganization({
				Id: 'ParisGuid'
			});
		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
			vm.sites[1].toggle();
		});

		expect(t.lastGoParams.siteIds).toEqual(['LondonGuid', 'ParisGuid']);
	});

	it('should go to historical overview on team', function (t) {
		t.backend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}]
		});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].Teams[0].toggle();
		});

		expect(t.lastGoParams.teamIds).toEqual(['LondonTeam1']);
	});

	it('should go to historical overview based on teams', function (t) {
		t.backend.withOrganization({
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
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].Teams[0].toggle();
			vm.sites[1].Teams[0].toggle();
		});

		expect(t.lastGoParams.teamIds).toEqual(['LondonTeam1', 'ParisTeam1']);
	});

	it('should go to historical overview based on site and team', function (t) {
		t.backend.withOrganization({
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
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
			vm.sites[1].Teams[0].toggle();
		});

		expect(t.lastGoParams.siteIds).toEqual(['LondonGuid']);
		expect(t.lastGoParams.teamIds).toEqual(['ParisTeam1']);
	});

	it('should go to historical overview when unselecting team', function (t) {
		t.backend.withOrganization({
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
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].Teams[0].toggle();
			vm.sites[0].Teams[0].toggle();
			vm.sites[1].Teams[0].toggle();
		});

		expect(t.lastGoParams.teamIds).toEqual(['ParisTeam1']);
	});

	it('should go to historical overview when unselecting site', function (t) {
		t.backend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}]
		})
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
			vm.sites[1].toggle();
			vm.sites[0].toggle();
		});

		expect(t.lastGoParams.siteIds).toEqual(['ParisGuid']);
	});

	it('should unselect site when preselected', function (t) {
		t.stateParams.siteIds = ['ParisGuid'];
		t.backend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
		});

		expect(vm.sites[0].isChecked).toBe(false);
	});
	
	it('should select team', function (t) {
		t.backend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
			vm.sites[0].Teams[0].toggle();
		});

		expect(vm.sites[0].isMarked).toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(false);
		expect(vm.sites[0].Teams[1].isChecked).toBe(true);
	});
	
	it('should select site and teams when preexisting selection for site', function (t) {
		t.stateParams.siteIds = ['ParisGuid'];
		t.backend.withOrganization({
			Id: 'ParisGuid',
			Teams: [{
				Id: 'ParisTeam1'
			}, {
				Id: 'ParisTeam2'
			}]
		});

		var vm = t.createController();
		
		expect(vm.sites[0].isChecked).toEqual(true);
		expect(vm.sites[0].Teams[0].isChecked).toEqual(true);
		expect(vm.sites[0].Teams[1].isChecked).toEqual(true);
	});

	it('should unselect site when preselected and team is unselected', function (t) {
		t.stateParams.siteIds = ['ParisGuid'];
		t.backend
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
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
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
			vm.sites[0].Teams[0].toggle();
		});

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
					Id: 'ParisTeam1'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
		});

		expect(t.lastGoParams.siteIds).toContain("LondonGuid");
		expect(t.lastGoParams.siteIds).toContain("ParisGuid");
	});

	it('should select site and teams when in site in stateParams', function (t) {
		t.stateParams.siteIds = ['LondonGuid'];
		t.backend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}]
		});

		var vm = t.createController();

		expect(vm.sites[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
	});

	it('should select team when team in stateParams', function (t) {
		t.stateParams.teamIds = ['LondonTeam1'];
		t.backend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}, {
				Id: 'LondonTeam2'
			}]
		});

		var vm = t.createController();

		expect(vm.sites[0].isChecked).toBe(false);
		expect(vm.sites[0].isMarked).toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
	});

	it('should select teams when teams in stateParams', function (t) {
		t.stateParams.teamIds = ['LondonTeam1', 'LondonTeam2'];
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

		var vm = t.createController();

		expect(vm.sites[0].isChecked).not.toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[1].isChecked).toBe(true);

	});

	it('should select site and team when in stateParams', function (t) {
		t.stateParams.siteIds = ['LondonGuid'];
		t.stateParams.teamIds = ['ParisTeam1'];
		t.backend.withOrganization({
			Id: 'LondonGuid',
			Teams: [{
				Id: 'LondonTeam1'
			}]
		})
			.withOrganization({
				Id: 'ParisGuid',
				Teams: [{
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();

		expect(vm.sites[0].isChecked).toBe(true);
		expect(vm.sites[0].Teams[0].isChecked).toBe(true);
		expect(vm.sites[1].isChecked).toBe(false);
		expect(vm.sites[1].Teams[0].isChecked).toBe(true);
	});

	it('should go to agents when selecting new site and deselecting some teams in the old one', function (t) {
		t.stateParams.siteIds = ['ParisGuid'];
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
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
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
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
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

		var vm = t.createController();
		t.apply(function () {
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
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
			vm.sites[0].Teams[0].toggle();
			vm.sites[0].Teams[1].toggle();
		});

		expect(vm.sites[0].isChecked).toBe(false);
	});

	it('should go to agents on site when all teams are selected', function (t) {
		t.backend
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: true,
				Teams: [{
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].Teams[0].toggle();
			vm.sites[0].Teams[1].toggle();
		});

		expect(t.lastGoParams.siteIds).toEqual(['ParisGuid']);
	});

	it('should go to agents on team when site was preselected', function (t) {
		t.stateParams.siteIds = ['ParisGuid'];
		t.backend
			.withOrganization({
				Id: 'ParisGuid',
				FullPermission: true,
				Teams: [{
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].Teams[1].toggle();
		});

		expect(t.lastGoParams.teamIds).toEqual(['ParisTeam1']);
	});

	it('should redirect to initial state after unselecting all', function (t) {
		t.stateParams.siteIds = ['LondonGuid'];
		t.stateParams.teamIds = ['ParisTeam2'];
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
					Id: 'ParisTeam1'
				}, {
					Id: 'ParisTeam2'
				}]
			});

		var vm = t.createController();
		t.apply(function () {
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

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
			vm.sites[0].Teams[0].toggle();
		});

		expect(vm.sites[0].isChecked).toBe(false);
	});

	it('should display site name from url', function (t) {
		t.stateParams.siteIds = ['id'];
		t.backend.withOrganization({
			Id: 'id',
			Name: 'Site'
		});

		var vm = t.createController();

		expect(vm.organizationPickerSelectionText).toContain("Site");
	});

	it('should display empty string', function (t) {
		t.backend.withOrganization({
			Id: 'id1',
			Name: 'Site1'
		})
			.withOrganization({
				Id: 'id2',
				Name: 'Site2'
			});

		var vm = t.createController();

		expect(vm.organizationPickerSelectionText).toEqual("");
	});

	it('should display empty string when sites have teams', function (t) {
		t.backend.withOrganization({
			Id: 'id1',
			Name: 'Site1',
			Teams: [{
				Name: 'Team1'
			}]
		})
			.withOrganization({
				Id: 'id2',
				Name: 'Site2',
				Teams: [{
					Name: 'Team2'
				}]
			});

		var vm = t.createController();

		expect(vm.organizationPickerSelectionText).toEqual("");
	});

});