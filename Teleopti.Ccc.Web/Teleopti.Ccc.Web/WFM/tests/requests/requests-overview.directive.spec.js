(function() {
	'use strict';

	describe('Requests overview directive', function() {

		var $compile, $rootScope, requestsDataService, requestsDefinitions;

		var targetElement, targetScope;
	
		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.requests'));

		beforeEach(function() {
			var requestsDataService = new FakeRequestsDataService();
			module(function($provide) {
				$provide.service('requestsDataService', function () {
					return requestsDataService;
				});
			});
		});

		beforeEach(inject(function (_$compile_, _$rootScope_, _requestsDataService_, _requestsDefinitions_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			requestsDataService = _requestsDataService_;
			requestsDefinitions = _requestsDefinitions_;
			targetScope = $rootScope.$new();
		}));


		it("show requests table container", function () {
			requestsDataService.setRequests({ data: [] });
			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
			targetScope.$digest();
			var targets = targetElement.find('requests-table-container');
			expect(targets.length).toEqual(1);
		});

		it("populate requests data from requests data service", function() {

			var request = {
				Id: 1,
				Type: requestsDefinitions.REQUEST_TYPES.TEXT
			};
		
			requestsDataService.setRequests({ data: [request] });
			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
			targetScope.$digest();
			var scope = getInnerScope(targetElement);		
			expect(scope.requestsOverview.requests.length).toEqual(1);
			expect(scope.requestsOverview.requests[0]).toEqual(request);
		});		

		it("should not request data when filter contains error", function() {

			requestsDataService.setRequests({ data: [] });

			targetScope.period = {};
			targetElement = $compile('<requests-overview period="period"></requests-overview>')(targetScope);			
			targetScope.$digest();			

			requestsDataService.reset();
			targetScope.period = {
				startDate: moment().add(1, 'day').toDate(),
				endDate: new Date()
			};
		
			targetScope.$digest();			
			expect(requestsDataService.getHasSentRequests()).toBeFalsy();
		});

		it("should request data when filter change to valid values", function() {

			requestsDataService.setRequests({ data: [] });
			targetScope.period = {};
			targetElement = $compile('<requests-overview period="period"></requests-overview>')(targetScope);

			targetScope.$digest();			

			requestsDataService.reset();

			targetScope.period = {
				startDate: new Date(),
				endDate: moment().add(2, 'day').toDate()
			}

			targetScope.$digest();
			expect(requestsDataService.getHasSentRequests()).toBeTruthy();
		});



		function getInnerScope(element) {
			var targets = element.find('requests-table-container');
			return angular.element(targets[0]).scope();
		}

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

		it('should apply template', function() {
			expect(targetElement.html()).not.toEqual('');
		});

		it('see UI Grid', function() {
			var targets = Array.from(targetElement.children());
			expect(targets.some(function (target) { return angular.element(target).hasClass('ui-grid'); })).toBeTruthy();

		});

		it("see table rows for each request", function () {
			var targets = targetElement[0].querySelectorAll('.ui-grid-row');
			expect(targets.length).toEqual(2);
		});

	});


	function FakeRequestsDataService() {

		var _requests;
		var _hasSentRequests;

		this.reset = function () {
			_requests = [];
			_hasSentRequests = false;
		};

		this.setRequests = function(requests) {
			_requests = requests;
		};

		this.getHasSentRequests = function () { return _hasSentRequests; }

		this.getAllRequestsPromise = function () {
			_hasSentRequests = true;
			return {
				then: function (cb) {					
					cb(_requests);
				}
			}
		};
	}


})();