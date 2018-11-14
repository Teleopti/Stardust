(function() {
	'use strict';

	describe('requests footer directive', function() {
		var $compile,
			$rootScope,
			requestsDefinitions,
			$filter,
			requestCommandParamsHolder,
			REQUESTS_TAB_NAMES,
			fakeState = {
				current: {
					name: ''
				}
			},
			toggles = {
				Wfm_Requests_People_Search_36294: true
			};

		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.requests'));

		beforeEach(function() {
			requestCommandParamsHolder = new FakeRequestCommandParamsHolder();

			module(function($provide) {
				$provide.service('Toggle', function() {
					toggles.togglesLoaded = {
						then: function(cb) {
							cb();
						}
					};

					return toggles;
				});

				$provide.service('$state', function() {
					return fakeState;
				});

				$provide.service('requestCommandParamsHolder', function() {
					return requestCommandParamsHolder;
				});
			});
		});

		beforeEach(inject(function(_$compile_, _$rootScope_, _requestsDefinitions_, _$filter_, _REQUESTS_TAB_NAMES_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			$filter = _$filter_;
			requestsDefinitions = _requestsDefinitions_;
			REQUESTS_TAB_NAMES = _REQUESTS_TAB_NAMES_;
		}));

		function setUpTarget(isShiftTradeViewActive, isUsingRequestSubmitterTimeZone) {
			var scope = $rootScope.$new();
			var directiveElem = getCompiledElement();

			function getCompiledElement() {
				var element = angular.element(
					'<requests-footer' +
						' paging="paging"' +
						' page-size-options="pageSizeOptions"' +
						' is-shift-trade-view-active="' +
						isShiftTradeViewActive +
						'"' +
						' is-using-request-submitter-time-zone="' +
						!!isUsingRequestSubmitterTimeZone +
						'"' +
						' ></requests-footer>'
				);
				var compiledElement = $compile(element)(scope);
				return compiledElement;
			}

			return { scope: scope, target: directiveElem };
		}

		it('should apply template', function() {
			var test = setUpTarget(true);
			test.scope.$digest();
			expect(test.target.html()).not.toEqual('');
		});

		it('should show selected request info', function() {
			var test = setUpTarget(true);
			requestCommandParamsHolder.setIds(['1', '2']);
			test.scope.$digest();

			test.target.isolateScope().requestsFooter.selectedRequestsInfoText = 'Selected {0} of {1} requests';
			var paging = {
				pageSize: 20,
				pageNumber: 1,
				totalPages: 1,
				totalRequestsCount: 5
			};
			test.target.isolateScope().requestsFooter.paging = paging;
			test.scope.$digest();

			var ret = test.target.isolateScope().requestsFooter.showSelectedRequestsInfo();
			expect(ret).toEqual('Selected 2 of 5 requests');
		});

		it('should show correct selected overtime requests info with selected requests ids number', function() {
			fakeState.current.name = REQUESTS_TAB_NAMES.overtime;

			var test = setUpTarget(false);

			requestCommandParamsHolder.setOvertimeSelectedRequestIds(['1']);
			test.scope.$digest();

			test.target.isolateScope().requestsFooter.selectedRequestsInfoText = 'Selected {0} of {1} requests';
			var paging = {
				pageSize: 20,
				pageNumber: 1,
				totalPages: 1,
				totalRequestsCount: 5
			};
			test.target.isolateScope().requestsFooter.paging = paging;
			test.scope.$digest();

			var ret = test.target.isolateScope().requestsFooter.showSelectedRequestsInfo();
			expect(ret).toEqual('Selected 1 of 5 requests');
		});

		it('should show none request info', function() {
			var test = setUpTarget(true);
			test.scope.$digest();

			var ret = test.target.isolateScope().requestsFooter.showSelectedRequestsInfo();
			expect(ret).toEqual('');
		});

		it('should show time in user timezone by default', function() {
			var test = setUpTarget(true, false);
			test.scope.$digest();

			var footer = test.target.isolateScope().requestsFooter;
			expect(footer.isUsingRequestSubmitterTimeZone).toEqual(false);

			var selectedRadioButtons = test.target[0].querySelectorAll(
				".time-zone-switch md-radio-button[aria-checked='true']"
			);
			expect(selectedRadioButtons.length).toEqual(1);
		});

		it('should set page number to 1 when page size changed', function() {
			var test = setUpTarget(true);
			test.scope.$digest();

			test.target.isolateScope().requestsFooter.selectedRequestsInfoText = 'Selected {0} of {1} requests';
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

		it('total pages should be changed when page size changed', function() {
			var test = setUpTarget(true);
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

		it('page number should be changed', function() {
			var test = setUpTarget(true);
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

		function FakeRequestCommandParamsHolder() {
			var _selectedRequestIds = [],
				_selectedOvertimeRequestIds = [];

			this.setIds = function(ids) {
				_selectedRequestIds = ids;
			};

			this.getSelectedRequestsIds = function(isShiftTrade) {
				return _selectedRequestIds;
			};

			this.setOvertimeSelectedRequestIds = function(overtimeIds) {
				_selectedOvertimeRequestIds = overtimeIds;
			};

			this.getOvertimeSelectedRequestIds = function() {
				return _selectedOvertimeRequestIds;
			};
		}
	});
})();
