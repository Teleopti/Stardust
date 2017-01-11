'use strict';
describe('RtaOverviewController', function() {
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

	it('should display site', function() {
		$fakeBackend.withSite({
			Name: "London",
			NumberOfAgents: 11
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites[0].Name).toEqual("London");
		expect(vm.sites[0].NumberOfAgents).toEqual(11);
	});

	it('should display agents out of adherence in sites', function() {
		$fakeBackend.withSite({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
			})
			.withSite({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
				Name: "Paris",
			})
			.withSiteAdherence({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				OutOfAdherence: 1
			})
			.withSiteAdherence({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
				OutOfAdherence: 5,
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites[0].OutOfAdherence).toEqual(1);
		expect(vm.sites[1].OutOfAdherence).toEqual(5);
	});

	it('should update adhernce', function() {
		$fakeBackend.withSite({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			})
			.withSiteAdherence({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				OutOfAdherence: 1
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				$fakeBackend.clearSiteAdherences()
					.withSiteAdherence({
						Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
						OutOfAdherence: 3
					});
			})
			.wait(5000);

		expect(vm.sites[0].OutOfAdherence).toEqual(3);
	});

	it('should stop polling when page is about to destroy', function() {
		$controllerBuilder.createController()
			.wait(5000);

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should go to agents for multiple sites', function() {
		$fakeBackend.withSite({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
			})
			.withSite({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
			});
		spyOn($state, 'go');

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.toggleSelection("d970a45a-90ff-4111-bfe1-9b5e015ab45c");
			vm.toggleSelection("6a21c802-7a34-4917-8dfd-9b5e015ab461");
			vm.openSelectedItems();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			siteIds: ['d970a45a-90ff-4111-bfe1-9b5e015ab45c',
				"6a21c802-7a34-4917-8dfd-9b5e015ab461"
			]
		});
	});

	it('should go to agents after deselecting site', function() {
		$fakeBackend.withSite({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
			})
			.withSite({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
			});
		spyOn($state, 'go');

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.toggleSelection("d970a45a-90ff-4111-bfe1-9b5e015ab45c");
			vm.toggleSelection("6a21c802-7a34-4917-8dfd-9b5e015ab461");
			vm.toggleSelection("d970a45a-90ff-4111-bfe1-9b5e015ab45c");
			vm.openSelectedItems();
		});

		expect($state.go).toHaveBeenCalledWith('rta.agents', {
			siteIds: ["6a21c802-7a34-4917-8dfd-9b5e015ab461"]
		});
	});

	it('should convert sites out of adherence and number of agents to percent', function() {
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

	it('should call notify service', function() {
		spyOn(NoticeService, 'info');

		$controllerBuilder.createController();

		expect(NoticeService.info).toHaveBeenCalled();
	});
});
