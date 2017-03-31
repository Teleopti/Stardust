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
		$fakeBackend.withToggle('RTA_SnappierDisplayOfOverview_43568');
	}));

	it('should display site', function () {
		$fakeBackend.withSiteAdherence({
			Id: "londonGuid",
			Name: "London",
			NumberOfAgents: 11,
			OutOfAdherence: 5,
			Color: "warning"
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites[0].Id).toEqual("londonGuid");
		expect(vm.sites[0].Name).toEqual("London");
		expect(vm.sites[0].NumberOfAgents).toEqual(11);
		expect(vm.sites[0].OutOfAdherence).toEqual(5);
		expect(vm.sites[0].Color).toEqual("warning");
	});

	it('should update adherence', function () {
		$fakeBackend.withSiteAdherence({
			Id: "londonGuid",
			NumberOfAgents: 11,
			OutOfAdherence: 5,
			Color: "warning"
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			$fakeBackend.clearSiteAdherences()
				.withSiteAdherence({
					Id: "londonGuid",
					NumberOfAgents: 11,
					OutOfAdherence: 2,
					Color: "good"
				});
		})
			.wait(5000);

		expect(vm.sites[0].OutOfAdherence).toEqual(2);
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
			.withSiteAdherence({
				Id: "parisGuid"
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
			.withSiteAdherence({
				Id: "parisGuid"
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

	it('should convert sites out of adherence and number of agents to percent', function () {
		//$fakeBackend.withSite({
		//		Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
		//		NumberOfAgents: 11
		//	})
		//	.withSiteAdherence({
		//		Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
		//		OutOfAdherence: 1
		//	});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		var result = vm.getAdherencePercent(1, 11);

		expect(result).toEqual(9);
	});

	it('should call notify service', function () {
		spyOn(NoticeService, 'info');

		$controllerBuilder.createController();

		expect(NoticeService.info).toHaveBeenCalled();
	});
});
