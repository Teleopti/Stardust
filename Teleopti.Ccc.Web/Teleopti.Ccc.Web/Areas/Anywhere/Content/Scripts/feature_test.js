define(
    [
		 'buster',
		 'require',
		 'feature',
		 'ajax'
    ], function (
        buster,
		require,
		feature,
		ajax
    ) {

    	return function () {

    		buster.testCase("features", {
    			"should be able to resolve with require": function () {
    				assert(feature.checkToggle);
    			},
    			"should return true if feature is enabled": function () {

				    var expectedFromToggleServiceIfFeatureIsEnabled = "True";
				    var success;

				    ajax.ajax = function (data) {
					    console.log(data);
					    success = data.success;
				    };

				    feature.checkToggle('enabledFeature');
				    assert.isTrue(success(expectedFromToggleServiceIfFeatureIsEnabled));

    			},
    			"should return false if feature is disabled": function () {

    				var expectedFromToggleServiceIfFeatureIsDisabled = "False";
    				var success;
				
    				ajax.ajax = function (data) {
    					console.log(data);
    					success = data.success;
    				};

    				feature.checkToggle('disabledFeature');
    				assert.isFalse(success(expectedFromToggleServiceIfFeatureIsDisabled));

    			},

    			"should call togglehandler specified featureName is enabled": function () {
    				var urlForAjax;

    				ajax.ajax = function (data) {
    					console.log(data);
    					urlForAjax = data.url;
    				};

    				feature.checkToggle('myFeature');
    				assert.equals(urlForAjax, 'ToggleHandler/IsEnabled?toggle=myFeature');
    			},

    		});

    	};
    });
		
