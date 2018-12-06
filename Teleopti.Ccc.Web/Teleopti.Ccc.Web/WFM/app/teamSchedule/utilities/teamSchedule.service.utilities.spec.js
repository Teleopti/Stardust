(function () {
	var target;

	describe('#teamschedule UtilityService#', function () {
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
			target.setNowDate('2018-02-26 07:00:00');
		}));

		commonTestsInDifferentLocale();

		it('should get correct weekdays for the given date', function () {
			var result = target.getWeekdays(moment('2018-06-11').toDate());
			expect(result.length).toBe(7);
			expect(moment(result[0].date).locale('en').format('YYYY-MM-DD')).toBe('2018-06-11');
			expect(moment(result[6].date).locale('en').format('YYYY-MM-DD')).toBe('2018-06-17');
		});

		it('should get first day of week', function () {
			expect(target.getFirstDayOfWeek('2018-10-10')).toEqual('2018-10-08');
		});

	});

	describe('#teamschedule UtilityService in locale ar-AE#', function () {
		beforeEach(module('wfm.teamSchedule'));
		beforeEach(module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return {
					CurrentUserInfo: function () {
						return {
							DateFormatLocale: "ar-AE",
							DefaultTimeZone: 'Asia/Hong_Kong',
							FirstDayOfWeek: 6
						};
					}
				};
			});
		}));

		beforeAll(function () {
			moment.locale('ar-AE');
		});

		afterAll(function () {
			moment.locale('en');
		});

		beforeEach(inject(function (_UtilityService_) {
			target = _UtilityService_;
			target.setNowDate('2018-02-26 07:00:00');
		}));

		commonTestsInDifferentLocale();

		it('should get first day of week', function () {
			expect(target.getFirstDayOfWeek('2018-10-10')).toEqual('2018-10-06');
		});

	});

	describe('#teamschedule UtilityService in locale fa-IR#', function () {
		beforeEach(module('wfm.teamSchedule'));
		beforeEach(module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return {
					CurrentUserInfo: function () {
						return {
							DateFormatLocale: "fa-IR",
							DefaultTimeZone: 'Asia/Hong_Kong',
							FirstDayOfWeek: 1
						};
					}
				};
			});
		}));

		beforeEach(inject(function (_UtilityService_) {
			target = _UtilityService_;
			target.setNowDate('2018-02-26 07:00:00');
		}));

		beforeEach(function () {
			moment.locale('fa-IR');
		});

		afterEach(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});

	function commonTestsInDifferentLocale() {
		it('should get correct now in user time zone', function () {
			expect(target.nowInTimeZone('Asia/Hong_Kong')).toEqual('2018-02-26T15:00:00+08:00');
		});

		it('should get correct next tick no earlier than eight', function () {
			expect(target.getNextTickNoEarlierThanEight('Asia/Hong_Kong')).toEqual('2018-02-26T15:15:00+08:00');
		});

	}
	
})();
