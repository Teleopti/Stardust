define(
    [
        'jquery'
    ], function(
        $
    ) {

        var callback = function () { };
        
        return {
            onresize: function (func) {
                callback = func;
                $(window)
                    .resize(callback)
                    .bind('orientationchange', callback)
                    .ready(callback)
                    ;
            },
            
            notify: function() {
                callback();
            }
        };

    });
