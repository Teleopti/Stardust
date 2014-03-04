require.config(requireconfiguration);
		'knockout': ['jquery'],


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
