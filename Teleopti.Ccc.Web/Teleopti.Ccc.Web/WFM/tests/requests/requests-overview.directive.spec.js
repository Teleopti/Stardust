(function() {
	'use strict';

	describe('Requests overview directive', function() {

		var $compile, $rootScope, requestsData, requestsDefinitions;

		var targetElement, targetScope;
	
		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.requests'));

		beforeEach(function() {
			var requestsDataService = new FakeRequestsDataService();
			module(function($provide) {
				$provide.service('requestsData', function() {
					return requestsDataService;
				});
			});
		});

		beforeEach(inject(function(_$compile_, _$rootScope_, _requestsData_, _requestsDefinitions_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			requestsData = _requestsData_;
			requestsDefinitions = _requestsDefinitions_;
			targetScope = $rootScope.$new();
			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
			targetScope.$digest();
		}));


		it("Show requests table container", function() {
			var targets = targetElement.find('requests-table-container');
			expect(targets.length).toEqual(1);
		});

		it("Populate requests data from requests data service", function() {

			var request = {
				Id: 1,
				Type: requestsDefinitions.REQUEST_TYPES.TEXT
			};
		
			requestsData.setRequests([request]);
			
			var targets = targetElement.find('requests-table-container');
			var scope = angular.element(targets[0]).scope();
			
			scope.requestsOverview.init();		

			expect(scope.requestsOverview.requests.length).toEqual(1);
			expect(scope.requestsOverview.requests[0]).toEqual(request);
		});

		// ToDo: didn't test correct data-binding for the child directive
	});

	describe('requests table container directive', function() {

		var $compile, $rootScope, requestsDefinitions;

		var targetElement, targetScope;
		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.requests'));
		

		beforeEach(inject(function (_$compile_, _$rootScope_, _requestsDefinitions_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			requestsDefinitions = _requestsDefinitions_;
			targetScope = $rootScope.$new();

			targetScope.requests = [{ Id: 1 }, { Id: 2 }];
			targetElement = $compile('<requests-table-container requests="requests"></requests-table-container>')(targetScope);
			targetScope.$digest();
		}));

		it('Should apply template', function() {
			expect(targetElement.html()).not.toEqual('');
		});

		it('See UI Grid', function() {
			var targets = Array.from(targetElement.children());
			expect(targets.some(function (target) { return angular.element(target).hasClass('ui-grid'); })).toBeTruthy();

		});

		it("See table rows for each request", function () {
			var targets = targetElement[0].querySelectorAll('.ui-grid-row');
			expect(targets.length).toEqual(2);
		});

	});


	function FakeRequestsDataService() {

		var _requests;

		this.setRequests = function(requests) {
			_requests = requests;
		};

		this.getAllRequestsPromise = function() {
			return {
				then: function (cb) {					
					cb(_requests);
				}
			}
		};
	}


})();