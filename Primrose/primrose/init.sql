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

create table test (x varchar(MAX), y int, z int)

insert into test (x, y, z)
values ('f', 1, 2), 
(null, 32, 1), 
(null, 5, 8)

update test
set x = 'b'
where x is not null

select * from test
where x is null