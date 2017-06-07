'use strict';
describe('RtaOverviewController', function () {
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

	it('should display team for site', function () {
		stateParams.siteIds = "londonGuid";
		$fakeBackend.withTeamAdherence({
			SiteId: "londonGuid",
			Name: "Green",
			NumberOfAgents: 11,
			OutOfAdherence: 5,
			Color: "warning"
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.teams[0].Name).toEqual("Green");
		expect(vm.teams[0].NumberOfAgents).toEqual(11);
		expect(vm.teams[0].OutOfAdherence).toEqual(5);
		expect(vm.teams[0].Color).toEqual("warning");
	});

	it('should update adherence', function () {
		stateParams.siteIds = "londonGuid";
		$fakeBackend.withTeamAdherence({
			SiteId: "londonGuid",
			NumberOfAgents: 11,
			OutOfAdherence: 5,
			Color: "warning"
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			$fakeBackend.clearTeamAdherences()
				.withTeamAdherence({
					SiteId: "londonGuid",
					Name: "Green",
					NumberOfAgents: 11,
					OutOfAdherence: 2,
					Color: "good"
				});
		})
			.wait(5000);

		expect(vm.teams[0].OutOfAdherence).toEqual(2);
		expect(vm.teams[0].Color).toEqual("good");
	});

	it('should stop polling when page is about to destroy', function () {
		stateParams.siteIds = "londonGuid";
		$controllerBuilder.createController()
			.wait(5000);

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should go to agents for multiple teams', function () {
		stateParams.siteIds = "londonGuid";
		$fakeBackend.withTeamAdherence({
			Id: "redGuid"
		})
			.withTeamAdherence({
				Id: "greenGuid"
			});
		spyOn($state, 'go');

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.toggleSelection("redGuid");
			vm.toggleSelection("greenGuid");
			vm.openSelectedItems();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			teamIds: ['redGuid',
				"greenGuid"
			]
		});
	});

	it('should go to agents after deselecting team', function () {
		stateParams.siteIds = "londonGuid";
		$fakeBackend.withTeamAdherence({
			Id: "redGuid"
		})
			.withTeam({
				Id: "greenGuid"
			});
		spyOn($state, 'go');

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			vm.toggleSelection("redGuid");
			vm.toggleSelection("greenGuid");
			vm.toggleSelection("redGuid");
			vm.openSelectedItems();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			teamIds: ["greenGuid"]
		});
	});

	it('should go back to sites when business unit is changed', function () {
		stateParams.siteIds = "londonGuid";
		$sessionStorage.buid = "oldBuGuid";
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function () {
				$sessionStorage.buid = "newBuGuid";
			});

		expect($state.go).toHaveBeenCalledWith('rta');
	});

	it('should not go back to sites overview when business unit is not initialized yet', function () {
		stateParams.siteIds = "londonGuid";
		$sessionStorage.buid = undefined;
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function () {
				$sessionStorage.buid = "newBuGuid";
			});

		expect($state.go).not.toHaveBeenCalledWith('rta');
	});
	
	it('should not call notify service', function () {
		stateParams.siteIds = "6a21c802-7a34-4917-8dfd-9b5e015ab461";
		spyOn(NoticeService, 'info');

		$controllerBuilder.createController();

		expect(NoticeService.info).not.toHaveBeenCalled();
	});
});
