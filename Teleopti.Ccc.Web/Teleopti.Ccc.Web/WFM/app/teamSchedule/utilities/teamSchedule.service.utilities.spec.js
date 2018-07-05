describe('teamschedule utility tests', function () {

	var target, currentUserInfo;

	beforeEach(module('wfm.teamSchedule'));
	beforeEach(module(function ($provide) {
		$provide.service('CurrentUserInfo', function () {
			return {
				CurrentUserInfo: function () {
					return {
						DateFormatLocale: "sv-SE",
						DefaultTimeZone: 'Asia/Hong_Kong',
						FirstDayOfWeek: 1
					};
				}
			};
		});
	}));
	beforeAll(function () {
		moment.locale('sv');
	});
	afterAll(function () {
		moment.locale('en');
	});

	beforeEach(inject(function (_UtilityService_) {
		target = _UtilityService_;
		target.setNowDate(new Date('2018-02-26T15:00:00+08:00'));
	}));

	it('should get correct week day names', function () {
		expect(target.getWeekdayNames().length).toBe(7);
		expect(target.getWeekdayNames()[0]).toBe("måndag");
		expect(target.getWeekdayNames()[6]).toBe("söndag");
	});

	it('should get correct weekdays for the given date', function () {
		var result = target.getWeekdays(moment('2018-06-11').toDate());
		expect(result.length).toBe(7);
		expect(result[0].name).toBe('måndag');
		expect(moment(result[0].date).locale('en').format('YYYY-MM-DD')).toBe('2018-06-11');
		expect(moment(result[6].date).locale('en').format('YYYY-MM-DD')).toBe('2018-06-17');
	});

	function commonTestsInDifferentLocale() {
		it('should get correct now in user time zone', function () {
			expect(target.nowInUserTimeZone()).toEqual('2018-02-26T15:00:00+08:00');
		});

		it('should get correct next tick no earlier than eight', function () {
			expect(target.getNextTickNoEarlierThanEight('Asia/Hong_Kong')).toEqual('2018-02-26T15:15:00+08:00');
		});
	}

	commonTestsInDifferentLocale();

	describe('in locale ar-AE', function () {
		beforeAll(function () {
			moment.locale('ar-AE');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});

	describe('in locale fa-IR', function () {
		beforeEach(function () {
			moment.locale('fa-IR');
		});

		afterEach(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});
	

});