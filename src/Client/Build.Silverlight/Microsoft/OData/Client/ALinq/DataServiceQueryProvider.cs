//   OData .NET Libraries
//   Copyright (c) Microsoft Corporation. All rights reserved.  
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace Microsoft.OData.Client
{
    #region Namespaces

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    #endregion Namespaces

    /// <summary>
    /// QueryProvider implementation
    /// </summary>
    public sealed class DataServiceQueryProvider : IQueryProvider
    {
        /// <summary>DataServiceContext for query provider</summary>
        internal readonly DataServiceContext Context;

        /// <summary>Constructs a query provider based on the context passed in </summary>
        /// <param name="context">The context for the query provider</param>
        internal DataServiceQueryProvider(DataServiceContext context)
        {
            this.Context = context;
        }

        #region IQueryProvider implementation

        /// <summary>Factory method for creating DataServiceOrderedQuery based on expression </summary>
        /// <param name="expression">The expression for the new query</param>
        /// <returns>new DataServiceQuery</returns>
        public IQueryable CreateQuery(Expression expression)
        {
            Util.CheckArgumentNull(expression, "expression");
            Type et = TypeSystem.GetElementType(expression.Type);
            Type qt = typeof(DataServiceQuery<>.DataServiceOrderedQuery).MakeGenericType(et);
            object[] args = new object[] { expression, this };

            ConstructorInfo ci = qt.GetInstanceConstructor(
                false /*isPublic*/,
                new Type[] { typeof(Expression), typeof(DataServiceQueryProvider) });

            return (IQueryable)Util.ConstructorInvoke(ci, args);
        }

        /// <summary>Factory method for creating DataServiceOrderedQuery based on expression </summary>
        /// <typeparam name="TElement">generic type</typeparam>
        /// <param name="expression">The expression for the new query</param>
        /// <returns>new DataServiceQuery</returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            Util.CheckArgumentNull(expression, "expression");
            return new DataServiceQuery<TElement>.DataServiceOrderedQuery(expression, this);
        }

        /// <summary>Creates and executes a DataServiceQuery based on the passed in expression</summary>
        /// <param name="expression">The expression for the new query</param>
        /// <returns>the results</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        public object Execute(Expression expression)
        {
            Util.CheckArgumentNull(expression, "expression");

            MethodInfo mi = typeof(DataServiceQueryProvider).GetMethod("ReturnSingleton", false /*isPublic*/, false /*isStatic*/);
            return mi.MakeGenericMethod(expression.Type).Invoke(this, new object[] { expression });
        }

        /// <summary>Creates and executes a DataServiceQuery based on the passed in expression</summary>
        /// <typeparam name="TResult">generic type</typeparam>
        /// <param name="expression">The expression for the new query</param>
        /// <returns>the results</returns>
        public TResult Execute<TResult>(Expression expression)
        {
            Util.CheckArgumentNull(expression, "expression");
            return ReturnSingleton<TResult>(expression);
        }

        #endregion

        /// <summary>Creates and executes a DataServiceQuery based on the passed in expression which results a single value</summary>
        /// <typeparam name="TElement">generic type</typeparam>
        /// <param name="expression">The expression for the new query</param>
        /// <returns>single valued results</returns>
        internal TElement ReturnSingleton<TElement>(Expression expression)
        {
            IQueryable<TElement> query = new DataServiceQuery<TElement>.DataServiceOrderedQuery(expression, this);

            MethodCallExpression mce = expression as MethodCallExpression;
            Debug.Assert(mce != null, "mce != null");

            SequenceMethod sequenceMethod;
            if (ReflectionUtil.TryIdentifySequenceMethod(mce.Method, out sequenceMethod))
            {
                switch (sequenceMethod)
                {
                    case SequenceMethod.Single:
                        return query.AsEnumerable().Single();
                    case SequenceMethod.SingleOrDefault:
                        return query.AsEnumerable().SingleOrDefault();
                    case SequenceMethod.First:
                        return query.AsEnumerable().First();
                    case SequenceMethod.FirstOrDefault:
                        return query.AsEnumerable().FirstOrDefault();
#if !ASTORIA_LIGHT && !PORTABLELIB
                    case SequenceMethod.LongCount:
                    case SequenceMethod.Count:
                        return (TElement)Convert.ChangeType(((DataServiceQuery<TElement>)query).GetQuerySetCount(this.Context), typeof(TElement), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
#endif
                    default:
                        throw Error.MethodNotSupported(mce);
                }
            }

            // Should never get here - should be caught by expression compiler.
            Debug.Assert(false, "Not supported singleton operator not caught by Resource Binder");
            throw Error.MethodNotSupported(mce);
        }

        /// <summary>Builds the Uri for the expression passed in.</summary>
        /// <param name="e">The expression to translate into a Uri</param>
        /// <returns>Query components</returns>
        internal QueryComponents Translate(Expression e)
        {
            Uri uri;
            Version version;
            bool addTrailingParens = false;
            Dictionary<Expression, Expression> normalizerRewrites = null;

            // short cut analysis if just a resource set or singleton resource.
            // note - to be backwards compatible with V1, will only append trailing () for queries
            // that include more then just a resource set.
            if (!(e is QueryableResourceExpression))
            {
                normalizerRewrites = new Dictionary<Expression, Expression>(ReferenceEqualityComparer<Expression>.Instance);
                e = Evaluator.PartialEval(e);
                e = ExpressionNormalizer.Normalize(e, normalizerRewrites);
                e = ResourceBinder.Bind(e, this.Context);
                addTrailingParens = true;
            }

            UriWriter.Translate(this.Context, addTrailingParens, e, out uri, out version);
            ResourceExpression re = e as ResourceExpression;
            Type lastSegmentType = re.Projection == null ? re.ResourceType : re.Projection.Selector.Parameters[0].Type;
            LambdaExpression selector = re.Projection == null ? null : re.Projection.Selector;
            return new QueryComponents(uri, version, lastSegmentType, selector, normalizerRewrites); 
        }
    }
}
