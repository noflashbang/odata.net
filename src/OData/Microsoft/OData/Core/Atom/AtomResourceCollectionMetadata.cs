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

namespace Microsoft.OData.Core.Atom
{
    /// <summary>
    /// Atom metadata description for a collection (in a workspace).
    /// </summary>
    public sealed class AtomResourceCollectionMetadata
    {
        /// <summary>Gets or sets the title of the collection.</summary>
        /// <returns>The title of the collection.</returns>
        public AtomTextConstruct Title
        {
            get;
            set;
        }

        /// <summary>Gets or sets the accept range of media types for this collection.</summary>
        /// <returns>The accept range of media types for this collection.</returns>
        public string Accept
        {
            get;
            set;
        }

        /// <summary>Gets or sets the categories for this collection.</summary>
        /// <returns>The categories for this collection.</returns>
        public AtomCategoriesMetadata Categories
        {
            get;
            set;
        }
    }
}
