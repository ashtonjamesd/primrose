create user primrose identified by 'primrose'
login user primrose identified by 'primrose'

create database master
use master

grant all privileges on master.* to primrose
with grant option

create table primrose_master (
    type varchar(255),
    name varchar(255),
    sql  varchar(MAX),
    test boolean default false
)

insert into primrose_master (type, name, sql)
values ('type test', 'name test', 'sql test')