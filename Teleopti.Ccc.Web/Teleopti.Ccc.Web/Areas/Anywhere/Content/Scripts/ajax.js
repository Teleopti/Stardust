define(
    [
        'jquery',
        'errorview'
    ], function(
        $,
        errorview
    ) {

        return {
            ajax: function(options) {
                options.cache = false;
                options.dataType = options.dataType || "json";
                options.contentType = options.contentType || "application/json";
                options.error = function(jqXHR, textStatus, errorThrown) {
                    errorview.display(errorThrown);
                };
                return $.ajax(options);
            }
        };

    });
