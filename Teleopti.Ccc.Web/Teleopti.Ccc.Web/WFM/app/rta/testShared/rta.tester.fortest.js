'use strict';

var rtaTester = (function () {

	var injectTester = function (sharedTestState, tests, withABagOfCandy) {

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
				}
			};
		};

		var makeTester = function () {
			var controllerTester;
			return {
				createController: function () {
					controllerTester = sharedTestState.$controllerBuilder.createController();
					return controllerTester.controller;
				},
				destroyController: function () {
					// simulate destroy atleast...
					sharedTestState.scope.$emit('$destroy');
				},
				get stateParams() {
					return sharedTestState.stateParams;
				},
				get lastGoParams() {
					return sharedTestState.lastGoParams;
				},
				get backend() {
					return sharedTestState.$fakeBackend;
				},
				get sessionStorage() {
					return sharedTestState.$sessionStorage;
				},
				get lastNotice() {
					return sharedTestState.lastNotice;
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
				},
				randomString: function (prefix) {
					return (prefix || '') + Math.random().toString(36).substring(8);
				}
			}
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

	var setup = function (tests, stateName, controllerName) {

		var state = {
			stateParams: {}
		};

		beforeEach(module('wfm.rta'));
		beforeEach(module('wfm.rtaTracer'));
		beforeEach(module('wfm.rtaTestShared'));

		beforeEach(function () {
			module(function ($provide) {
				$provide.factory('$stateParams', function () {
					state.stateParams = {};
					return state.stateParams;
				});
				$provide.factory('skills', function () {
					return state.$fakeBackend.skills;
				});
				$provide.factory('skillAreas', function () {
					return state.$fakeBackend.skillAreas;
				});
			});
		});

		beforeEach(inject(
			['$httpBackend', '$interval', '$state', '$sessionStorage', 'FakeRtaBackend', 'ControllerBuilder', 'NoticeService', '$translate',
				function ($httpBackend, $interval, $state, $sessionStorage, $fakeBackend, $controllerBuilder, NoticeService, $translate) {
					state.$interval = $interval;
					state.$state = $state;
					state.$sessionStorage = $sessionStorage;
					state.$httpBackend = $httpBackend;
					state.$fakeBackend = $fakeBackend;
					state.$controllerBuilder = $controllerBuilder;
					state.NoticeService = NoticeService;
					state.$state.current.name = stateName;
					state.scope = state.$controllerBuilder.setup(controllerName);

					spyOn($state, 'go').and.callFake(function (_, params) {
						state.lastGoParams = params;
					});

					spyOn(NoticeService, 'warning').and.callFake(function (message, lifetime, destroyOnStateGo) {
						state.lastNotice = {
							Warning: message,
							Lifetime: lifetime,
							DestroyOnStateGo: destroyOnStateGo
						};
					});

					spyOn($translate, 'instant').and.callFake(function (key) {
						return state.$fakeBackend.data.translation()[key] || key;
					});

				}]
		));

		afterEach(function () {
			if (state.$fakeBackend)
				state.$fakeBackend.clear.all();
			if (state.$sessionStorage)
				state.$sessionStorage.$reset();
			state.lastGoParams = undefined;
			state.lastNotice = undefined;
		});

		var bagOfCandy = controllerName == 'RtaAgentsController48724';
		injectTester(state, tests, bagOfCandy);
	};

	function setupByDescription(description, tests) {
		if (description === 'RtaOverviewController')
			setup(tests, '', 'RtaOverviewController46933');
		if (description === 'RtaAgentsController')
			setup(tests, '', 'RtaAgentsController48724');
		if (description === 'RtaHistoricalController')
			setup(tests, 'rta-historical', 'RtaHistoricalController47721');
		if (description === 'RtaTracerController')
			setup(tests, '', 'RtaTracerController');
	}

	return {
		describe: function (description, tests) {
			return describe(description, function () {
				setupByDescription(description, tests);
			});
		},
		fdescribe: function (description, tests) {
			return fdescribe(description, function () {
				setupByDescription(description, tests);
			});
		}
	}

})();

