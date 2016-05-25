'use strict';
describe('RtaSitesCtrl', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.service('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		scope = $controllerBuilder.setup('RtaSitesCtrl');

		$fakeBackend.clear();

	}));

	it('should display site', function () {
		$fakeBackend.withSite({
			Name: "London",
			NumberOfAgents: 11
		});

		$controllerBuilder.createController();

		expect(scope.sites[0].Name).toEqual("London");
		expect(scope.sites[0].NumberOfAgents).toEqual(11);
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

		$controllerBuilder.createController();

		expect(scope.sites[0].OutOfAdherence).toEqual(1);
		expect(scope.sites[1].OutOfAdherence).toEqual(5);
	});

	it('should update adhernce', function () {
		$fakeBackend.withSite({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			})
			.withSiteAdherence({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				OutOfAdherence: 1
			});

		$controllerBuilder.createController()
			.apply(function() {
				$fakeBackend.clearSiteAdherences()
					.withSiteAdherence({
						Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
						OutOfAdherence: 3
					});
			})
			.wait(5000);

		expect(scope.sites[0].OutOfAdherence).toEqual(3);
	});

	it('should stop polling when page is about to destroy', function() {
		$controllerBuilder.createController()
			.wait(5000);

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should go to teams', function() {
		$fakeBackend.withSite({
			Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			NumberOfAgents: 1
		});
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function() {
				scope.onSiteSelect({
					Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
					NumberOfAgents: 1
				});
			});

		expect($state.go).toHaveBeenCalledWith('rta.teams', {
			siteId: 'd970a45a-90ff-4111-bfe1-9b5e015ab45c'
		});
	});

	it('should go to agents for multiple sites', function() {
		$fakeBackend.withSite({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
			})
			.withSite({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
			});
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function() {
				scope.toggleSelection("d970a45a-90ff-4111-bfe1-9b5e015ab45c");
				scope.toggleSelection("6a21c802-7a34-4917-8dfd-9b5e015ab461");
				scope.openSelectedSites();
			});

		expect($state.go).toHaveBeenCalledWith('rta.agents-sites', {
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

		$controllerBuilder.createController()
			.apply(function () {
				scope.toggleSelection("d970a45a-90ff-4111-bfe1-9b5e015ab45c");
				scope.toggleSelection("6a21c802-7a34-4917-8dfd-9b5e015ab461");
				scope.toggleSelection("d970a45a-90ff-4111-bfe1-9b5e015ab45c");
				scope.openSelectedSites();
			});

		expect($state.go).toHaveBeenCalledWith('rta.agents-sites', {
			siteIds: ["6a21c802-7a34-4917-8dfd-9b5e015ab461"]
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

		$controllerBuilder.createController();

		var result = scope.getAdherencePercent(1, 11);

		expect(result).toEqual(9);
	});
});
