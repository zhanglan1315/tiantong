create table if not exists order_items (
  id serial not null primary key,
  warehouse_id int not null,
  order_id int not null,
  item_id int not null,
  supplier_id int not null,
  price float not null,
  quantity int not null,
  arrived_quantity int not null,
  unique(order_id, item_id, supplier_id)
);
