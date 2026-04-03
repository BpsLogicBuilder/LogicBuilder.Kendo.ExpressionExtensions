using Kendo.Mvc;
using Kendo.Mvc.Infrastructure.Implementation.Expressions;
using LogicBuilder.Kendo.ExpressionExtensions.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LogicBuilder.Kendo.ExpressionExtensions.Grouping
{
    internal class GroupDescriptorCollectionExpressionBuilderEx(Expression expression, IEnumerable<GroupDescriptor> groupDescriptors, Expression notPagedData) : ExpressionBuilderBase(expression.GetUnderlyingElementType())
    {
        private readonly Expression queryable = expression;
        private readonly IEnumerable<GroupDescriptor> groupDescriptors = groupDescriptors;
        private readonly Expression notPagedData = notPagedData;

        public Expression CreateExpression()
        {
            GroupDescriptorExpressionBuilderEx? childBuilder = null;
            foreach (GroupDescriptor groupDescriptor in groupDescriptors.Reverse())
            {
                var builder = new GroupDescriptorExpressionBuilderEx(this.queryable, groupDescriptor, childBuilder, notPagedData);
                builder.Options.LiftMemberAccessToNull = false;
                childBuilder = builder;
            }

            if (childBuilder != null)
            {
                return childBuilder.CreateExpression();
            }

            return queryable;
        }
    }
}
