IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[etl_dim_shift_length_id_get]') AND type in (N'P', N'PC'))
DROP PROCEDURE [mart].[etl_dim_shift_length_id_get]
GO


-- =============================================
-- Author:		Jonas
-- Create date: 2015-01-14
-- Description:	Get a shift length id from given shift length in minutes. If not exits hten create it first.
-- =============================================
CREATE PROCEDURE [mart].[etl_dim_shift_length_id_get]
@shift_length_m int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @shift_length_id int

    SELECT @shift_length_id = shift_length_id 
	FROM mart.dim_shift_length
	WHERE shift_length_m = @shift_length_m

	IF @shift_length_id IS NULL
	BEGIN
		INSERT INTO mart.dim_shift_length (shift_length_m, shift_length_h, datasource_id)
		VALUES (@shift_length_m, CONVERT(decimal(10,2), @shift_length_m / 60.0), 1)

		SET @shift_length_id = @@IDENTITY
	END

	SELECT @shift_length_id
END

GO


