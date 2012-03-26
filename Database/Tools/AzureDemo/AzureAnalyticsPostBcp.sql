set quoted_identifier on
set arithabort off
set numeric_roundabort off
set ansi_warnings on
set ansi_padding on
set ansi_nulls on
set concat_null_yields_null on
set cursor_close_on_commit off

GO
CREATE UNIQUE INDEX UIX_only_one_default_zone ON mart.dim_time_zone(only_one_default_zone)
GO