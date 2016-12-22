describe('teamschedule utility tests', function() {

	var	target;

	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function () {
		moment.locale('sv');
	});

	beforeEach(inject(function(_UtilityService_) {
		target = _UtilityService_;
	}));

	it('should get correct week day names', function() {
		
		expect(target.getWeekdayNames().length).toBe(7);
		expect(target.getWeekdayNames()[0]).toBe("måndag");
		expect(target.getWeekdayNames()[6]).toBe("söndag");
	});

	it('should get correct weekdays for the given date', function() {
		var result = target.getWeekdays(moment('2016-08-17').toDate());

		expect(result.length).toBe(7);
		expect(result[0].name).toBe('måndag');
		expect(moment(result[0].date).format('YYYY-MM-DD')).toBe('2016-08-15');
	});
});