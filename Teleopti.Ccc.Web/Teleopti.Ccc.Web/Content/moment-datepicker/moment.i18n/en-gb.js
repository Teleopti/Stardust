// moment.js language configuration
// language : great britain english (en-gb)
// author : Chris Gedrim : https://github.com/chrisgedrim
(function () {
    var lang = {
            months : "January_February_March_April_May_June_July_August_September_October_November_December".split("_"),
            monthsShort : "Jan_Feb_Mar_Apr_May_Jun_Jul_Aug_Sep_Oct_Nov_Dec".split("_"),
            weekdays : "Sunday_Monday_Tuesday_Wednesday_Thursday_Friday_Saturday".split("_"),
            weekdaysShort : "Sun_Mon_Tue_Wed_Thu_Fri_Sat".split("_"),
            weekdaysMin : "Su_Mo_Tu_We_Th_Fr_Sa".split("_"),
            longDateFormat : {
                LT : "h:mm A",
                L : "DD/MM/YYYY",
                LL : "D MMMM YYYY",
                LLL : "D MMMM YYYY LT",
                LLLL : "dddd, D MMMM YYYY LT"
            },
            calendar : {
                sameDay : '[Today at] LT',
                nextDay : '[Tomorrow at] LT',
                nextWeek : 'dddd [at] LT', 
                lastDay : '[Yesterday at] LT',
                lastWeek : '[last] dddd [at] LT', 
                sameElse : 'L'
            },
            relativeTime : {
                future : "in %s",
                past : "%s ago",
                s : "a few seconds",
                m : "a minute",
                mm : "%d minutes",
                h : "an hour",
                hh : "%d hours",
                d : "a day",
                dd : "%d days",
                M : "a month",
                MM : "%d months",
                y : "a year",
                yy : "%d years"
            },
            ordinal : function (number) {
                var b = number % 10;
                return (~~ (number % 100 / 10) === 1) ? 'th' :
                    (b === 1) ? 'st' :
                    (b === 2) ? 'nd' :
                    (b === 3) ? 'rd' : 'th';
            }
        };

    // Node
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = lang;
    }
    // Browser
    if (typeof window !== 'undefined' && this.moment && this.moment.lang) {
        this.moment.lang('en-gb', lang);
    }
}());
