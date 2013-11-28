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
                options.error = options.error || function (jqXHR, textStatus, errorThrown) {
                    var message = {
                        title: "Ajax error!",
                        message: {
                            status: jqXHR.status || "",
                            textStatus: textStatus || "",
                            errorThrown: errorThrown || "",
                        }
                    };
                    errorview.display(message);
                };
                return $.ajax(options);
            }
        };

    });
