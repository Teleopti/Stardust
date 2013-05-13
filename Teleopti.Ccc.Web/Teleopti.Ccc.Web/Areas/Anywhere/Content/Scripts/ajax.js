define(
    [
        'jquery'
    ], function(
        $
    ) {

        return {
            ajax: function(options) {
                options.cache = false;
                options.dataType = options.dataType || "json";
                options.contentType = options.contentType || "application/json";
                return $.ajax(options);
            }
        };

    });
