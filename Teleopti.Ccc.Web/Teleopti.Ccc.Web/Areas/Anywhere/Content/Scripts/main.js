require.config(requireconfiguration);

require([
        'jquery',
        'modernizr',
        'respond',
        'bootstrap',
        'layout'
    ], function() {

    });

window.test = {
    callViewMethodWhenReady: function (viewName, method) {
        
        var args = Array.prototype.slice.call(arguments, 2);
        
        require(['views/' + viewName + '/view'], function (view) {
            
            var callMethodIfReady = function () {
                if (view.ready)
                    view[method].apply(view, args);
                else
                    setTimeout(callMethodIfReady, 20);
            };
            setTimeout(callMethodIfReady, 0);

        });
    }
};


// copied from sinon.js
// https://github.com/cjohansen/Sinon.JS/blob/master/lib/sinon/util/fake_timers.js

window.fakeTime = function (fyear, fmonth, fdate, fhour, fminute, fsecond)
{
	function mirrorDateProperties(target, source) {
		if (source.now) {
			target.now = function now() {
				return target.clock.now;
			};
		} else {
			delete target.now;
		}

		if (source.toSource) {
			target.toSource = function toSource() {
				return source.toSource();
			};
		} else {
			delete target.toSource;
		}

		target.toString = function toString() {
			return source.toString();
		};

		target.prototype = source.prototype;
		target.parse = source.parse;
		target.UTC = source.UTC;
		target.prototype.toUTCString = source.prototype.toUTCString;

		for (var prop in source) {
			if (source.hasOwnProperty(prop)) {
				target[prop] = source[prop];
			}
		}

		return target;
	}

	Date = (function(year, month, date, hour, minute, second) {
		var NativeDate = Date;

		function ClockDate(year, month, date, hour, minute, second, ms) {
			// Defensive and verbose to avoid potential harm in passing
			// explicit undefined when user does not pass argument
			switch (arguments.length) {
			case 0:
				return new NativeDate(fyear, fmonth, fdate, fhour, fminute, fsecond);
			case 1:
				return new NativeDate(year);
			case 2:
				return new NativeDate(year, month);
			case 3:
				return new NativeDate(year, month, date);
			case 4:
				return new NativeDate(year, month, date, hour);
			case 5:
				return new NativeDate(year, month, date, hour, minute);
			case 6:
				return new NativeDate(year, month, date, hour, minute, second);
			default:
				return new NativeDate(year, month, date, hour, minute, second, ms);
			}
		}

		return mirrorDateProperties(ClockDate, NativeDate);
	}());

};

