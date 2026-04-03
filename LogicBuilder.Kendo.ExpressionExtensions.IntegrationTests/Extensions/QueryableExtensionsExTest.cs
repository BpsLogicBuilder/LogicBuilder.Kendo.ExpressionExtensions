using Contoso.Data.Entities;
using Kendo.Mvc.UI;
using LogicBuilder.Kendo.ExpressionExtensions.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests.Extensions
{
    public class QueryableExtensionsExTest
    {
        [Fact]
        public void CallWhereWithFilterDescriptors_UpdatesTheExpression()
        {
            //arrange
            DataSourceRequestOptions dataSourceRequestOptions = new()
            {
                Aggregate = "lastName-count~enrollmentDate-min",
                Filter = "id~gt~0",
                Group = "enrollmentDate-asc",
                Page = 1,
                Sort = null,
                PageSize = 5
            };
            DataSourceRequest dataSourceRequest = dataSourceRequestOptions.CreateDataSourceRequest();
            ParameterExpression parameterExpression = Expression.Parameter(typeof(IQueryable<Student>), "x");

            //act
            Expression expression = parameterExpression.Where(dataSourceRequest.Filters);

            //assert
            Assert.IsType<MethodCallExpression>(expression, exactMatch: false);
        }

        [Fact]
        public void CallWhereWithoutFilterDescriptors_LeavesTheExpressionUnChanged()
        {
            //arrange
            ParameterExpression parameterExpression = Expression.Parameter(typeof(IQueryable<Student>), "x");

            //act
            Expression expression = parameterExpression.Where([]);

            //assert
            Assert.IsType<ParameterExpression>(expression, exactMatch: false);
        }

        [Fact]
        public void CallAggregateWithAggregateFunctions_UpdatesTheExpression()
        {
            //arrange
            DataSourceRequestOptions dataSourceRequestOptions = new()
            {
                Aggregate = "lastName-count~enrollmentDate-min",
                Filter = "id~gt~0",
                Group = "enrollmentDate-asc",
                Page = 1,
                Sort = null,
                PageSize = 5
            };
            DataSourceRequest dataSourceRequest = dataSourceRequestOptions.CreateDataSourceRequest();
            ParameterExpression parameterExpression = Expression.Parameter(typeof(IQueryable<Student>), "x");

            //act
            Expression expression = parameterExpression.Aggregate(dataSourceRequest.Aggregates.SelectMany(a => a.Aggregates));

            //assert
            Assert.IsType<MethodCallExpression>(expression, exactMatch: false);
        }

        [Fact]
        public void CallAggregateWithoutAggregateFunctions_ReturnsNull()
        {
            //arrange
            ParameterExpression parameterExpression = Expression.Parameter(typeof(IQueryable<Student>), "x");

            //act
            Expression expression = parameterExpression.Aggregate([]);

            //assert
            Assert.Null(expression);
        }

        [Fact]
        public void GetGroupByExpression_CanBeCalledWithoutInitialPaging()
        {
            //arrange
            DataSourceRequestOptions dataSourceRequestOptions = new()
            {
                Aggregate = "lastName-count~enrollmentDate-min",
                Filter = "id~gt~0",
                Group = "enrollmentDate-asc",
                Page = 1,
                Sort = null,
                PageSize = 5
            };
            DataSourceRequest dataSourceRequest = dataSourceRequestOptions.CreateDataSourceRequest();
            ParameterExpression parameterExpression = Expression.Parameter(typeof(IQueryable<Student>), "x");

            //act
            Expression expression = parameterExpression.GetGroupByExpression(dataSourceRequest.Groups);

            //assert
            Assert.IsType<MethodCallExpression>(expression, exactMatch: false);
        }

        [Fact]
        public void CallOrderByWithoutASortDirection_DoesnotChangeTheExpression()
        {
            //arrange
            ParameterExpression parameterExpression = Expression.Parameter(typeof(IQueryable<Student>), "x");
            Expression<Func<Student, object>> selector = x => x.LastName;

            //act
            Expression expression = parameterExpression.OrderBy(selector, null);

            //assert
            Assert.IsType<ParameterExpression>(expression, exactMatch: false);
        }

        [Fact]
        public void CreateUngroupedMethodExpression_ThrowsIfGroupsArePresent()
        {
            //arrange
            DataSourceRequestOptions dataSourceRequestOptions = new()
            {
                Aggregate = "lastName-count~enrollmentDate-min",
                Filter = "id~gt~0",
                Group = "enrollmentDate-asc",
                Page = 1,
                Sort = null,
                PageSize = 5
            };
            DataSourceRequest dataSourceRequest = dataSourceRequestOptions.CreateDataSourceRequest();
            ParameterExpression parameterExpression = Expression.Parameter(typeof(IQueryable<Student>), "x");

            //act & assert
            Assert.Throws<ArgumentException>(() => dataSourceRequest.CreateUngroupedMethodExpression(parameterExpression));
        }

        [Fact]
        public void CreateUngroupedQueryableExpression_ThrowsIfGroupsArePresent()
        {
            //arrange
            DataSourceRequestOptions dataSourceRequestOptions = new()
            {
                Aggregate = "lastName-count~enrollmentDate-min",
                Filter = "id~gt~0",
                Group = "enrollmentDate-asc",
                Page = 1,
                Sort = null,
                PageSize = 5
            };
            DataSourceRequest dataSourceRequest = dataSourceRequestOptions.CreateDataSourceRequest();

            //act & assert
            Assert.Throws<ArgumentException>(dataSourceRequest.CreateUngroupedQueryableExpression<Student>);
        }

        [Fact]
        public void CreateGroupedByQueryExpressions_ThrowsIfGroupsIsNull()
        {
            //arrange
            DataSourceRequestOptions dataSourceRequestOptions = new()
            {
                Aggregate = "lastName-count~enrollmentDate-min",
                Filter = "id~gt~0",
                Group = null,
                Page = 1,
                Sort = null,
                PageSize = 5
            };
            DataSourceRequest dataSourceRequest = dataSourceRequestOptions.CreateDataSourceRequest();

            //act & assert
            Assert.Throws<ArgumentException>(dataSourceRequest.CreateGroupedByQueryExpressions<Student>);
        }

        [Fact]
        public void CreateGroupedByQueryExpressions_ThrowsIfGroupsIsEmpty()
        {
            //arrange
            DataSourceRequestOptions dataSourceRequestOptions = new()
            {
                Aggregate = "lastName-count~enrollmentDate-min",
                Filter = "id~gt~0",
                Group = null,
                Page = 1,
                Sort = null,
                PageSize = 5
            };
            DataSourceRequest dataSourceRequest = dataSourceRequestOptions.CreateDataSourceRequest();
            dataSourceRequest.Groups = [];

            //act & assert
            Assert.Throws<ArgumentException>(dataSourceRequest.CreateGroupedByQueryExpressions<Student>);
        }

        [Fact]
        public void CreateAggregatesQueryExpressions_ThrowsIfAggregatesIsNull()
        {
            //arrange
            DataSourceRequestOptions dataSourceRequestOptions = new()
            {
                Aggregate = null,
                Filter = "id~gt~0",
                Group = null,
                Page = 1,
                Sort = null,
                PageSize = 5
            };
            DataSourceRequest dataSourceRequest = dataSourceRequestOptions.CreateDataSourceRequest();

            //act & assert
            Assert.Throws<ArgumentException>(dataSourceRequest.CreateAggregatesQueryExpressions<Student>);
        }

        [Fact]
        public void CreateAggregatesQueryExpressions_ThrowsIfAggregatesIsEmpty()
        {
            //arrange
            DataSourceRequestOptions dataSourceRequestOptions = new()
            {
                Aggregate = null,
                Filter = "id~gt~0",
                Group = null,
                Page = 1,
                Sort = null,
                PageSize = 5
            };
            DataSourceRequest dataSourceRequest = dataSourceRequestOptions.CreateDataSourceRequest();
            dataSourceRequest.Aggregates = [];

            //act & assert
            Assert.Throws<ArgumentException>(dataSourceRequest.CreateAggregatesQueryExpressions<Student>);
        }
    }
}
