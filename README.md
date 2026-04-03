# LogicBuilder.Kendo.ExpressionExtensions

A library that extends Telerik's Kendo UI DataSourceRequest functionality with powerful LINQ expression building capabilities for ASP.NET Core applications.

## Overview

LogicBuilder.Kendo.ExpressionExtensions provides extension methods for creating `IQueryable` expressions from Telerik's `DataSourceRequest` class, enabling seamless integration between Kendo UI components and Entity Framework Core repositories. While this library depends on `Telerik.UI.for.AspNet.Core`, it has not been created or maintained by Telerik/Progress.

## Key Features

- **DataSource Operations**: Transform Kendo UI grid requests into Entity Framework queries with support for:
  - Filtering
  - Sorting
  - Paging
  - Grouping
  - Aggregations (Count, Sum, Min, Max, Average)

- **Data Retrieval Methods**:
  - `GetData<TModel, TData>()` - Retrieve paged and filtered data with full DataSourceResult support
  - `GetSingle<TModel, TData>()` - Fetch a single entity matching filter criteria
  - `GetDynamicSelect<TModel, TData>()` - Execute dynamic projections with custom select expressions
  - Example implementations can be found in the [Helpers class](https://github.com/BpsLogicBuilder/LogicBuilder.Kendo.ExpressionExtensions/blob/master/LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests/Helpers.cs) from the test project.

- **Advanced Query Capabilities**:
  - Select/expand definitions for loading related entities
  - Filtered expansions for child collections
  - Sorted and paginated child collections
  - Dynamic select projections

## Getting Started

### Prerequisites

First, implement the context, store, repository, and service registrations as described in [LogicBuilder.EntityFrameworkCore.SqlServer](https://github.com/BpsLogicBuilder/LogicBuilder.EntityFrameworkCore.SqlServer).

### Basic Usage

```c#
// Use the DataSourceRequest helper to get the DataSourceResult

ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();
DataSourceResult result = await request.GetData<StudentModel, Student>(repository);

public static async Task<DataSourceResult> GetData<TModel, TData>(this DataRequest request, IContextRepository contextRepository) where TModel : BaseModelClass where TData : BaseDataClass 
{ 
	return await request.Options.CreateDataSourceRequest().GetDataSourceResult<TModel, TData> ( contextRepository, request.SelectExpandDefinition ); 
}
```

### Examples

For comprehensive examples of all features, refer to [the data request tests](https://github.com/BpsLogicBuilder/LogicBuilder.DataComponents/blob/master/LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests/DataRequestTests.cs).

## Use Cases

- Building ASP.NET Core APIs that serve Kendo UI grids
- Implementing server-side data operations for large datasets
- Creating flexible query APIs with dynamic filtering and sorting
- Supporting complex data models with related entities

## Related Projects

- [LogicBuilder.EntityFrameworkCore.SqlServer](https://github.com/BpsLogicBuilder/LogicBuilder.EntityFrameworkCore.SqlServer)
