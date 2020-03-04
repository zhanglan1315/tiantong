create table if not exists areas (
  id serial not null primary key,
  warehouse_id int not null,
  number varchar(255) not null,
  name varchar(255) not null,
  comment varchar(255) not null,
  total_area varchar(255) not null,
  is_enabled boolean not null default false,
  unique(warehouse_id, number)
);
