'use strict';
describe('RtaOverviewController', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope,
		NoticeService,
		vm;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
		module(function($provide) {
			$provide.factory('$stateParams', function() {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function(_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_, _NoticeService_) {
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

	it('should display team for site', function() {
		stateParams.siteIds = "d970a45a-90ff-4111-bfe1-9b5e015ab45c";
		$fakeBackend.withTeam({
			SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			Name: "Green",
			NumberOfAgents: 1
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.teams[0].Name).toEqual("Green");
		expect(vm.teams[0].NumberOfAgents).toEqual(1);
	});

	it('should display agents out of adherence in the team', function() {
		stateParams.siteIds = "d970a45a-90ff-4111-bfe1-9b5e015ab45c";
		$fakeBackend.withTeam({
				Id: "2d45a50e-db48-41db-b771-a53000ef6565",
				SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
			})
			.withTeam({
				Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495",
				SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
			})
			.withTeamAdherence({
				Id: "2d45a50e-db48-41db-b771-a53000ef6565",
				OutOfAdherence: 1
			})
			.withTeamAdherence({
				Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495",
				OutOfAdherence: 5,
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.teams[0].OutOfAdherence).toEqual(1);
		expect(vm.teams[1].OutOfAdherence).toEqual(5);
	});

	it('should update adherence', function() {
		stateParams.siteIds = "6a21c802-7a34-4917-8dfd-9b5e015ab461";
		$fakeBackend.withTeam({
				Id: "2d45a50e-db48-41db-b771-a53000ef6565",
				SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
			})
			.withTeamAdherence({
				Id: "2d45a50e-db48-41db-b771-a53000ef6565",
				OutOfAdherence: 1
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				$fakeBackend.clearTeamAdherences()
					.withTeamAdherence({
						Id: "2d45a50e-db48-41db-b771-a53000ef6565",
						OutOfAdherence: 3
					});
			})
			.wait(5000);

		expect(vm.teams[0].OutOfAdherence).toEqual(3);
	});

	it('should stop polling when page is about to destroy', function() {
		stateParams.siteIds = "6a21c802-7a34-4917-8dfd-9b5e015ab461";
		$controllerBuilder.createController()
			.wait(5000);

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should go to agents for multiple teams', function() {
		stateParams.siteIds = "6a21c802-7a34-4917-8dfd-9b5e015ab461";
		$fakeBackend.withTeam({
				Id: "2d45a50e-db48-41db-b771-a53000ef6565"
			})
			.withTeam({
				Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495"
			});
		spyOn($state, 'go');

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.toggleSelection("2d45a50e-db48-41db-b771-a53000ef6565");
			vm.toggleSelection("0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495");
			vm.openSelectedItems();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			teamIds: ['2d45a50e-db48-41db-b771-a53000ef6565',
				"0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495"
			]
		});
	});

	it('should go to agents after deselecting team', function() {
		stateParams.siteIds = "6a21c802-7a34-4917-8dfd-9b5e015ab461";
		$fakeBackend.withTeam({
				Id: "2d45a50e-db48-41db-b771-a53000ef6565"
			})
			.withTeam({
				Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495"
			});
		spyOn($state, 'go');

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.toggleSelection("2d45a50e-db48-41db-b771-a53000ef6565");
			vm.toggleSelection("0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495");
			vm.toggleSelection("2d45a50e-db48-41db-b771-a53000ef6565");
			vm.openSelectedItems();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			teamIds: ["0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495"]
		});
	});

	it('should go back to sites when business unit is changed', function() {
		stateParams.siteIds = "6a21c802-7a34-4917-8dfd-9b5e015ab461";
		$sessionStorage.buid = "928dd0bc-bf40-412e-b970-9b5e015aadea";
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function() {
				$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
			});

		expect($state.go).toHaveBeenCalledWith('rta');
	});

	it('should not go back to sites overview when business unit is not initialized yet', function() {
		stateParams.siteIds = "6a21c802-7a34-4917-8dfd-9b5e015ab461";
		$sessionStorage.buid = undefined;
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function() {
				$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
			});

		expect($state.go).not.toHaveBeenCalledWith('rta');
	});

	it('should convert teams out of adherence and number of agents to percent', function() {
		stateParams.siteIds = "6a21c802-7a34-4917-8dfd-9b5e015ab461";
		//$fakeBackend.withTeamAdherence({
		//		OutOfAdherence: 5
		//	})
		//	.withTeam({
		//		NumberOfAgents: 10
		//	});

		vm = $controllerBuilder.createController().vm;

		var result = vm.getAdherencePercent(5, 10);

		expect(result).toEqual(50);
	});


	it('should not call notify service', function() {
		stateParams.siteIds = "6a21c802-7a34-4917-8dfd-9b5e015ab461";
		spyOn(NoticeService, 'info');

		$controllerBuilder.createController();

		expect(NoticeService.info).not.toHaveBeenCalled();
	});
});
