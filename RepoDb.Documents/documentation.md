# RepoDb
A dynamic ORM .Net Library used to create an entity-based repository classes when accessing data from the database.

## Class Entity

It is required that the entity classes inherit the `RepoDb.DataEntity` class. It is also advisable (not required) that an explicit interface must be implemented in the entity classes. The interfaces must implement the `RepoDb.Interfaces.IDataEntity` interface in order to be considered as a contracted data entity. See example below.

Entity Interface:
```
public interface IStock : IDataEntity

{
	int Id { get; set; }
	string Name { get; set; }
	...
	DateTime CreatedDate { get; set; }
}
```
Entity Class:
```
public class Stock : DataEntity, IStock
{
	public int Id { get; set; }
	public string Name { get; set; }
	...
	public DateTime CreatedDate { get; set; }
}
```
By default, `RepoDb` is using the class name as the default mapped object in the database. The class above named `Stock` is automatically be mapped to `[dbo].[Stock]` database object.

### Map Class Attribute

The class name mappings can be changed by specifying the `RepoDb.Attributes.Map` attribute on the class level. See sample below.
```
[Map("[dbo].[StockTable]", CommandType.Text)]
public class Stock : DataEntity, IStock
```
Above class `Stock` is forcely mapped to `[dbo].[StockTable]` of the database.

