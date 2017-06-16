'use strict';
describe('RtaOverviewController', function () {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder,
		NoticeService,
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

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_, _NoticeService_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;
		NoticeService = _NoticeService_;

		scope = $controllerBuilder.setup('RtaOverviewController');

		$fakeBackend.clear();
	}));

	it('should display site', function () {
		$fakeBackend.withSiteAdherence({
			Id: "londonGuid",
			Name: "London",
			AgentsCount: 11,
			InAlarmCount: 5,
			Color: "warning"
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites[0].Id).toEqual("londonGuid");
		expect(vm.sites[0].Name).toEqual("London");
		expect(vm.sites[0].AgentsCount).toEqual(11);
		expect(vm.sites[0].InAlarmCount).toEqual(5);
		expect(vm.sites[0].Color).toEqual("warning");
	});

	it('should update adherence', function () {
		$fakeBackend.withSiteAdherence({
			Id: "londonGuid",
			AgentsCount: 11,
			InAlarmCount: 5,
			Color: "warning"
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			$fakeBackend.clearSiteAdherences()
				.withSiteAdherence({
					Id: "londonGuid",
					AgentsCount: 11,
					InAlarmCount: 2,
					Color: "good"
				});
		})
			.wait(5000);

		expect(vm.sites[0].InAlarmCount).toEqual(2);
		expect(vm.sites[0].Color).toEqual("good");
	});

	it('should stop polling when page is about to destroy', function () {
		$controllerBuilder.createController()
			.wait(5000);

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should go to agents for multiple sites', function () {
		$fakeBackend.withSiteAdherence({
			Id: "londonGuid"
		})
			.withOrganization({
				Id: "londonGuid",
				FullPermission: true
			})
			.withSiteAdherence({
				Id: "parisGuid"
			})
			.withOrganization({
				Id: "parisGuid",
				FullPermission: true
			});
		spyOn($state, 'go');

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.toggleSelection("londonGuid");
			vm.toggleSelection("parisGuid");
			vm.openSelectedItems();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			siteIds: ['londonGuid',
				"parisGuid"
			]
		});
	});

	it('should go to agents after deselecting site', function () {
		$fakeBackend.withSiteAdherence({
			Id: "londonGuid"
		})
			.withOrganization({
				Id: "londonGuid",
				FullPermission: true
			})
			.withSiteAdherence({
				Id: "parisGuid"
			})
			.withOrganization({
				Id: "parisGuid",
				FullPermission: true
			});
		spyOn($state, 'go');

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.toggleSelection("londonGuid");
			vm.toggleSelection("parisGuid");
			vm.toggleSelection("londonGuid");
			vm.openSelectedItems();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			siteIds: ["parisGuid"]
		});
	});

	it('should go to agents for permitted teams in site', function () {
		$fakeBackend.withSiteAdherence({
			Id: "londonGuid"
		})
			.withOrganization({
				Id: 'londonGuid',
				FullPermission: false,
				Name: 'London',
				Teams: [{
					Id: 'teamId',
					Name: 'Team Preferences'
				}]
			});
		spyOn($state, 'go');

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.toggleSelection("londonGuid");
			vm.openSelectedItems();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			teamIds: ['teamId']
		});
	});

	it('should go to agents for permitted teams in site1  and site2', function () {
		$fakeBackend.withSiteAdherence({
			Id: "londonGuid1"
		}).withSiteAdherence({
			Id: "londonGuid2"
		})
			.withOrganization({
				Id: 'londonGuid1',
				FullPermission: false,
				Name: 'London',
				Teams: [{
					Id: 'teamId1',
					Name: 'Team Preferences'
				}]
			})
			.withOrganization({
				Id: 'londonGuid2',
				FullPermission: false,
				Name: 'London2',
				Teams: [{
					Id: 'teamId2',
					Name: 'Team Preferences2'
				}]
			});
		spyOn($state, 'go');

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.toggleSelection("londonGuid1");
			vm.toggleSelection("londonGuid2");
			vm.openSelectedItems();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			teamIds: ['teamId1', 'teamId2']
		});
	});

	it('should not go to untoggled site', function () {
		$fakeBackend.withSiteAdherence({
			Id: "londonGuid1"
		}).withSiteAdherence({
			Id: "londonGuid2"
		})
			.withOrganization({
				Id: 'londonGuid1',
				FullPermission: false,
				Name: 'London',
				Teams: [{
					Id: 'teamId1',
					Name: 'Team Preferences'
				}]
			})
			.withOrganization({
				Id: 'londonGuid2',
				FullPermission: false,
				Name: 'London2',
				Teams: [{
					Id: 'teamId2',
					Name: 'Team Preferences2'
				}]
			});
		spyOn($state, 'go');

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.toggleSelection("londonGuid1");
			vm.toggleSelection("londonGuid1");
			vm.toggleSelection("londonGuid2");
			vm.openSelectedItems();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			teamIds: ['teamId2']
		});
	});

});
