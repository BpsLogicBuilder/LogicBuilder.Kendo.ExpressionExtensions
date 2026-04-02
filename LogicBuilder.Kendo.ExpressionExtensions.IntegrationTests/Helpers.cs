using Contoso.Data.Entities;
using Contoso.Domain;
using Kendo.Mvc;
using Kendo.Mvc.Infrastructure;
using Kendo.Mvc.UI;
using LogicBuilder.Data;
using LogicBuilder.Domain;
using LogicBuilder.EntityFrameworkCore.SqlServer.Repositories;
using LogicBuilder.Expressions.Utils.Expansions;
using LogicBuilder.Expressions.Utils.ExpressionBuilder;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Collection;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Lambda;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Operand;
using LogicBuilder.Kendo.ExpressionExtensions.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests
{
    internal static class Helpers
    {
        public static async Task<DataSourceResult> GetData<TModel, TData>(this DataRequest request, IContextRepository contextRepository)
            where TModel : BaseModelClass
            where TData : BaseDataClass
        {
            return await request.Options.CreateDataSourceRequest().GetDataSourceResult<TModel, TData>
            (
                contextRepository,
                request.SelectExpandDefinition
            );
        }

        public static async Task<IEnumerable<dynamic>> GetDynamicSelect<TModel, TData>(this DataRequest request, IContextRepository contextRepository)
            where TModel : BaseModelClass
            where TData : BaseDataClass
        {
            var parametersDictionary = new Dictionary<string, ParameterExpression>();
            IExpressionPart selectOperator = new SelectOperator
            (
                parametersDictionary,
                new ParameterOperator(parametersDictionary, "q"),
                new MemberInitOperator
                (
                    request.Selects.ToDictionary<KeyValuePair<string, string>, string, IExpressionPart>
                    (
                        s => s.Key,
                        s => new MemberSelectorOperator(s.Value, new ParameterOperator(parametersDictionary, "s"))
                    )
                ),
                "s"
            );

            if (request.Distinct)
                selectOperator = new DistinctOperator(selectOperator);

            SelectorLambdaOperator selectorLambdaOperator = new
            (
                parametersDictionary,
                selectOperator,
                typeof(IQueryable<TModel>),
                typeof(IEnumerable<dynamic>),
                "q"
            );

            Expression<Func<IQueryable<TModel>, IEnumerable<dynamic>>> expression = (Expression<Func<IQueryable<TModel>, IEnumerable<dynamic>>>)selectorLambdaOperator.Build();

            return await contextRepository.QueryAsync<TModel, TData, IEnumerable<dynamic>, IEnumerable<dynamic>>
            (
                expression,
                request.SelectExpandDefinition
            );
        }

        public static async Task<TModel> GetSingle<TModel, TData>(this DataRequest request, IContextRepository contextRepository)
            where TModel : BaseModelClass
            where TData : BaseDataClass
        {

            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> exp = request.Options.CreateDataSourceRequest().CreateUngroupedQueryableExpression<TModel>();

            return 
            (
                await contextRepository.QueryAsync<TModel, TData, IQueryable<TModel>, IQueryable<TData>>
                (
                    exp,
                    request.SelectExpandDefinition
                )
            ).Single();
        }

        public static DataSourceRequest CreateDataSourceRequest(this DataSourceRequestOptions req)
        {
            var request = new DataSourceRequest();

            if (req.Sort != null)
                request.Sorts = DataSourceDescriptorSerializer.Deserialize<SortDescriptor>(req.Sort);


            request.Page = req.Page;

            request.PageSize = req.PageSize;

            if (req.Filter != null)
                request.Filters = FilterDescriptorFactory.Create(req.Filter);

            if (req.Group != null)
                request.Groups = DataSourceDescriptorSerializer.Deserialize<GroupDescriptor>(req.Group);

            if (req.Aggregate != null)
                request.Aggregates = DataSourceDescriptorSerializer.Deserialize<AggregateDescriptor>(req.Aggregate);

            return request;
        }

        /// <summary>
        /// Get DataSource Result
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="request"></param>
        /// <param name="contextRepository"></param>
        /// <param name="selectExpandDefinition"></param>
        /// <returns></returns>
        public static async Task<DataSourceResult> GetDataSourceResult<TModel, TData>(this DataSourceRequest request, IContextRepository contextRepository, SelectExpandDefinition selectExpandDefinition = null)
            where TModel : BaseModel
            where TData : BaseData
            => request.Groups != null && request.Groups.Count > 0
                ? await request.GetGroupedDataSourceResult<TModel, TData>(contextRepository, request.Aggregates != null && request.Aggregates.Count > 0, selectExpandDefinition)
                : await request.GetUngroupedDataSourceResult<TModel, TData>(contextRepository, request.Aggregates != null && request.Aggregates.Count > 0, selectExpandDefinition);

        private static async Task<DataSourceResult> GetUngroupedDataSourceResult<TModel, TData>(this DataSourceRequest request, IContextRepository contextRepository, bool getAggregates, SelectExpandDefinition selectExpandDefinition = null)
            where TModel : BaseModel
            where TData : BaseData
        {
            Expression<Func<IQueryable<TModel>, int>> totalExp = QueryableExtensionsEx.CreateTotalExpression<TModel>(request);
            Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> ungroupedExp = QueryableExtensionsEx.CreateUngroupedQueryableExpression<TModel>(request);

            return new DataSourceResult
            {
                Data = await contextRepository.QueryAsync<TModel, TData, IQueryable<TModel>, IQueryable<TData>>(ungroupedExp, selectExpandDefinition),
                AggregateResults = getAggregates
                                    ? (await GetAggregateFunctionsGroup()).GetAggregateResults(request.Aggregates.SelectMany(a => a.Aggregates))
                                    : null,
                Total = await contextRepository.QueryAsync<TModel, TData, int, int>(totalExp, selectExpandDefinition)
            };

            async Task<AggregateFunctionsGroup> GetAggregateFunctionsGroup()
            {
                var aggrewgatexpressions = request.CreateAggregatesQueryExpressions<TModel>();
                IQueryable<TModel> pagedQuery = await contextRepository.QueryAsync<TModel, TData, IQueryable<TModel>, IQueryable<TData>>(aggrewgatexpressions.QueryableExpression, selectExpandDefinition);
                return aggrewgatexpressions.AggregateExpression.Compile()(pagedQuery);
            }
        }

        private static async Task<DataSourceResult> GetGroupedDataSourceResult<TModel, TData>(this DataSourceRequest request, IContextRepository contextRepository, bool getAggregates, SelectExpandDefinition selectExpandDefinition = null)
            where TModel : BaseModel
            where TData : BaseData
        {
            Expression<Func<IQueryable<TModel>, int>> totalExp = QueryableExtensionsEx.CreateTotalExpression<TModel>(request);

            return new DataSourceResult
            {
                Data = await GetData(),
                AggregateResults = getAggregates
                                    ? (await GetAggregateFunctionsGroup()).GetAggregateResults(request.Aggregates.SelectMany(a => a.Aggregates))
                                    : null,
                Total = await contextRepository.QueryAsync<TModel, TData, int, int>(totalExp, selectExpandDefinition)
            };

            async Task<IEnumerable> GetData()
            {
                var groupByExpressions = request.CreateGroupedByQueryExpressions<TModel>();
                IQueryable<TModel> pagedQuery = await contextRepository.QueryAsync<TModel, TData, IQueryable<TModel>, IQueryable<TData>>(groupByExpressions.PagingExpression, selectExpandDefinition);
                return groupByExpressions.GroupByExpression.Compile()(pagedQuery);
            }

            async Task<AggregateFunctionsGroup> GetAggregateFunctionsGroup()
            {
                var aggrewgatexpressions = request.CreateAggregatesQueryExpressions<TModel>();
                IQueryable<TModel> pagedQuery = await contextRepository.QueryAsync<TModel, TData, IQueryable<TModel>, IQueryable<TData>>(aggrewgatexpressions.QueryableExpression, selectExpandDefinition);
                return aggrewgatexpressions.AggregateExpression.Compile()(pagedQuery);
            }
        }
    }
}
