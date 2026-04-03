using Kendo.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LogicBuilder.Kendo.ExpressionExtensions
{
    public class GroupByQueryExpressions<TModel>(Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> pagingExpression, Expression<Func<IQueryable<TModel>, IEnumerable<AggregateFunctionsGroup>>> groupByExpression)
    {
        public Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> PagingExpression { get; set; } = pagingExpression;
        public Expression<Func<IQueryable<TModel>, IEnumerable<AggregateFunctionsGroup>>> GroupByExpression { get; set; } = groupByExpression;
    }
}
