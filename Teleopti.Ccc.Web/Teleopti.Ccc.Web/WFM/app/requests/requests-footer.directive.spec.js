(function () {
'use strict';

	describe('requests footer directive',
		function() {
			var $compile, $rootScope, requestsDefinitions, $filter, requestCommandParamsHolder;

			beforeEach(module('wfm.templates'));
			beforeEach(module('wfm.requests'));

			beforeEach(function () {
				var requestCommandParamsHolder = new FakeRequestCommandParamsHolder();

				module(function($provide) {
					$provide.service('Toggle',
						function() {
							return {
								Wfm_Requests_People_Search_36294: true,
								Wfm_Requests_Performance_36295: true,
								Wfm_Requests_ShiftTrade_More_Relevant_Information_38492: true,
								Wfm_Requests_Default_Status_Filter_39472: true,
								Wfm_Requests_Show_Pending_Reasons_39473: true,
								togglesLoaded: {
									then: function(cb) { cb(); }
								}
							}
						});

					$provide.service('requestCommandParamsHolder', function () {
						return requestCommandParamsHolder;
					});
				});
			});

			beforeEach(inject(function (_$compile_, _$rootScope_, _requestsDefinitions_, _$filter_, $templateCache, _requestCommandParamsHolder_) {
				$compile = _$compile_;
				$rootScope = _$rootScope_;
				requestsDefinitions = _requestsDefinitions_;
				$filter = _$filter_;
				$templateCache.put('app/requests/html/requests-footer.tpl.html', 'app/requests/');
				requestCommandParamsHolder = _requestCommandParamsHolder_;
			}));

			function setUpTarget() {
				var scope = $rootScope.$new();
				var directiveElem = getCompiledElement();

				function getCompiledElement() {
					var element = angular.element('<requests-footer' +
						' paging="paging"' +
						' page-size-options="pageSizeOptions"' +
						' is-shift-trade-view-active="true"' +
						' is-using-request-submitter-time-zone="true"' +
						' ></requests-footer>');
					var compiledElement = $compile(element)(scope);
					return compiledElement;
				};

				return { scope: scope, target: directiveElem };
			}

			it('should apply template', function () {
				var test = setUpTarget();
				test.scope.$digest();
				expect(test.target.html()).not.toEqual('');
			});

			it('should show selected request info', function() {
				var test = setUpTarget();
				requestCommandParamsHolder.setIds(['1', '2']);
				test.scope.$digest();

				test.target.isolateScope().requestsFooter.selectedRequestsInfoText = "Selected {0} of {1} requests";
				var paging = {
					pageSize: 20,
					pageNumber: 1,
					totalPages: 1,
					totalRequestsCount: 5
				};
				test.target.isolateScope().requestsFooter.paging = paging;
				test.scope.$digest();

				var ret = test.target.isolateScope().requestsFooter.showSelectedRequestsInfo();
				expect(ret).toEqual("Selected 2 of 5 requests");
			});

			it('should show none request info', function () {
				var test = setUpTarget();
				test.scope.$digest();

				var ret = test.target.isolateScope().requestsFooter.showSelectedRequestsInfo();
				expect(ret).toEqual('');
			});

			it('should set page number to 1 when page size changed', function() {
				var test = setUpTarget();
				test.scope.$digest();

				test.target.isolateScope().requestsFooter.selectedRequestsInfoText = "Selected {0} of {1} requests";
				var paging = {
					pageSize: 20,
					pageNumber: 3,
					totalPages: 4,
					totalRequestsCount: 68
				};
				test.target.isolateScope().requestsFooter.paging = paging;
				test.scope.$digest();

				var footer = test.target.isolateScope().requestsFooter;
				footer.paging.pageSize = 50;
				footer.onPageSizeChanges();
				expect(footer.paging.pageNumber).toEqual(1);
			});

			it('total pages should be changed when page size changed', function () {
				var test = setUpTarget();
				test.scope.$digest();

				var paging = {
					pageSize: 20,
					pageNumber: 3,
					totalPages: 4,
					totalRequestsCount: 68
				};
				test.target.isolateScope().requestsFooter.paging = paging;
				test.scope.$digest();

				var footer = test.target.isolateScope().requestsFooter;
				footer.paging.pageSize = 50;
				footer.onPageSizeChanges();
				expect(footer.paging.totalPages).toEqual(2);
			});

			it('page number should be changed', function () {
				var test = setUpTarget();
				test.scope.$digest();

				var paging = {
					pageSize: 20,
					pageNumber: 3,
					totalPages: 4,
					totalRequestsCount: 68
				};
				test.target.isolateScope().requestsFooter.paging = paging;
				test.scope.$digest();

				var footer = test.target.isolateScope().requestsFooter;
				footer.onPageNumberChange(2);
				expect(footer.paging.pageNumber).toEqual(2);
			});
		});

	function FakeRequestCommandParamsHolder() {
		var _selectedRequestIds = [];

		this.setIds = function(ids) {
			_selectedRequestIds = ids;
		}

		this.getSelectedRequestsIds = function(isShiftTrade) {
			return _selectedRequestIds;
		}
	}
})();