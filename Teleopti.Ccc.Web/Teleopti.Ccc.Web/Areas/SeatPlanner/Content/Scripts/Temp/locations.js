define([
],
    function (
		) {
    	return function () {
    		var that = {};
    		that.getLocations = function () {
    			return [{
    				"id": "a5bfa0d0-f045-430b-bf30-9393c3b0fee9",
    				"name": "China",
    				"childLocations": [
						{
							"id" : "f1dbfa12-2186-436b-b669-86f75a90af1e",
							"name": "Shenzhen",
							"childLocations": [
								{
									"id": "fee34359-35f5-44d8-91c6-ce57fdf44a81",
									"name": "Nan Shan",
									"seats": [
										{ "id": "Nan Shan Seat 1" },
										{ "id": "Nan Shan Seat 2" },
										{ "id": "Nan Shan Seat 3" }
									]
								},
								{
									"id": "33607c9d-af0e-42be-9570-f3b5d9d7821c",
									"name": "Futian",
									"seats": [
										{ "id": "Nan Shan Seat 1" },
										{ "id": "Nan Shan Seat 2" },
										{ "id": "Nan Shan Seat 3" }
									]
								}

							]
						}, {
							"id": "a52915ca-c732-4a1f-8e39-7daf812fbb40",
							"name": "Chongqing",
							"childLocations": [
								{
									"id": "c84061d7-3ab0-493e-bedf-ef8e650d16e4",
									"name": "Guanyinqiao",
									"seats": [
										{ "id": "Guanyinqiao Seat 1" },
										{ "id": "Guanyinqiao Seat 2" }
									]
								}
							],
							"seats": [{ "id": "Chongqing Seat 1" }]

						}
    				]
    			}];
    		};

    		return that;
    	};
    }
);

