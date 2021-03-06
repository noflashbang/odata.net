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

namespace Microsoft.OData.Core.UriParser.Semantic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.OData.Core.UriParser.TreeNodeKinds;
    using Microsoft.OData.Core.UriParser.Visitors;
    using Microsoft.OData.Edm;
    using ODataErrorStrings = Microsoft.OData.Core.Strings;

    /// <summary>
    /// Node to represent a function call that returns a Collection
    /// </summary>
    public sealed class CollectionFunctionCallNode : CollectionNode
    {
        /// <summary>
        /// the name of this function
        /// </summary>
        private readonly string name;

        /// <summary>
        /// the list of operation imports
        /// </summary>
        private readonly ReadOnlyCollection<IEdmFunction> functions;

        /// <summary>
        /// the list of parameters provided to this function
        /// </summary>
        private readonly ReadOnlyCollection<QueryNode> parameters;

        /// <summary>
        /// the individual item type returned by this function
        /// </summary>
        private readonly IEdmTypeReference itemType;

        /// <summary>
        /// the collection type returned by this function
        /// </summary>
        private readonly IEdmCollectionTypeReference returnedCollectionType;

        /// <summary>
        /// The semantically bound parent of this function.
        /// </summary>
        private readonly QueryNode source;

        /// <summary>
        /// Creates a CollectionFunctionCallNode to represent a operation call that returns a collection
        /// </summary>
        /// <param name="name">The name of this operation.</param>
        /// <param name="functions">the list of functions that this node should represent.</param>
        /// <param name="parameters">the list of already bound parameters to this operation</param>
        /// <param name="returnedCollectionType">the type of the collection returned by this operation.</param>
        /// <param name="source">The parent of this CollectionFunctionCallNode.</param>
        /// <exception cref="System.ArgumentNullException">Throws if the provided name is null.</exception>
        /// <exception cref="System.ArgumentNullException">Throws if the provided collection type reference is null.</exception>
        /// <exception cref="System.ArgumentException">Throws if the element type of the provided collection type reference is not a primitive or complex type.</exception>
        public CollectionFunctionCallNode(string name, IEnumerable<IEdmFunction> functions, IEnumerable<QueryNode> parameters, IEdmCollectionTypeReference returnedCollectionType, QueryNode source)
        {
            ExceptionUtils.CheckArgumentNotNull(name, "name");
            ExceptionUtils.CheckArgumentNotNull(returnedCollectionType, "returnedCollectionType");
            this.name = name;
            this.functions = new ReadOnlyCollection<IEdmFunction>(functions == null ? new List<IEdmFunction>() : functions.ToList());
            this.parameters = new ReadOnlyCollection<QueryNode>(parameters == null ? new List<QueryNode>() : parameters.ToList());
            this.returnedCollectionType = returnedCollectionType;
            this.itemType = returnedCollectionType.ElementType();

            if (!this.itemType.IsPrimitive() && !this.itemType.IsComplex() && !this.itemType.IsEnum())
            {
                throw new ArgumentException(ODataErrorStrings.Nodes_CollectionFunctionCallNode_ItemTypeMustBePrimitiveOrComplexOrEnum);
            }

            this.source = source;
        }

        /// <summary>
        /// Gets the name of this function.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets the list of operation imports represeted by this node
        /// </summary>
        public IEnumerable<IEdmFunction> Functions
        {
            get { return this.functions; }
        }

        /// <summary>
        /// Gets the list of parameters to this function
        /// </summary>
        public IEnumerable<QueryNode> Parameters
        {
            get { return this.parameters; }
        }

        /// <summary>
        /// Gets the individual item type returned by this function
        /// </summary>
        public override IEdmTypeReference ItemType
        {
            get { return itemType; }
        }

        /// <summary>
        /// The type of the collection represented by this node.
        /// </summary>
        public override IEdmCollectionTypeReference CollectionType
        {
            get { return this.returnedCollectionType; }
        }

        /// <summary>
        /// Gets the semantically bound parent node of this CollectionFunctionCallNode.
        /// </summary>
        public QueryNode Source
        {
            get { return this.source; }
        }

        /// <summary>
        /// Gets the kind of this node.
        /// </summary>
        internal override InternalQueryNodeKind InternalKind
        {
            get
            {
                return InternalQueryNodeKind.CollectionFunctionCall;
            }
        }

        /// <summary>
        /// Accept a <see cref="QueryNodeVisitor{T}"/> that walks a tree of <see cref="QueryNode"/>s.
        /// </summary>
        /// <typeparam name="T">Type that the visitor will return after visiting this token.</typeparam>
        /// <param name="visitor">An implementation of the visitor interface.</param>
        /// <returns>An object whose type is determined by the type parameter of the visitor.</returns>
        /// <exception cref="System.ArgumentNullException">Throws if the input visitor is null.</exception>
        public override T Accept<T>(QueryNodeVisitor<T> visitor)
        {
            ExceptionUtils.CheckArgumentNotNull(visitor, "visitor");
            return visitor.Visit(this);
        }
    }
}
