'use strict';
describe('ResourceplannerArchiveScheduleCtrl', function () {
	var $q,
		archiveScheduleSrvc;

	beforeEach(function () {
		module('wfm.resourceplanner');
	});

	beforeEach(inject(function (_$q_) {
		$q = _$q_;
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

		$controller('ResourceplannerArchiveScheduleCtrl', { ArchiveScheduleSrvc: archiveScheduleSrvc });
		expect($controller).not.toBe(null);
	}));
});
