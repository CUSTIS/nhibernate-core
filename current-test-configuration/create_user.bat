rem setup creating user
if not defined DB_UID set DB_UID=%USERNAME%
set DB_UID=%DB_UID:-=_%

rem launch
echo quit|sqlplus.exe sys/egylyarn@evol10 as sysdba @create_user.sql 'nhibernate_%DB_UID%'