describe('teamschedule utility tests', function() {
	var fakeLocale;

	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function() {
		module(function($provide) {
			$provide.service('$locale', function() {

				return {
					DATETIME_FORMATS: {
						DAY: ["Sunday", "Monday", "Tuesday", "Wendesday", "Thursday", "Friday", "Saturday"],
						FIRSTDAYOFWEEK: 4
					}
				};
			});
		});
	});

	it('should get correct week day names', inject(function(UtilityService) {
		var target = UtilityService;

		expect(target.getWeekdayNames().length).toBe(7);
		expect(target.getWeekdayNames()[0]).toBe("Friday");
		expect(target.getWeekdayNames()[6]).toBe("Thursday");
	}));
});