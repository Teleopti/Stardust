// moment.js language configuration
// language : icelandic (is)
// author : Hinrik Örn Sigurðsson : https://github.com/hinrik
(function () {
    var plural = function (n) {
        if (n % 100 == 11) {
            return true;
        } else if (n % 10 == 1) {
            return false;
        } else {
            return true;
        }
    },

    translate = function (number, withoutSuffix, key, isFuture) {
        var result = number + " ";
        switch (key) {
        case 's':
            return withoutSuffix || isFuture ? 'nokkrar sekúndur' : 'nokkrum sekúndum';
        case 'm':
            return withoutSuffix ? 'mínúta' : 'mínútu';
        case 'mm':
            if (plural(number)) {
                return result + (withoutSuffix || isFuture ? 'mínútur' : 'mínútum');
            } else if (withoutSuffix) {
                return result + 'mínúta';
            } else {
                return result + 'mínútu';
            }
        case 'hh':
            if (plural(number)) {
                return result + (withoutSuffix || isFuture ? 'klukkustundir' : 'klukkustundum');
            } else {
                return result + 'klukkustund';
            }
        case 'd':
            if (withoutSuffix) {
                return 'dagur'
            } else {
                return isFuture ? 'dag' : 'degi';
            }
        case 'dd':
            if (plural(number)) {
                if (withoutSuffix) {
                    return result + 'dagar'
                } else {
                    return result + (isFuture ? 'daga' : 'dögum');
                }
            } else if (withoutSuffix) {
                return result + 'dagur'
            } else {
                return result + (isFuture ? 'dag' : 'degi');
            }
        case 'M':
            if (withoutSuffix) {
                return 'mánuður'
            } else {
                return isFuture ? 'mánuð' : 'mánuði';
            }
        case 'MM':
            if (plural(number)) {
                if (withoutSuffix) {
                    return result + 'mánuðir'
                } else {
                    return result + (isFuture ? 'mánuði' : 'mánuðum');
                }
            } else if (withoutSuffix) {
                return result + 'mánuður';
            } else {
                return result + (isFuture ? 'mánuð' : 'mánuði');
            }
        case 'y':
            return withoutSuffix || isFuture ? 'ár' : 'ári';
        case 'yy':
            if (plural(number)) {
                return result + (withoutSuffix || isFuture ? 'ár' : 'árum');
            } else {
                return result + (withoutSuffix || isFuture ? 'ár' : 'ári');
            }
        }
    },

    lang = {
            months : "janúar_febrúar_mars_apríl_maí_júní_júlí_ágúst_september_október_nóvember_desember".split("_"),
            monthsShort : "jan_feb_mar_apr_maí_jún_júl_ágú_sep_okt_nóv_des".split("_"),
            weekdays : "sunnudagur_mánudagur_þriðjudagur_miðvikudagur_fimmtudagur_föstudagur_laugardagur".split("_"),
            weekdaysShort : "sun_mán_þri_mið_fim_fös_lau".split("_"),
            weekdaysMin : "Su_Má_Þr_Mi_Fi_Fö_La".split("_"),
            longDateFormat : {
                LT : "H:mm",
                L : "DD/MM/YYYY",
                LL : "D. MMMM YYYY",
                LLL : "D. MMMM YYYY kl. LT",
                LLLL : "dddd, D. MMMM YYYY kl. LT"
            },
            calendar : {
                sameDay : '[í dag kl.] LT',
                nextDay : '[á morgun kl.] LT',
                nextWeek : 'dddd [kl.] LT',
                lastDay : '[í gær kl.] LT',
                lastWeek : '[síðasta] dddd [kl.] LT',
                sameElse : 'L'
            },
            relativeTime : {
                future : "eftir %s",
                past : "fyrir %s síðan",
                s : translate,
                m : translate,
                mm : translate,
                h : "klukkustund",
                hh : translate,
                d : translate,
                dd : translate,
                M : translate,
                MM : translate,
                y : translate,
                yy : translate,
            },
            ordinal : function (number) {
                return '.';
            }
        };

    // Node
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = lang;
    }
    // Browser
    if (typeof window !== 'undefined' && this.moment && this.moment.lang) {
        this.moment.lang('is', lang);
    }
}());
