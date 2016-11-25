'use strict';
describe('ResourceplannerManageScheduleCtrl', function () {
	var $q,
		$rootScope,
		manageScheduleSrvc;

	beforeEach(function () {
		module('wfm.resourceplanner');
	});

	beforeEach(inject(function (_$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		manageScheduleSrvc = {
			scenarios: {
				query: function () {
					var queryDeferred = $q.defer();
					queryDeferred.resolve([
					{
						Id: "1",
						Name: "Default",
						DefaultScenario: true
					}]);
					return { $promise: queryDeferred.promise };
				}
			}
		}
	}));

	it('not null', inject(function ($controller) {
		var scope = $rootScope.$new();

		$controller('ResourceplannerManageScheduleCtrl', { ManageScheduleSrvc: manageScheduleSrvc, $scope: scope });
		expect($controller).not.toBe(null);
	}));
});
