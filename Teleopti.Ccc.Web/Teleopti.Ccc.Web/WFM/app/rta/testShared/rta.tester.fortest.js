'use strict';

var rtaTester = (function () {

	var injectTester = function (sharedTestState, tests, withABagOfCandy) {

		var makeTester = function () {
			var controllerTester;
			return {
				createController: function () {
					controllerTester = sharedTestState.$controllerBuilder.createController();
					return controllerTester.controller;
				},
				get stateParams() {
					return sharedTestState.stateParams;
				},
				get backend() {
					return sharedTestState.$fakeBackend;
				},
				get controller() {
					return controllerTester.controller;
				},
				href: sharedTestState.$state.href,
				apply: function (a) {
					return controllerTester.apply(a)
				},
				wait: function (a) {
					return controllerTester.wait(a)
				},
				randomId: function () {
					return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
						var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
						return v.toString(16);
					});
				}
			}
		};

		var makeLegacyPolyfill = function () {
			return {
				createController: function () {
					return sharedTestState.$controllerBuilder.createController();
				},

				get current() {
					return sharedTestState.$state.current;
				},
				get go() {
					return sharedTestState.$state.go;
				},

				$emit: function (a) {
					return sharedTestState.scope.$emit(a);
				},
				flush: function () {
					return sharedTestState.$interval.flush();
				},
				verifyNoOutstandingRequest: function () {
					return sharedTestState.$httpBackend.verifyNoOutstandingRequest();
				},

				// fakeBackend stuff
				withTime: function (a) {
					return sharedTestState.$fakeBackend.withTime(a);
				},
				withAgentState: function (a) {
					return sharedTestState.$fakeBackend.withAgentState(a);
				},
				withPhoneState: function (a) {
					return sharedTestState.$fakeBackend.withPhoneState(a);
				},
				withSkillAreas: function (a) {
					return sharedTestState.$fakeBackend.withSkillAreas(a);
				},
				clearAgentStates: function (a) {
					return sharedTestState.$fakeBackend.clearAgentStates(a);
				},
				get lastAgentStatesRequestParams() {
					return sharedTestState.$fakeBackend.lastAgentStatesRequestParams;
				},

				// stateParams stuff
				set siteIds(value) {
					sharedTestState.stateParams.siteIds = value;
				},
				set teamIds(value) {
					sharedTestState.stateParams.teamIds = value;
				},
				set skillAreaId(value) {
					sharedTestState.stateParams.skillAreaId = value;
				},
				set skillIds(value) {
					sharedTestState.stateParams.skillIds = value;
				},
				set es(value) {
					sharedTestState.stateParams.es = value;
				},
				set showAllAgents(value) {
					sharedTestState.stateParams.showAllAgents = value;
				},

				// session storage stuff
				set buid(value) {
					sharedTestState.$sessionStorage.buid = value;
				},

				// NoticeService stuff
				get info() {
					return sharedTestState.NoticeService.info;
				},
				set info(value) {
					sharedTestState.NoticeService.info = value;
				},
			};
		};

		var wit = function (s, fn) {
			it(s, function () {
				fn(makeTester());
			});
		};

		var wfit = function (s, fn) {
			fit(s, function () {
				fn(makeTester());
			});
		};

		var wxit = function (s, fn) {
			xit(s, function () {
				fn(makeTester());
			});
		};

		if (!withABagOfCandy) {
			tests(
				wit,
				wfit,
				wxit)
		} else {
			var legacyPolyfill = makeLegacyPolyfill();
			tests(
				wit,
				wfit,
				wxit,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill);
		}

	};

	var agentsSetup = function (tests) {

		var state = {
			stateParams: {}
		};

		beforeEach(module('wfm.rta'));
		beforeEach(module('wfm.rtaTestShared'));

		beforeEach(function () {
			module(function ($provide) {
				$provide.factory('$stateParams', function () {
					state.stateParams = {};
					return state.stateParams;
				});
			});
		});

		beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_, _NoticeService_) {
			state.$interval = _$interval_;
			state.$state = _$state_;
			state.$sessionStorage = _$sessionStorage_;
			state.$httpBackend = _$httpBackend_;
			state.$fakeBackend = _FakeRtaBackend_;
			state.$controllerBuilder = _ControllerBuilder_;
			state.NoticeService = _NoticeService_;
			state.scope = state.$controllerBuilder.setup('RtaAgentsController46786');
			spyOn(state.$state, 'go');
		}));

		afterEach(function () {
			state.$fakeBackend.clear();
			state.$sessionStorage.$reset();
		});

		injectTester(state, tests, true);

	};

	var historicalSetup = function (tests) {

		var state = {
			stateParams: {}
		};

		beforeEach(module('wfm.rta'));
		beforeEach(module('wfm.rtaTestShared'));

		beforeEach(function () {
			module(function ($provide) {
				$provide.factory('$stateParams', function () {
					state.stateParams = {};
					return state.stateParams;
				});
			});
		});

		beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _FakeRtaBackend_, _ControllerBuilder_, _$translate_) {
			state.$interval = _$interval_;
			state.$state = _$state_;
			state.$httpBackend = _$httpBackend_;
			state.$fakeBackend = _FakeRtaBackend_;
			state.$controllerBuilder = _ControllerBuilder_;
			state.$translate = _$translate_;
			state.$state.current.name = 'rta-historical';
			state.$controllerBuilder.setup('RtaHistoricalController46826');
		}));

		afterEach(function () {
			if (state.$fakeBackend)
				state.$fakeBackend.clear();
		});

		injectTester(state, tests);
	};

	return {
		describe: function (description, tests) {
			return describe(description, function () {
				if (description === 'RtaAgentsController')
					agentsSetup(tests);
				if (description === 'RtaHistoricalController')
					historicalSetup(tests);
			});
		},
		fdescribe: function (description, tests) {
			return fdescribe(description, function () {
				if (description === 'RtaAgentsController')
					agentsSetup(tests);
				if (description === 'RtaHistoricalController')
					historicalSetup(tests);
			});
		}
	}

})();

