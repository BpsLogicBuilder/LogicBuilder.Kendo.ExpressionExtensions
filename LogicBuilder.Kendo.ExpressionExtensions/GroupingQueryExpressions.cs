using Kendo.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LogicBuilder.Kendo.ExpressionExtensions
{
    public class GroupingQueryExpressions<TModel>(Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> ungroupedUnpagedExpression, Expression<Func<IQueryable<TModel>, IEnumerable<AggregateFunctionsGroup>>> pagedGroupedExpression)
    {
        public Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> UngroupedUnpagedExpression { get; set; } = ungroupedUnpagedExpression;
        public Expression<Func<IQueryable<TModel>, IEnumerable<AggregateFunctionsGroup>>> PagedGroupedExpression { get; set; } = pagedGroupedExpression;
    }
}
