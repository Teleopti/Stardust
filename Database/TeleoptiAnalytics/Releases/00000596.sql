update mart.dim_acd_login
set acd_login_original_id = left(acd_login_original_id,len(acd_login_original_id)-11)
where right(acd_login_original_id,11) = ' (inactive)'