The `Map` attribute has second parameter called `commandType` of `System.Data` namespace. If the `CommandType` parameter is defined, the library will then use the class object to be executed under that command type. See Microsoft documentation [here](https://msdn.microsoft.com/en-us/library/system.data.commandtype%28v=vs.110%29.aspx).

**Note:** The `commandType` parameter is ignored if implemented at the field-level.

### Map Field Attribute
[Soon to be supported above v1.0.9]

By default, at the field-level, the entity class `property` is mapped to the database object `field` based on the equality of the name (case-insensitive). If the `Map` attribute is defined, it will force the entity class `property` to be mapped directly to the table `field` based on the name defined at the `Map` attribute. See sample below:
```
[Map("[dbo].[StockTable]", CommandType.Text)]
public class Stock : DataEntity, IStock
{
	[Map("StockId")]
	public int Id { get; set; }
	[Map("SecurityName")]
	public string Name { get; set; }
	...
	public DateTime CreatedDate { get; set; }
}
```
On the sample class above named `Stock`, the property `Id` is mapped to `StockId` field and the property `Name` is mapped to `SecurityName` field.

### Primary Attribute

The `Primary` attribute is used in order to define which property of the data entity class is the primary key. See sample below.
```
[Primary]
public int Id { get; set; }
```
This attribute accepts a `System.Boolean` parameter that tells whether the primary property is an identity column. If sets to `true`, the value of the identity column of the newly added record from the database will be returned during `Insert` operation.

By default, the library has built-in mechanism on identifying the primary key. If the `Primary` attribute is defined, the first occurrence will automatically be qualified as the primary property.

If the `Primary` attribute is not defined, then the following identifications will be used.

  - Search for `Id` property. If present, this automatically overrule the mechanism.
  - If there is no `Id` property from the class, the class name plus the word `Id` will be evaluated. This means that the `Stock` class, the property `StockId` will be identified.
  - If both properties above `Id` and `StockId` is not defined, then the mechanism will evaluate the `Map` attribute mapped object plus the `Id` word. On the above mappings, `Map("[dbo].[StockTable]")`, the `StockTableId` will be evalulated.

### Ignore Attribute

The `Ignore` attribute is necessary to command the library which property of the entity class is being ignored in certain operations. It accepts an argument of `RepoDb.Enumerations.Command` enumeration. The following are the values of this enumeration.

 - None
 - Select
 - Insert
 - Update
 - Delete
 - Create
 - Drop
 - Alter
 - Execute

Below is a way on how to marked the property `Id` to be ignored during `Update` operation.
```
[Ignore(Command.Update)]
public int Id { get; set; }
```
On the other hand, this attribute can also be used for multiple ignore commands on a single property. Below is the sample on how to marked the `CreatedDate` property to be ignored during `Insert` and `Update` operation. *(This is advisable only to a property with default values from the database)*
```
[Ignore(Command.Insert | Command.Update)]
public int CreatedDate { get; set; }
```
Currently, the library is only using the `None`, `Select`, `Insert`, `Update`, `Delete` commands on its certain operations.

## Repository

The library contains two base repository objects, the `RepoDb.BaseRepository<TEntity, TDbConnection>` and the `RepoDb.DbRepository<TDbConnection>`. The latter is the `heart` of `RepoDb` as it contains all the operations that is being used by all other repositories within or outside the library.

This means that, the `BaseRepository` is only abstracting the operations of the `DbRepository` object in all areas.

Both classes accept the following parameters on there respective constructors.

 - **connectionString** - the connection string to connect to.
 - **commandTimeout (optional)** - the command timeout in seconds. It is being used to set the value of the `DbCommand.CommandTimeout` object prior to the execution of the operation.
 - **cache (optional)** - the cache object to be used by the repository. By default, the repository is using the `RepoDb.MemoryCache` object.
 - **trace (optional)** - the trace object to be used by the repository. The default is `null`.
 - **statementBuilder (optional)** - the statement builder object to be used by the repository. By default, the repository is using the `RepoDb.SqlDbStatementBuilder` object.

### Creating a Repository

The class must inherit the `RepoDb.BaseRepository<TEntity, TDbConnection>` object when creating an entity-based repository. See sample below.
```
public StockRepository : BaseRepository<Stock, SqlConnection>
{
	public StockRepository(string connectionString)
		: base(connectionString)
}
```
The class above named `StockRepository` is an entity-based repository for an entity `Stock`. It uses the `SqlConnection` object as the database connection provider when accessing the datababase.

**Note:** Any type of `IDbConnection` type can be passed connection on the second dynamic type of the said repository. It is very useful when accessing different database like Oracle, PostgreSQL, OleDb databases and others.

On the other hand, if a shared repository is being created, then it should inherit from `DbRepository<TDbConnection>` repository. See sample below.
```
public SharedRepository : DbRepository<SqlConnection>
{
	public SharedRepository(string connectionString)
		: base(connectionString)
}
```
Since using this repository does not limit to a single entity object, then all operations of the said repository can be used anywhere in the solution by any entities.

We also recommend that a contracted interface must be implemented when creating a repository, it helps the repository to be easily injectable anywhere in the solution (if an Dependency Injection library is used).

To create an interface for the repository, it should implement the `RepoDb.Interfaces.IBaseRepository<TEntity, TDbConnection>` or `RepoDb.Interfaces.IDbRepository<TDbConnection>` interface on the contract interface. See sample below.
```
public interface IStockRepository : IBaseRepository<TEntity, TDbConnection>
{
	...
}

public interface ISharedRepository : IDbRepository<TDbConnection>
{
	...
}
```
and implement it on the custom repositories as shown below.
```
public StockRepository : BaseRepository<Stock, SqlConnection>, IStockRepository
{
	public StockRepository(string connectionString)
		: base(connectionString)
}

public SharedRepository : DbRepository<SqlConnection>, ISharedRepository
{
	public SharedRepository(string connectionString)
		: base(connectionString)
}
```

## Creating a Connection

The repository object is used to create a connection object (`System.Data.IDbConnection`), allowing the caller to manually manipulate the data with its own.

A method named **CreateConnection** is used to create a new connection object. Below is the way on how to create a connection.
```
var stockRepository = new StockRepository(connectionString);
var connection = stockRepository.CreateConnection();
```
The library has created certain extension methods on the connection object. Below are the list of extension methods.

 - **EnsureOpen** - used to ensure that the connection is open. Returns the instance of the connection object.
 - **ExecuteReader** - used to read certain records from the database in fast-forward access.
 - **ExecuteNonQuery** - used to execute a non-queryable query statement in the database.
 - **ExecuteScalar** - used to execute a command that returns a single-object value from the database. 

### EnsureOpen

This operation is used to ensure that the current connection object is open. The underlying call of the method is the `IDbConnection.Open` method. It returns the connection object instance (self instance).

Below is the way on how to use the operation.
```
var stockRepository = new StockRepository(connectionString);
using (var connection = stockRepository.CreateConnection().EnsureOpen())
{
	...
}
```

### ExecuteReader

This connection extension method is use to execute a SQL statement query from the database in fast-forward access. It returns an `IEnumerable` object with `dynamic` or `object` type as its generic type.

Below are the parameters:

 - **commandText** - the SQL statement to be used for execution.
 - **param** - the parameters to be used for the execution. It could be an entity class or a dynamic object.
 - **commandTimeout** - the command timeout in seconds to be used when executing the query in the database.
 - **commandType** - the type of command to be used whether it is a `Text`, `StoredProcedure` or `TableDirect`.
 - **transaction** - the transaction object be used when executing the command.
 - **trace** - the trace object to be used on this operation.

Below is the way on how to call the operation.
```
var stockRepository = new StockRepository(connectionString);
using (var connection = stockRepository.CreateConnection().EnsureOpen())
{
	var param = new { Name = "GOOGL" };
	var result = connection.ExecuteReader<Stock>("SELECT * FROM [dbo].[Stock] WHERE (Name = @Name);", param);
}
```

### ExecuteNonQuery

This connection extension method is used to execute a non-queryable SQL statement. It returns an `int` that holds the number of affected rows during the execution.

Below are the parameters:

 - **commandText** - the SQL statement to be used for execution.
 - **param** - the parameters to be used for the execution.
 - **commandTimeout** - the command timeout in seconds to be used when executing the query in the database.
 - **commandType** - the type of command to be used whether it is a `Text`, `StoredProcedure` or `TableDirect`.
 - **transaction** - the transaction object be used when executing the command.
 - **trace** - the trace object to be used on this operation.

Below is the way on how to call the operation.
```
var stockRepository = new StockRepository(connectionString);
using (var connection = stockRepository.CreateConnection().EnsureOpen())
{
	var param = new
	{
		Name = "GOOGL",
		Motto = "Do not be evil.",
		UpdatedDate = DateTime.UtcNow
	};
	var result = connection.ExecuteNonQuery("UPDATE [dbo].[Stock] SET Motto = @Motto, UpdatedDate = @UpdatedDate WHERE Name = @Name;", param);
}
```

### ExecuteScalar

This connection extension method is used to execute a query statement that returns single value (of type `System.Object`).

Below are the parameters:

 - **commandText** - the SQL statement to be used for execution.
 - **param** - the parameters to be used for the execution.
 - **commandTimeout** - the command timeout in seconds to be used when executing the query in the database.
 - **commandType** - the type of command to be used whether it is a `Text`, `StoredProcedure` or `TableDirect`.
 - **transaction** - the transaction object be used when executing the command.
 - **trace** - the trace object to be used on this operation.

Below is the way on how to call the operation.
```
var stockRepository = new StockRepository(connectionString);
using (var connection = stockRepository.CreateConnection().EnsureOpen())
{
	var param = new { Name = "GOOGL" };
	var id = connection.ExecuteScalar("SELECT [Id] FROM [dbo].[Stock] Name = @Name;", param);
}
```

**Note:** All repositories operations are using these connection extension methods underneath on every execution.

## Expression Tree

The expression tree defines the best possible way of doing a `WHERE` expression (SQL Statement) by composing it via `dynamic` or `System.Interfaces.IQueryGroup` objects.

Certain operations uses expression tree to compose the SQL Statement on the fly prior the execution back to the database.

Below are the objects useful for composing the expression tree.

 - **QueryGroup** - used to group an expression.
 - **AndQueryGroup** - used to group an expression with `AND` conjunction.
 - **OrQueryGroup** - used to group an expression with `OR` conjunction.
 - **QueryField** - holds the field/value pair values of the expressions.
 - **Conjunction** - an enumeration that holds the value whether the expression is on `And` or `Or` operation.
 - **Operation** - an enumeration that holds the value what kind of operation is going to be executed on certain expression. It holds the value of like `Equal`, `NotEqual`, `Between`, `GreaterThan` and etc.

There are two ways of building the expression trees, the explicit way by using `IQueryGroup` objects and dynamic way by using `dynamic` objects.

### QueryGroup

The `QueryGroup` object is very important to group an expression (implements `RepoDb.Interfaces.IQueryGroup`). Below are the constructor parameters.

 - **queryFields** - the list of `IQueryField` objects to be included in the expression composition. It stands as `[FieldName] = @FiedName` when it comes to SQL Statement compositions.
 - **queryGroups** - the list of child `IQueryGroup` objects to be included in the expresson composition. It stands as the `([FieldName] = @FieldName AND [FieldName1] = @FieldName1)` when it comes to SQL Statement compositions.
 - **conjunction** - the conjuction to be used when grouping the fields. It stands as the `AND` or `OR` in the SQL Statement compositions.
 
Below is the pseudo-codes on how to create a query groups driven expressions.

Explicit way:
```
var tree = new QueryGroup(new QueryField[] { ... }, new QueryGroup[] { ... }, Conjunction.And);
```
Dynamic way:
```
var tree = new {
	Conjunction = Conjunction.And,
	Field1 = "Field1",
	Field2 = "Field2",
	QueryGroups = new []
	{
		new { .... }
	}
};
```
Below are the actual code-compositions when querying a `Stock` data where (`Name` has `A` character) `OR` (the `DateInserted` is between `Date1` and `Date2` variables `AND` the `IsActive` flag is `true`.

Explicit way:
```
var tree = new QueryGroup(
	new QueryField("Id", Operation.Like, "%A%").AsEnumerable(),
	new QueryGroup(
		new []
		{
			new QueryField("DateInserted", Operation.Between, new [] { Date1, Date2 }),
			new QueryField("IsActive", true }),
		}
	).AsEnumerable(),
	Conjunction.Or
);
```
Dynamic way:
```
var tree = {
	Conjunction = Conjunction.Or,
	Id = new { Operation = Operation.Like, Value = "%A%" },
	QueryGroups = new {
		DateInserted = new { Operation = Operation.Between, Value = new [] { Date1, Date2 } },
		IsActive = true
	}
};
```
The expressions above will return a SQL Statement below.
```
WHERE
(
	[Id] LIKE @Id
	OR
	(
		([DateInserted] BETWEEN @DateInserted_1 AND @DateInserted_2)
		AND
		([IsActive] = @IsActive)
	)
);
```
where the values of the following fields are (`@Id` like `%A%`, `@DateInserted_1` = `Date1`, `@DateInserted_2` = `Date2`, `@IsActive` = `true`).

By default, the `QueryGroup` conjunction is `Conjunction.And`. It can be explicitly set it by passing the `Conjunction.Or` value to the `Conjunction` field (dynamic way) or parameter (explicit way).

TODO: By Michael Pendon

## Operations

The repositories contain different operations to manipulate the data from the database. Below are the list of common operations widely used.

 - **Query** - used to query a record from the database. It uses the `SELECT` command of SQL.
 - **Insert** - used to insert a record in the database. It uses the `INSERT` command of SQL.
 - **Update** - used to update a record in the database. It uses the `UPDATE` command of SQL.
 - **Delete** - used to delete a record in the database. It uses the `DELETE` command of SQL.
 - **Merge** - used to merge a record in the database. It uses the `MERGE` command of SQL.
 - **BulkInsert** - used to bulk-insert the records in the database.
 - **BatchQuery** - [soon to be supported] used to query a record from the database by batch. It uses the `SELECT` in combination of `ROW_NUMBER` and `ORDER` command of SQL.
 - **ExecuteReader** - used to read certain records from the database in fast-forward access.
 - **ExecuteNonQuery** - used to execute a non-queryable query statement in the database.
 - **ExecuteScalar** - used to execute a command that returns a single-object value from the database.

On the other hand, the library has extension methods on the `IDbConnection` object level that can be used to execute. Below are the 3 common connection extension methods widely used.

 - **ExecuteReader** - used to read certain records from the database in fast-forward access.
 - **ExecuteNonQuery** - used to execute a non-executable query in the database.
 - **ExecuteScalar** - used to execute a command that returns a single-object value from the database.

All operations mentioned above has its own corresponding asynchronous operation. Usually, the asynchronous operation is only appended by `Async` keyword. Below are the list of asynchronous operations.

  - **QueryAsync**
  - **InsertAsync**
  - **UpdateAsync**
  - **DeleteAsync**
  - **MergeAsync**
  - **BulkInsertAsync**
  - **BatchQueryAsync** [soon to be supported]
  - **ExecuteReaderAsync**
  - **ExecuteNonQueryAsync**
  - **ExecuteScalar**

## Query Operation

This operation is used to query a data from the database and returns an `IEnumerable<TEntity>` object. Below are the parameters.

  - **where** - an expression to used to filter the data.
  - **transaction** - the transaction object to be used when querying a data.
  - **top** - the value used to return certain number of rows from the database.
  - **orderBy** - the list of fields to be used to sort the data during querying.
  - **cacheKey** - the key of the cache to check.

Below is a sample on how to query a data.
```
var stockRepository = new StockRepository(connectionString);
var stocks = stockRepository.Query();
```
Above snippet will return all the `Stock` records from the database. The data can filtered using the `where` parameter. See sample below.

Implicit way:
```
var stocks = stockRepository.Query(1);
```
Dynamic way:
```
var stocks = stockRepository.Query(new { Id = 1 });
```
Explicity way:
```
var stocks = stockRepository.Query(
	new QueryGroup(new QueryField("Id", 1).AsEnumerable())
);
```
Below is the sample on how to query with multiple columns.
```
var stocks = stockRepository.Query(new { Id = 1, Name = "AAPL" });
```
Explicity way:
```
var stocks = stockRepository.Query(
	new QueryGroup(
		new []
		{
			new QueryField("Id", Operation.Equal, 1),
			new QueryField("Name", Operation.Equal, "AAPL")
		}
	));
```
When querying a data where `Id` field is greater than 50 and less than 100. See sample expressions below.

Dynamic way:
```
var stocks = stockRepository.Query(
new
{
	Id = new { Operation = Operation.Between, Value = new int[] { 50, 100 } }
});
```
or

```
var stocks = stockRepository.Query(
new
{
	QueryGroups = new[]
	{
		new
		{ 
			Id = { Operation = Operation.GreaterThanOrEqual, Value = 50 }
		},
		new
		{
			Id = { Operation = Operation.LessThanOrEqual, Value = 100 }
		}
	}
});
```
or
```
var stocks = stockRepository.Query(
new
{
	Id = new
	{
		Operation = Operation.All,
		Value = new object[]
		{
			new { Operation = Operation.GreaterThanOrEqual, Value = 50 },
			new { Operation = Operation.LessThanOrEqual, Value = 100 }
		} 
	}
});
```
Explicit way:
```
var stocks = stockRepository.Query(
new QueryGroup(
	new []
	{
		new QueryField("Id", Operation.GreaterThanOrEqual, 50),
		new QueryField("Id", Operation.LessThanOrEqual, 100)
	}
));
```
or
```
var stocks = stockRepository.Query(new QueryGroup(
	new QueryField("Id", Operation.Between, new [] { 50, 100 }).AsEnumerable()
));
```
**Note**: Querying a record using `PrimaryKey` will throw a `PrimaryFieldNotFoundException` exception back to the caller if the `PrimaryKey` is not found from the entity.

### Ordering

An ordering is the way of sorting the result of your query in `ascending` or `descending` order, depending on the qualifier fields. Below is a sample snippet that returns the `Stock` records ordered by `ParentId` field in ascending manner and `Name` field is in `descending` manner.

Dynamic way:
```
var orderBy = new
{
	ParentId = Order.Ascending,
	Name = Order.Descending
};
var stocks = stockRepository.Query(new { ParentId = 1 }, orderBy: OrderField.Parse(orderBy));
```
Explicit way:
```
var orderBy = new []
{
	new OrderField("ParentId", Order.Ascending),
	new OrderField("Name", Order.Descending)
};
var stocks = stockRepository.Query(new { ParentId = 1 }, orderBy: orderBy);
```
The `RepodDb.OrderField` is an object that is being used to order a query result. The `Parse` method is used to convert the `dynamic` object to become an `OrderField` instances.

**Note:** When composing a dynamic ordering object, the value of the properties should be equal to `RepoDb.Interfaces.Order` values (`Ascending` or `Descending`). Otherwise, an exception will be thrown during `Parse` operation.

### Top

A top parameter is used to limit the result when querying a data from the database. Below is a sample way on how to use the top parameter.

Dynamic way:
```
var orderBy = new
{
	ParentId = Order.Ascending,
	Name = Order.Descending
};
var stocks = stockRepository.Query(new { ParentId = 1 }, orderBy: OrderField.Parse(orderBy), top: 100);
```
Explicit way:
```
var orderBy = new []
{
	new OrderField("ParentId", Order.Ascending),
	new OrderField("Name", Order.Descending)
};
var stocks = stockRepository.Query(new { ParentId = 1 }, orderBy: orderBy, top: 100);
```

**Note:** Top value should not be less than `1`, otherwise the parameter is ignored.

## Insert Operation

This operation is used to insert a record in the database. It returns an object valued by the `PrimaryKey` column. If the `PrimaryKey` column is identity, this operation will return the newly added identity column value. Below are the parameters:

 - **entity** - the entity object to be inserted.
 - **transaction** - the transaction object to be used when inserting a data.

Below is a sample on how to insert a data.
```
var stockRepository = new StockRepository(connectionString);
var stock = new Stock()
{
	Name = "GOOGL",
	CreatedDate = DateTime.UtcNow
};
repository.Insert(stock);
```

## Update Operation

This operation is used to update an existing record from the database. It returns an `int` value indicating the number of rows affected by the updates. Below are the parameters:

 - **entity** - the entity object to be updated.
 - **where** - an expression to used when updating a record.
 - **transaction** - the transaction object to be used when updating a data.

Below is a sample on how to update a data.
```
var stockRepository = new StockRepository(connectionString);
var stock = stockRepository.Query(new { Name = "GOOGL" }).FirstOrDefault();
if (stock != null)
{
	stock.Motto = "Do not be evil.";
	stock.UpdateDate = DateTime.UtcNow;
	var affectedRows = repository.Update(stock);
}
```
Dynamic way (column-based update) [Soon to be supported]:
```
var stockRepository = new StockRepository(connectionString);
var affectedRows = stockRepository.Update(
new
{
	Motto = "Do not be evil."
	UpdatedDate = DateTime.UtcNow
},
new
{
	Name = "GOOGL"
});
```
**Note**:  Updating a record using `PrimaryKey` will throw a `PrimaryFieldNotFoundException` exception back to the caller if the `PrimaryKey` is not found from the entity.

## Delete Operation

This operation is used to delete an existing record from the database. It returns an `int` value indicating the number of rows affected by the delete. Below are the parameters:

 - **where** - an expression to used when deleting a record.
 - **transaction** - the transaction object to be used when deleting a data.

Below is a sample on how to delete a data.
```
var stockRepository = new StockRepository(connectionString);
var stock = stockRepository.Query(new { Name = "GOOGL" }).FirstOrDefault();
if (stock != null)
{
	var affectedRows = stockRepository.Delete(stock);
}
```
or by `PrimaryKey`
```
var affectedRows = stockRepository.Delete(stock.Id);
```
Dynamic way:
```
var stockRepository = new StockRepository(connectionString);
var affectedRows = stockRepository.Delete(new { Name = "GOOGL" });
```
**Note**: Deleting a record using `PrimaryKey` will throw a `PrimaryFieldNotFoundException` exception back to the caller if the `PrimaryKey` is not found from the entity.

## Merge Operation

This operation is used to merge an entity from the existing record from the database. It returns an `int` value indicating the number of rows affected by the merge. Below are the parameters:

 - **entity** - the entity object to be merged.
 - **qualifiers** - the list of fields to be used as a qualifiers when merging a record.
 - **transaction** - the transaction object to be used when merging a data.

Below is a sample on how to merge a data.
```
var stockRepository = new StockRepository(connectionString);
var stock = stockRepository.Query(1);
stock.Motto = "Do not be evil all the time.";
UpdatedDate = DateTime.UtcNow;
stockRepository.Merge(stock, Field.Parse(new { stock.Name }));
```
or by creating a new entity with existing qualifier value.
```
var stock = new Stock()
{
	Name = "GOOGL"
	Motto = "Do not be evil all the time.",
	UpdatedDate = DateTime.UtcNow
};
stockRepository.Merge(stock, Field.Parse(new { stock.Name }));
```
**Note**:  If the `qualifiers` are not defined, the library will automatically used the `PrimaryKey` as the default qualifier. If however the `PrimaryKey` is not defined in the entity, a `PrimaryFieldNotFoundException` will be thrown back to the caller.

Please also note that merging is a process of updating and inserting. If the data is present in the database using the qualifiers, then the existing data will be updated, otherwise, a new data will be inserted in the database.

## BulkInsert Operation

This operation is used to bulk-insert the entities to the database. It returns an `int` value indicating the number of rows affected by the bulk-inserting. Below are the parameters:

 - **entities** - the list of entities to be inserted.
 - **transaction** - the transaction object to be used when doing bulk-insert.

Below is a sample on how to do bulk-insert.
```
var stockRepository = new StockRepository(connectionString);
var entities = new List<Stock>();
entities.Add(new Stock()
{
	Name = "GOOGL"
	Motto = "Do not be evil all the time.",
	CreatedDate = DateTime.UtcNow,
	UpdatedDate = DateTime.UtcNow
});
entities.Add(new Stock()
{
	Name = "MSFT"
	Motto = "Make it all easy.",
	CreatedDate = DateTime.UtcNow,
	UpdatedDate = DateTime.UtcNow
});
var affectedRows = stockRepository.BulkInsert(entities);
```

## ExecuteReader Operation

This connection extension method is used to execute a SQL Statement query from the database in fast-forward access. It returns an `IEnumerable` object with `dynamic` or `object` type as its generic type.

Below are the parameters:

 - **commandText** - the SQL statement to be used for execution.
 - **param** - the parameters to be used for the execution. It could be an entity class or a dynamic object.
 - **commandTimeout** - the command timeout in seconds to be used when executing the query in the database.
 - **commandType** - the type of command to be used whether it is a `Text`, `StoredProcedure` or `TableDirect`.
 - **transaction** - the transaction object be used when executing the command.

Below is the way on how to call the operation.
```
var stockRepository = new StockRepository(connectionString);
var param = new { Name = "GOOGL" };
var result = stockRepository.ExecuteReader<Stock>("SELECT * FROM [dbo].[Stock] WHERE Name = @Name;", param);
```

## ExecuteNonQuery Operation

This connection extension method is used to execute a non-queryable SQL statement query. It returns an `int` that holds the number of affected rows during the execution.

Below are the parameters:

 - **commandText** - the SQL statement to be used for execution.
 - **param** - the parameters to be used for the execution.
 - **commandTimeout** - the command timeout in seconds to be used when executing the query in the database.
 - **commandType** - the type of command to be used whether it is a `Text`, `StoredProcedure` or `TableDirect`.
 - **transaction** - the transaction object be used when executing the command.

Below is the way on how to call the operation.
```
var stockRepository = new StockRepository(connectionString);
var param = new
{
	Name = "GOOGL",
	Motto = "Do not be evil.",
	UpdatedDate = DateTime.UtcNow
};
var result = stockRepository.ExecuteNonQuery("UPDATE [dbo].[Stock] SET Motto = @Motto, UpdatedDate = @UpdatedDate WHERE Name = @Name;", param);
```

## ExecuteScalar Operation

This connection extension method is used to execute a query statement that returns a single value.

Below are the parameters:

 - **commandText** - the SQL statement to be used for execution.
 - **param** - the parameters to be used for the execution.
 - **commandTimeout** - the command timeout in seconds to be used when executing the query in the database.
 - **commandType** - the type of command to be used whether it is a `Text`, `StoredProcedure` or `TableDirect`.
 - **transaction** - the transaction object be used when executing the command.

Below is the way on how to call the operation.
```
var stockRepository = new StockRepository(connectionString);
var param = new { Name = "GOOGL" };
var id = stockRepository.ExecuteScalar("SELECT [Id] FROM [dbo].[Stock] Name = @Name;", param);
```