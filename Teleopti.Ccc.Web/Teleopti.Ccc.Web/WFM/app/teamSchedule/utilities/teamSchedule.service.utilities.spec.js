describe('teamschedule utility tests', function () {

	var target, currentUserInfo, realNow;

	beforeEach(module('wfm.teamSchedule'));
	beforeEach(module(function ($provide) {
		currentUserInfo = new FakeCurrentUserInfo();
		$provide.service('CurrentUserInfo', function () {
			return currentUserInfo;
		});
	}));
	beforeAll(function () {
		moment.locale('sv');
	});
	afterAll(function () {
		moment.locale('en');
	});

	beforeEach(inject(function (_UtilityService_) {
		realNow = _UtilityService_.now;
		target = _UtilityService_;
		target.now = function () {
			return new Date('2018-02-26 15:00:00');
		}
	}));

	afterEach(function () {
		target.now = realNow;
	});

	it('should get correct week day names', function () {

		expect(target.getWeekdayNames().length).toBe(7);
		expect(target.getWeekdayNames()[0]).toBe("måndag");
		expect(target.getWeekdayNames()[6]).toBe("söndag");
	});

	it('should get correct weekdays for the given date', function () {
		var result = target.getWeekdays(moment('2016-08-17').toDate());
		expect(result.length).toBe(7);
		expect(result[0].name).toBe('måndag');
		expect(moment(result[0].date).locale('en').format('YYYY-MM-DD')).toBe('2016-08-15');
	});

	function commonTestsInDifferentLocale() {
		it('should get correct now in user time zone', function () {
			expect(target.nowInUserTimeZone()).toEqual('2018-02-26T07:00:00+00:00');
		});

		it('should get correct next tick', function () {
			expect(target.getNextTick()).toEqual('2018-02-26T15:00:00+08:00');
		});

		it('should get correct next tick no earlier than eight', function () {
			expect(target.getNextTickNoEarlierThanEight()).toEqual('2018-02-26T15:00:00+08:00');
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
	function FakeCurrentUserInfo() {
		this.CurrentUserInfo = function () {
			return {
				DateFormatLocale: "en-US",
				DefaultTimeZone: 'Asia/HongKong'
			};
		};
	}

});