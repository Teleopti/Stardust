define(
    [
        'jquery',
        'errorview'
    ], function(
        $,
        errorview
    ) {

    	

        return {
        	ajax: function (options) {

        		options.statusCode401 = options.statusCode401 || function () { window.location.href = ""; };

                options.cache = false;
                options.dataType = options.dataType || "json";
                options.contentType = options.contentType || "application/json";
                options.error = options.error || function (jqXHR, textStatus, errorThrown) {

                	if (options.statusCode401 && jqXHR && jqXHR.status == 401) {
                		options.statusCode401(jqXHR, textStatus, errorThrown);
                		return;
                	}

                	if (options.statusCode500 && jqXHR && jqXHR.status == 500) {
                		options.statusCode500(jqXHR, textStatus, errorThrown);
                		return;
                	}

                	if (options.statusCode501 && jqXHR && jqXHR.status == 501) {
                		options.statusCode501(jqXHR, textStatus, errorThrown);
                		return;
                	}

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
