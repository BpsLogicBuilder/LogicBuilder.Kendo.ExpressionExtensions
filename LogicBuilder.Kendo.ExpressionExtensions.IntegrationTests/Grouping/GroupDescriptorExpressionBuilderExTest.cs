using Contoso.Data.Entities;
using Kendo.Mvc.UI;
using LogicBuilder.Kendo.ExpressionExtensions.Grouping;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests.Grouping
{
    public class GroupDescriptorExpressionBuilderExTest
    {
        [Fact]
        public void GetChildBuilder_ReturnsExprectedExpression()
        {
            // Arrange
            DataSourceRequestOptions dataSourceRequestOptions = new()
            {
                Aggregate = "lastName-count~enrollmentDate-min",
                Filter = "id~gt~0",
                Group = "enrollmentDate-asc~lastName-desc",
                Page = 1,
                Sort = null,
                PageSize = 5
            };
            DataSourceRequest dataSourceRequest = dataSourceRequestOptions.CreateDataSourceRequest();
            ParameterExpression parameterExpression = Expression.Parameter(typeof(IQueryable<Student>), "x");
            ParameterExpression parameterExpression2 = Expression.Parameter(typeof(IQueryable<Student>), "x");
            GroupDescriptorExpressionBuilderEx builder = new
            (
                parameterExpression,
                dataSourceRequest.Groups[0],
                new GroupDescriptorExpressionBuilderEx(
                    parameterExpression2,
                    dataSourceRequest.Groups[1],
                    null,
                    null
                ),
                null
            );

            // Act
            GroupDescriptorExpressionBuilderEx childBuilder = builder.ChildBuilder;

            // Assert
            Assert.IsType<GroupDescriptorExpressionBuilderEx>(childBuilder, exactMatch: false);
        }
    }
}
