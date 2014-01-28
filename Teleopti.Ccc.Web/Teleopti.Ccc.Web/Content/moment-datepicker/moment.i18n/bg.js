// moment.js language configuration
// language : bulgarian (bg)
// author : Krasen Borisov : https://github.com/kraz
(function () {
    var lang = {
            months : "януари_февруари_март_април_май_юни_юли_август_септември_октомври_ноември_декември".split("_"),
            monthsShort : "янр_фев_мар_апр_май_юни_юли_авг_сеп_окт_ное_дек".split("_"),
            weekdays : "неделя_понеделник_вторник_сряда_четвъртък_петък_събота".split("_"),
            weekdaysShort : "нед_пон_вто_сря_чет_пет_съб".split("_"),
            weekdaysMin : "нд_пн_вт_ср_чт_пт_сб".split("_"),
            longDateFormat : {
                LT : "h:mm",
                L : "D.MM.YYYY",
                LL : "D MMMM YYYY",
                LLL : "D MMMM YYYY LT",
                LLLL : "dddd, D MMMM YYYY LT"
            },
            calendar : {
                sameDay : '[Днес в] LT',
                nextDay : '[Утре в] LT',
                nextWeek : 'dddd [в] LT',
                lastDay : '[Вчера в] LT',
                lastWeek : function () {
                    switch (this.day()) {
                    case 0:
                    case 3:
                    case 6:
                        return '[В изминалата] dddd [в] LT';
                    case 1:
                    case 2:
                    case 4:
                    case 5:
                        return '[В изминалия] dddd [в] LT';
                    }
                },
                sameElse : 'L'
            },
            relativeTime : {
                future : "след %s",
                past : "преди %s",
                s : "няколко секунди",
                m : "минута",
                mm : "%d минути",
                h : "час",
                hh : "%d часа",
                d : "ден",
                dd : "%d дни",
                M : "месец",
                MM : "%d месеца",
                y : "година",
                yy : "%d години"
            },
            ordinal : function (number) {
                return '.';
            }
        };

    // Node
    if (typeof module !== 'undefined') {
        module.exports = lang;
    }
    // Browser
    if (typeof window !== 'undefined' && this.moment && this.moment.lang) {
        this.moment.lang('bg', lang);
    }
}());
