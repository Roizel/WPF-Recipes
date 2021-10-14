create database RecipeDB
go
use RecipeDB
go
create table RecipeTable
(
     Id int identity(1,1) primary key,
     Name nvarchar(max) not null,
     TypeOfCuisine nvarchar(max) not null,
     TypeOfMeal nvarchar(max) not null,
     Ingredients nvarchar(max) not null,
     HowToCook nvarchar(max) not null,
)
go
insert into RecipeTable(Name, TypeOfCuisine, TypeOfMeal, Ingredients, HowToCook)
values('Sushi', 'Japanese', 'Fish', 'Fish..', '...')