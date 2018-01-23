DELETE FROM [mart].[dim_request_type] WHERE request_type_id = 4

GO

INSERT INTO [mart].[dim_request_type]
(request_type_id,request_type_name, resource_key)
VALUES
(4,'Overtime','ResRequestTypeOvertime')