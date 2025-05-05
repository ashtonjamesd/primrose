create user primrose identified by 'primrose'
login user primrose identified by 'primrose'

create database master
use master

grant all privileges on master.* to primrose 
with grant option

create table primrose_master (
    type varchar(255),
    name varchar(255),
    sql  varchar(MAX)
)

create table test (x int, y int, z int)

insert into test (x, y, z)
values (1, 1, 2), 
(2, 32, 1), 
(3, 5, 8)

select * from test
where x > 1 and y = 32 or y = 5