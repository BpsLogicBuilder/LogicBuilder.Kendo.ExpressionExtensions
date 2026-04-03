using Kendo.Mvc.Infrastructure;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace LogicBuilder.Kendo.ExpressionExtensions
{
    public class AggregatesQueryExpressions<TModel>(Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> queryableExpression, Expression<Func<IQueryable<TModel>, AggregateFunctionsGroup>> aggregateExpression)
    {
        public Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> QueryableExpression { get; set; } = queryableExpression;
        public Expression<Func<IQueryable<TModel>, AggregateFunctionsGroup>> AggregateExpression { get; set; } = aggregateExpression;
    }
}
