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
    #region Namespaces

    using Microsoft.OData.Edm;

    #endregion Namespaces

    /// <summary>
    /// Base class for all semantically bound nodes which represent a composable collection of values.
    /// </summary>
    public abstract class EntityCollectionNode : CollectionNode
    {
        /// <summary>
        /// Get the resouce type of a single entity from the collection represented by this node.
        /// </summary>
        public abstract IEdmEntityTypeReference EntityItemType { get; }

        /// <summary>
        /// Get the navigation source that contains this collection.
        /// </summary>
        public abstract IEdmNavigationSource NavigationSource { get; }
    }
}
