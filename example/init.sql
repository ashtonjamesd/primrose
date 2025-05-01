create database master
use master

create user admin identified by 'admin'
grant all privileges on *.* to admin