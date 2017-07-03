'use strict';
(function() {
	angular
		.module('wfm.rta')
		.factory('ControllerBuilder', controllerBuilder);

	controllerBuilder.$inject = ['$controller', '$interval', '$httpBackend', '$rootScope', '$log'];

	function controllerBuilder($controller, $interval, $httpBackend, $rootScope, $log) {
		var controllerName = "hejsan";
		var scope;

		var service = {
			setup: setup,
			createController: createController
		};

		return service;
		/////////////////////

		function setup(name) {
			controllerName = name;
			scope = $rootScope.$new();
			return scope;
		};

		function createController(skills, skillAreas) {

			var vm = $controller(controllerName, {
				$scope: scope,
				skills: skills,
				skillAreas: skillAreas
			});

			scope.$digest();
			safeBackendFlush();

			var callbacks = {
				apply: function(apply) {
					if (angular.isFunction(apply)) {
						apply();
						scope.$digest();
					} else {
						scope.$apply(apply);
					}
					safeBackendFlush();
					return callbacks;
				},

				wait: function(milliseconds) {
					$interval.flush(milliseconds);
					safeBackendFlush();
					return callbacks;
				},
				vm: vm
			};

			return callbacks;

			function safeBackendFlush() {
				try { // the internal mock will throw if no requests were made
					$httpBackend.flush();
				} catch (e) {
					if (e.message.includes("No pending request to flush !"))
						return;
					$log.error(e.message);
				}
			};
		}

	};
})();
