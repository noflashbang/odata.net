//   Copyright 2011 Microsoft Corporation
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace Microsoft.Data.OData.Query.SyntacticAst
{
    using Microsoft.Data.OData.Query.SemanticAst;

    #region Namespaces
    #endregion Namespaces

    /// <summary>
    /// Lexical token representing the last segment in a path.
    /// </summary>
    internal sealed class EndPathToken : PathToken
    {
        /// <summary>
        /// The Identifier of the property to access.
        /// </summary>
        private readonly string identifier;

        /// <summary>
        /// The NextToken token to access the property on.
        /// If this is null, then the property access has no NextToken. That usually means to access the property
        /// on the implicit parameter for the expression, the result on which the expression is being applied.
        /// </summary>
        private QueryToken nextToken;

        /// <summary>
        /// Create a EndPathToken given the Identifier and the NextToken (if any)
        /// </summary>
        /// <param name="identifier">The Identifier of the property to access.</param>
        /// <param name="nextToken">The NextToken token to access the property on. </param>
        public EndPathToken(string identifier, QueryToken nextToken)
        {
            ExceptionUtils.CheckArgumentStringNotNullOrEmpty(identifier, "Identifier");

            this.identifier = identifier;
            this.nextToken = nextToken;
        }

        /// <summary>
        /// The kind of the query token.
        /// </summary>
        public override QueryTokenKind Kind
        {
            get { return QueryTokenKind.EndPath; }
        }

        /// <summary>
        /// The NextToken token to access the property on.
        /// If this is null, then the property access has no NextToken. That usually means to access the property
        /// on the implicit parameter for the expression, the result on which the expression is being applied.
        /// </summary>
        public override QueryToken NextToken
        {
            get { return this.nextToken; }
            set { this.nextToken = value; }
        }

        /// <summary>
        /// The Identifier of the property to access.
        /// </summary>
        public override string Identifier
        {
            get { return this.identifier; }
        }

        /// <summary>
        /// Accept a <see cref="ISyntacticTreeVisitor{T}"/> to walk a tree of <see cref="QueryToken"/>s.
        /// </summary>
        /// <typeparam name="T">Type that the visitor will return after visiting this token.</typeparam>
        /// <param name="visitor">An implementation of the visitor interface.</param>
        /// <returns>An object whose type is determined by the type parameter of the visitor.</returns>
        public override T Accept<T>(ISyntacticTreeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}