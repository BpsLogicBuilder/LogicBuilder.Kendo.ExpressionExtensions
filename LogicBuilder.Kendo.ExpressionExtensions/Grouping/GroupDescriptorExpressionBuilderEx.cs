using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.Infrastructure;
using Kendo.Mvc.Infrastructure.Implementation;
using LogicBuilder.Kendo.ExpressionExtensions.Expressions;
using LogicBuilder.Kendo.ExpressionExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LogicBuilder.Kendo.ExpressionExtensions.Grouping
{
    internal class GroupDescriptorExpressionBuilderEx(Expression queryable, GroupDescriptor groupDescriptor, GroupDescriptorExpressionBuilderEx? childBuilder, Expression notPagedExpression) : GroupDescriptorExpressionBuilderBaseEx(queryable)
    {
        private readonly GroupDescriptor groupDescriptor = groupDescriptor;
        private readonly GroupDescriptorExpressionBuilderEx? childBuilder = childBuilder;
        private readonly Expression notPagedExpression = notPagedExpression;

        private ParameterExpression? groupingParameterExpression;
        private Expression? aggregateParameterExpression;

        [MemberNotNullWhen(true, nameof(HasSubgroups))]
        public GroupDescriptorExpressionBuilderEx? ChildBuilder
        {
            get
            {
                return this.childBuilder;
            }
        }

        public GroupDescriptor GroupDescriptor
        {
            get
            {
                return this.groupDescriptor;
            }
        }

        public bool HasSubgroups
        {
            get
            {
                return this.childBuilder != null;
            }
        }

        protected override ListSortDirection? SortDirection
        {
            get
            {
                return this.groupDescriptor.SortDirection;
            }
        }

        private ParameterExpression GroupingParameterExpression
        {
            get
            {
                if (this.groupingParameterExpression == null)
                {
                    LambdaExpression groupByExpression = this.CreateGroupByExpression();
                    Type groupingType = typeof(IGrouping<,>).MakeGenericType(groupByExpression.Body.Type, this.ItemType);

                    this.groupingParameterExpression = Expression.Parameter(groupingType, "group" + GetHashCode());
                }

                return this.groupingParameterExpression;
            }
        }

        private Expression AggregateParameterExpression
        {
            get
            {
                if (aggregateParameterExpression == null)
                {
                    var groupItemsFilterExpression = CreateChildItemsFilterExpression();
                    Expression items = notPagedExpression;

                    ParentBuilder?.CreateChildItemsFilterExpressionFromRecursive()
                               .Each(expression =>
                               {
                                   items = items.Where(expression);
                               });

                    items = items.Where(groupItemsFilterExpression);

                    aggregateParameterExpression = items;
                }

                return aggregateParameterExpression;
            }
        }

        public GroupDescriptorExpressionBuilderEx? ParentBuilder
        {
            get;
            set;
        }

        public GroupDescriptorExpressionBuilderEx(Expression queryable, GroupDescriptor groupDescriptor)
            : this(queryable, groupDescriptor, null, queryable)
        {
            this.groupDescriptor = groupDescriptor;
        }

        protected override LambdaExpression CreateGroupByExpression()
        {
            var memberAccessBuilder = ExpressionBuilderFactoryEx.MemberAccess(this.Queryable, this.groupDescriptor.MemberType, this.groupDescriptor.Member);
            memberAccessBuilder.ParameterExpression = this.ParameterExpression;
            return memberAccessBuilder.CreateLambdaExpression();
        }

        protected override LambdaExpression CreateOrderByExpression()
        {
            var keyPropertyExpression = Expression.Property(this.GroupingParameterExpression, "Key");
            LambdaExpression memberExpression = Expression.Lambda(keyPropertyExpression, GroupingParameterExpression);

            return memberExpression;
        }

        protected override LambdaExpression CreateSelectExpression()
        {
            if (HasSubgroups)
            {
                childBuilder!.ParentBuilder = this;//We know child builder is not null since HasSubgroups is true.
            }
            return Expression.Lambda(this.CreateSelectBodyExpression(), this.GroupingParameterExpression);
        }

        private MemberInitExpression CreateSelectBodyExpression()
        {
            var newGroupExpression = Expression.New(typeof(AggregateFunctionsGroup));
            var memberBindings = this.CreateMemberBindings();

            return Expression.MemberInit(newGroupExpression, memberBindings);
        }

        protected virtual IEnumerable<MemberBinding> CreateMemberBindings()
        {
            yield return this.CreateKeyMemberBinding();
            yield return this.CreateCountMemberBinding();
            yield return this.CreateHasSubgroupsMemberBinding();
            yield return this.CreateFieldNameMemberBinding();

            if (groupDescriptor.AggregateFunctions.Count > 0)
            {
                yield return this.CreateAggregateFunctionsProjectionMemberBinding();
            }
            yield return this.CreateItemsMemberBinding();
        }

        protected MemberBinding CreateItemsMemberBinding()
        {
            PropertyInfo itemsPropertyInfo = typeof(AggregateFunctionsGroup).GetProperty(nameof(AggregateFunctionsGroup.Items))!;//We know this property exists since we are creating the expression for it.
            Expression itemsExpression = this.CreateItemsExpression();

            return Expression.Bind(itemsPropertyInfo, itemsExpression);
        }

        private Expression CreateItemsExpression()
        {
            if (HasSubgroups)
            {
                return this.CreateItemsExpressionFromChildBuilder();
            }

            return this.GroupingParameterExpression;
        }

        private MethodCallExpression CreateItemsExpressionFromChildBuilder()
        {
            var groupItemsFilterExpression = CreateChildItemsFilterExpression();

            Expression groupItems = this.Queryable.Where(groupItemsFilterExpression);
            childBuilder!.Queryable = groupItems;//We know child builder is not null since HasSubgroups is true.

            return childBuilder.CreateExpression();
        }

        public IEnumerable<LambdaExpression> CreateChildItemsFilterExpressionFromRecursive()
        {
            var result = new List<LambdaExpression> {
               CreateChildItemsFilterExpression()
            };

            if (ParentBuilder != null)
            {
                result.AddRange(ParentBuilder.CreateChildItemsFilterExpressionFromRecursive());
            }

            return result;
        }

        public LambdaExpression CreateChildItemsFilterExpression()
        {
            LambdaExpression groupByExpression = this.CreateGroupByExpression();
            Expression keyPropertyExpression = Expression.Property(GroupingParameterExpression, "Key");
            Expression body = Expression.Equal(groupByExpression.Body, keyPropertyExpression);

            return Expression.Lambda(body, ParameterExpression);
        }

        protected MemberBinding CreateKeyMemberBinding()
        {
            PropertyInfo keyPropertyInfo = typeof(AggregateFunctionsGroup).GetProperty(nameof(AggregateFunctionsGroup.Key))!;//Known property
            Expression keyPropertyExpression = Expression.Property(GroupingParameterExpression, "Key");

            // Our Key property is of type object so we need to box if the value is ValueType.
            // EF did not support convert so did not call it.
            // Note: We can fix all this if our group is generic type similar to IGrouping<TKey, TElement>
            if (keyPropertyExpression.Type.IsValueType())
            {
                keyPropertyExpression = Expression.Convert(keyPropertyExpression, typeof(object));
            }

            return Expression.Bind(keyPropertyInfo, keyPropertyExpression);
        }

        protected MemberBinding CreateCountMemberBinding()
        {
            PropertyInfo itemCountPropertyInfo = typeof(AggregateFunctionsGroup).GetProperty(nameof(AggregateFunctionsGroup.ItemCount))!;//Known property

            Expression countMethodCallExpression =
                Expression.Call(typeof(Enumerable), "Count", [this.ItemType], GroupingParameterExpression);

            return Expression.Bind(itemCountPropertyInfo, countMethodCallExpression);
        }

        protected MemberBinding CreateFieldNameMemberBinding()
        {
            PropertyInfo memberPropertyInfo = typeof(AggregateFunctionsGroup).GetProperty(nameof(AggregateFunctionsGroup.Member))!;//Known property
            Expression memberExpression = Expression.Constant(GroupDescriptor.Member ?? "");

            return Expression.Bind(memberPropertyInfo, memberExpression);
        }

        protected MemberBinding CreateHasSubgroupsMemberBinding()
        {
            PropertyInfo hasSubgroupsPropertyInfo = typeof(AggregateFunctionsGroup).GetProperty(nameof(AggregateFunctionsGroup.HasSubgroups))!;//Known property
            Expression hasSubgroupsExpression = Expression.Constant(this.HasSubgroups);

            return Expression.Bind(hasSubgroupsPropertyInfo, hasSubgroupsExpression);
        }

        protected MemberBinding CreateAggregateFunctionsProjectionMemberBinding()
        {
            PropertyInfo projectionPropertyInfo = typeof(AggregateFunctionsGroup).GetProperty(nameof(AggregateFunctionsGroup.AggregateFunctionsProjection))!;//Known property
            Expression projectionInitExpression = this.CreateProjectionInitExpression();

            return Expression.Bind(projectionPropertyInfo, projectionInitExpression);
        }

        private MemberInitExpression CreateProjectionInitExpression()
        {
            var projectionPropertyValueExpressions = this.ProjectionPropertyValueExpressions().ToList();
            var newProjectionExpression = this.CreateProjectionNewExpression(projectionPropertyValueExpressions);
            var projectionMemberBindings = this.CreateProjectionMemberBindings(newProjectionExpression.Type, projectionPropertyValueExpressions);

            return Expression.MemberInit(newProjectionExpression, projectionMemberBindings);
        }

        private IEnumerable<Expression> ProjectionPropertyValueExpressions()
        {
            return this.groupDescriptor.AggregateFunctions.Select(f => f.CreateAggregateExpression(AggregateParameterExpression, Options.LiftMemberAccessToNull));
        }

        private NewExpression CreateProjectionNewExpression(IEnumerable<Expression> propertyValuesExpressions)
        {
            var properties = this.groupDescriptor.AggregateFunctions.Consolidate(
                propertyValuesExpressions, (f, e) => new DynamicProperty(f.FunctionName, e.Type));
            var projectionType = ClassFactory.Instance.GetDynamicClass(properties);

            return Expression.New(projectionType);
        }

        private IEnumerable<System.Linq.Expressions.MemberBinding> CreateProjectionMemberBindings(Type projectionType, IEnumerable<Expression> propertyValuesExpressions)
        {
            return
                this.groupDescriptor.AggregateFunctions.Consolidate(
                    propertyValuesExpressions, (f, e) => Expression.Bind(projectionType.GetProperty(f.FunctionName)!/*Invalid function names are filtered out by Kendo.Mvc.*/, e)).Cast<MemberBinding>();
        }
    }
}
