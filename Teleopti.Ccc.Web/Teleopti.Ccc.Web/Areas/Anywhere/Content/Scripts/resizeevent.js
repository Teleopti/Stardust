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
                    .off('resize')
                    .off('orientationchange')
                    .off('ready')
                    .on('resize', callback)
                    .on('orientationchange', callback)
                    .on('ready', callback)
                    ;
            },
            
            notify: function() {
                callback();
            }
        };

    });
