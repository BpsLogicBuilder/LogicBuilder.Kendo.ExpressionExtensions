using Kendo.Mvc;
using LogicBuilder.Kendo.ExpressionExtensions.Expressions;
using LogicBuilder.Kendo.ExpressionExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LogicBuilder.Kendo.ExpressionExtensions.Sorting
{
    internal class SortDescriptorCollectionExpressionBuilderEx(Expression parentExpression, IEnumerable<SortDescriptor> sortDescriptors)
    {
        private readonly IEnumerable<SortDescriptor> sortDescriptors = sortDescriptors;
        private readonly Expression parentExpression = parentExpression;

        public MethodCallExpression GetSortExpression()
        {
            MethodCallExpression? mce = null;
            bool isFirst = true;

            foreach (var descriptor in this.sortDescriptors)
            {
                Type memberType = typeof(object);
                var descriptorBuilder = ExpressionBuilderFactoryEx.MemberAccess(this.parentExpression, memberType, descriptor.Member);
                var expression = descriptorBuilder.CreateLambdaExpression();

                string methodName = isFirst 
                    ? GetOrderByMethodName(descriptor.SortDirection) 
                    : GetThenByMethodName(descriptor.SortDirection);

                mce = Expression.Call
                (
                    typeof(Queryable),
                    methodName,
                    [parentExpression.GetUnderlyingElementType(), expression.Body.Type],
                    isFirst ? parentExpression : mce!,//mce is not null after the first iteration
                    Expression.Quote(expression)
                );

                isFirst = false;

                static string GetOrderByMethodName(ListSortDirection sortDirection) =>
                    sortDirection == ListSortDirection.Ascending ? "OrderBy" : "OrderByDescending";
                static string GetThenByMethodName(ListSortDirection sortDirection) =>
                    sortDirection == ListSortDirection.Ascending ? "ThenBy" : "ThenByDescending";
            }

            return mce!;//There is at least one sort descriptor (added by using FirstSortableProperty in QueryableExtensionsEx when there are no sort descriptors), so mce will not be null
        }
    }
}
