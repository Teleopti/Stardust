describe('teamschedule utility tests', function() {

	var	target;

	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function() {
		module(function($provide) {
			$provide.service('$locale', function() {
				moment.locale('sv');
				return {
					DATETIME_FORMATS: {
						DAY: ["Sunday", "Monday", "Tuesday", "Wendesday", "Thursday", "Friday", "Saturday"],
						FIRSTDAYOFWEEK: 0
					}
				};
			});
		});
	});

	beforeEach(inject(function(_UtilityService_) {
		target = _UtilityService_;
	}));

	it('should get correct week day names', function() {
		
		expect(target.getWeekdayNames().length).toBe(7);
		expect(target.getWeekdayNames()[0]).toBe("Monday");
		expect(target.getWeekdayNames()[6]).toBe("Sunday");
	});

	it('should get correct weekdays for the given date', function() {
		var result = target.getWeekdays(moment('2016-08-17').toDate());

		expect(result.length).toBe(7);
		expect(result[0].name).toBe('Monday');
		expect(moment(result[0].date).format('YYYY-MM-DD')).toBe('2016-08-15');


	});
});