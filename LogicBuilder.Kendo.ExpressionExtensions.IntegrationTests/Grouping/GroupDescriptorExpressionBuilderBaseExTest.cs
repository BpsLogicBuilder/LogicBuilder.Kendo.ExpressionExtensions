using Kendo.Mvc;
using LogicBuilder.Kendo.ExpressionExtensions.Grouping;
using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests.Grouping
{
    public class GroupDescriptorExpressionBuilderBaseExTest
    {
        [Fact]
        public void SetQueryableOn_GroupDescriptorExpressionBuilderBaseEx_Works()
        {
            // Arrange
            Expression expression = Expression.Parameter(typeof(IQueryable<int?>), "x");
            MockGroupDescriptorExpressionBuilder builder = new(Expression.Constant(Enumerable.Empty<int>().AsQueryable()));

            // Act
            builder.SetQueryable(expression);

            // Assert
            Assert.IsType<ParameterExpression>(builder.Queryable, exactMatch: false);
        }

        [Fact]
        public void GetSortDirectionOn_GroupDescriptorExpressionBuilderBaseEx_Works()
        {
            // Arrange
            Expression expression = Expression.Parameter(typeof(IQueryable<int?>), "x");
            MockGroupDescriptorExpressionBuilder builder = new(Expression.Constant(Enumerable.Empty<int>().AsQueryable()));

            // Act
            builder.SetQueryable(expression);

            // Act & Assert
            Assert.Null(builder.GetSortDirection());
        }

        private class MockGroupDescriptorExpressionBuilder(Expression expression) : GroupDescriptorExpressionBuilderBaseEx(expression)
        {
            public void SetQueryable(Expression newQueryable)
            {
                this.Queryable = newQueryable;
            }

            public ListSortDirection? GetSortDirection() => this.SortDirection;

            protected override LambdaExpression CreateGroupByExpression()
            {
                throw new NotImplementedException();
            }

            protected override LambdaExpression CreateOrderByExpression()
            {
                throw new NotImplementedException();
            }

            protected override LambdaExpression CreateSelectExpression()
            {
                throw new NotImplementedException();
            }
        }
    }
}
