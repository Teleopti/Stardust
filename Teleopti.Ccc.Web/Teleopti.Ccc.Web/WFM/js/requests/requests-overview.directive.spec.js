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
				$provide.service('requestsDataService', function() {
					return requestsDataService;
				});
			});
		});

		beforeEach(inject(function(_$compile_, _$rootScope_, _requestsDataService_, _requestsDefinitions_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			requestsDataService = _requestsDataService_;
			requestsDefinitions = _requestsDefinitions_;
			targetScope = $rootScope.$new();
		}));


		it("show requests table container", function() {
			requestsDataService.setRequests([]);
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

			requestsDataService.setRequests([request]);
			targetElement = $compile('<requests-overview></requests-overview>')(targetScope);
			targetScope.$digest();
			var scope = getInnerScope(targetElement);
			expect(scope.requestsOverview.requests.length).toEqual(1);
			expect(scope.requestsOverview.requests[0]).toEqual(request);
		});

		it("should not request data when filter contains error", function() {

			requestsDataService.setRequests([]);

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

			requestsDataService.setRequests([]);
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

		it("should request data when search term changed", function() {

			requestsDataService.setRequests([]);
			targetScope.period = {};
			targetScope.agentSearchTerm = "";

			targetElement = $compile('<requests-overview period="period" agent-search-term="agentSearchTerm"></requests-overview>')(targetScope);

			targetScope.$digest();

			requestsDataService.reset();

			targetScope.agentSearchTerm = "search term";
			targetScope.$digest();
			expect(requestsDataService.getHasSentRequests()).toBeTruthy();
			expect(requestsDataService.getLastRequestParameters()[0].agentSearchTerm).toEqual("search term");
		});

		it("should request data when pagination changed", function() {		

			requestsDataService.setRequests([{ Id: 1 }, { Id: 2 }]);
			targetScope.period = {};
			targetScope.paging = {
				pageSize: 1,
				pageNumber: 1,
				totalPages: 1,
				totalRequestsCount: 2
			};
			targetScope.isPaginationEnabled = true;


			targetElement = $compile('<requests-overview period="period"  paging="paging" toggle-pagination-enabled="isPaginationEnabled"></requests-overview>')(targetScope);

			targetScope.$digest();

			requestsDataService.reset();

			targetScope.paging.pageNumber = 2;

			targetScope.$broadcast('reload.requests.with.selection');

			targetScope.$digest();
			expect(requestsDataService.getHasSentRequests()).toBeTruthy();
			expect(requestsDataService.getLastRequestParameters()[2].pageNumber).toEqual(2);
		});


		function getInnerScope(element) {
			var targets = element.find('requests-table-container');
			return angular.element(targets[0]).scope();
		}

	});

	describe('requests table container directive', function() {

		var $compile, $rootScope, requestsDefinitions;

		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.requests'));


		beforeEach(inject(function(_$compile_, _$rootScope_, _requestsDefinitions_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			requestsDefinitions = _requestsDefinitions_;
		}));

		it('should apply template', function() {
			var test = setUpTarget();
			expect(test.target.html()).not.toEqual('');
		});

		it('see UI Grid', function () {
			var test = setUpTarget();
			var targets = Array.from(test.target.children());
			expect(targets.some(function (target) { return angular.element(target).hasClass('ui-grid'); })).toBeTruthy();

		});

		it("see table rows for each request", function () {
			var test = setUpTarget();
			test.scope.requests = [{ Id: 1 }, { Id: 2 }];
			test.scope.$digest();
			var targets = test.target[0].querySelectorAll('.ui-grid-render-container-body .ui-grid-row');
			expect(targets.length).toEqual(2);
		});


		function setUpTarget() {
			var scope = $rootScope.$new();

			var directiveElem = getCompiledElement();

			function getCompiledElement() {
				var element = angular.element('<requests-table-container requests="requests"></requests-table-container>');
				var compiledElement = $compile(element)(scope);
				scope.$digest();
				return compiledElement;
			};

			return {  scope: scope, target: directiveElem };
		}

	});


	function FakeRequestsDataService() {

		var _requests;
		var _hasSentRequests;
		var _lastRequestParameters;

		this.reset = function () {
			_requests = [];
			_hasSentRequests = false;
			_lastRequestParameters = null;
		};

		this.setRequests = function(requests) {
			_requests = requests;
		};

		this.getHasSentRequests = function () { return _hasSentRequests; }
		this.getLastRequestParameters = function() { return _lastRequestParameters; }

		this.getAllRequestsPromise_old = function () {
			_hasSentRequests = true;
			_lastRequestParameters = arguments;
			return {
				then: function (cb) {					
					cb({ data: _requests });
				}
			}
		};

		this.getAllRequestsPromise = function() {
			_hasSentRequests = true;
			_lastRequestParameters = arguments;
			return {
				then: function(cb) {
					cb({
						data: {
							Requests: _requests,
							TotalCount: _requests.length
						}
					});
				}
			}
		}
	}


})();