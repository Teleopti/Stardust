UPDATE [rta].[Events] 
SET Type = SUBSTRING(Type, 0, CHARINDEX(', Version', Type))
WHERE CHARINDEX(', Version', Type) > 0
GO