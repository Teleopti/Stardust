'use strict';
describe('ResourceplannerArchiveScheduleCtrl', function () {
	var $q,
		$rootScope,
		archiveScheduleSrvc;

	beforeEach(function () {
		module('wfm.resourceplanner');
	});

	beforeEach(inject(function (_$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		archiveScheduleSrvc = {
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

		$controller('ResourceplannerArchiveScheduleCtrl', { ArchiveScheduleSrvc: archiveScheduleSrvc, $scope: scope });
		expect($controller).not.toBe(null);
	}));
});
