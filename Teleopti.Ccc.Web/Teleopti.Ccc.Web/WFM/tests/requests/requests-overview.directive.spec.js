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
		}));


		it("Show requests table container", function () {
			requestsData.setRequests({ data: [] });
			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
			targetScope.$digest();
			var targets = targetElement.find('requests-table-container');
			expect(targets.length).toEqual(1);
		});

		it("Populate requests data from requests data service", function() {

			var request = {
				Id: 1,
				Type: requestsDefinitions.REQUEST_TYPES.TEXT
			};
		
			requestsData.setRequests({data: [request]});
			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
			targetScope.$digest();
			var scope = getInnerScope(targetElement);		
			expect(scope.requestsOverview.requests.length).toEqual(1);
			expect(scope.requestsOverview.requests[0]).toEqual(request);
		});

		// ToDo: didn't test correct data-binding for the child directive

		it("Should not request data when filter contains error", function() {

			requestsData.setRequests({ data: [] });
			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
			
			targetScope.$digest();
			var scope = getInnerScope(targetElement);

			requestsData.reset();
			scope.requestsOverview.requestsFilter = {
				period: {
					startDate: moment().add(1, 'day').toDate(),
					endDate: new Date()
				}
			};
		
			targetScope.$digest();			
			expect(requestsData.getHasSentRequests()).toBeFalsy();
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