(function() {
	'use strict';

	describe('[Requests Budget Allowance Directive Controller Tests]', function() {
		var $rootScope, $controller, $translate, $filter;

		var requestsDataService = new fakeRequestsDataService();

		beforeEach(module('wfm.requests'));
		beforeEach(inject(function(_$rootScope_, _$controller_, _$translate_, _$filter_) {
			$rootScope = _$rootScope_;
			$controller = _$controller_;
			$translate = _$translate_;
			$filter = _$filter_;
		}));

		function setUpTarget() {
			return $controller('requestsBudgetAllowanceController', { $translate, $filter, requestsDataService });
		}

		it('show convert allowance to model correctly', function() {
			var target = setUpTarget();
			target.loadBudgetAllowance();

			expect(target.budgetAllowanceList.length).toEqual(3);
			var allowance1 = target.budgetAllowanceList[0];
			expect(allowance1.date.format('YYYY-MM-DD')).toEqual('2016-12-30');
			expect(allowance1.fullAllowance).toEqual('0.00');
			expect(allowance1.shrinkedAllowance).toEqual('0.00');
			expect(allowance1.usedTotal).toEqual('1.00');
			expect(allowance1.absoluteDifference).toEqual('-1.00');
			expect(allowance1.relativeDifference).toEqual('∞');
			expect(allowance1.totalHeadCounts).toEqual('1.00');
			expect(allowance1.isWeekend).toEqual(false);
			expect(allowance1.Illness).toEqual('0.00');
			expect(allowance1.Holiday).toEqual('1.00');
			expect(allowance1.AWOL).toEqual('0.00');

			var allowance2 = target.budgetAllowanceList[1];
			expect(allowance2.date.format('YYYY-MM-DD')).toEqual('2016-12-31');
			expect(allowance2.fullAllowance).toEqual('2.00');
			expect(allowance2.shrinkedAllowance).toEqual('1.00');
			expect(allowance2.usedTotal).toEqual('0.00');
			expect(allowance2.absoluteDifference).toEqual('0.00');
			expect(allowance2.relativeDifference).toEqual('-');
			expect(allowance2.totalHeadCounts).toEqual('0.00');
			expect(allowance2.isWeekend).toEqual(true);
			expect(allowance2.Illness).toEqual('0.00');
			expect(allowance2.Holiday).toEqual('0.00');
			expect(allowance2.AWOL).toEqual('0.00');

			var allowance3 = target.budgetAllowanceList[2];
			expect(allowance3.date.format('YYYY-MM-DD')).toEqual('2017-01-01');
			expect(allowance3.fullAllowance).toEqual('4.00');
			expect(allowance3.shrinkedAllowance).toEqual('2.00');
			expect(allowance3.usedTotal).toEqual('1.00');
			expect(allowance3.absoluteDifference).toEqual('2.00');
			expect(allowance3.relativeDifference).toEqual('33.33%');
			expect(allowance3.totalHeadCounts).toEqual('1.00');
			expect(allowance3.isWeekend).toEqual(true);
			expect(allowance3.Illness).toEqual('0.00');
			expect(allowance3.Holiday).toEqual('1.00');
			expect(allowance3.AWOL).toEqual('0.00');
		});
	});

	function fakeRequestsDataService() {
		this.getBudgetGroupsPromise = function() {
			return {
				then: function(callback) {
					callback({
						data: [
							{
								Id: '1',
								Name: 'Budget Group 1'
							},
							{
								Id: '2',
								Name: 'Budget Group 2'
							}
						]
					});
				}
			};
		};

		this.getBudgetAllowancePromise = function(date, budgetGroupId) {
			return {
				then: function(callback) {
					callback({
						data: [
							{
								FullAllowance: 0,
								ShrinkedAllowance: 0,
								UsedAbsencesDictionary: {
									Illness: 0,
									Holiday: 1,
									AWOL: 0
								},
								UsedTotalAbsences: 1,
								AbsoluteDifference: -1,
								RelativeDifference: 'Infinity',
								Date: '2016-12-30T00:00:00',
								TotalHeadCounts: 1,
								IsWeekend: false
							},
							{
								FullAllowance: 2,
								ShrinkedAllowance: 1,
								UsedAbsencesDictionary: {
									Illness: 0,
									Holiday: 0,
									AWOL: 0
								},
								UsedTotalAbsences: 0,
								AbsoluteDifference: 0,
								RelativeDifference: null,
								Date: '2016-12-31T00:00:00',
								TotalHeadCounts: 0,
								IsWeekend: true
							},
							{
								FullAllowance: 4,
								ShrinkedAllowance: 2,
								UsedAbsencesDictionary: {
									Illness: 0,
									Holiday: 1,
									AWOL: 0
								},
								UsedTotalAbsences: 1,
								AbsoluteDifference: 2,
								RelativeDifference: 1 / 3,
								Date: '2017-01-01T00:00:00',
								TotalHeadCounts: 1,
								IsWeekend: true
							}
						]
					});
				}
			};
		};
	}
})();
