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

#if ASTORIA_CLIENT
namespace Microsoft.OData.Client.ALinq.UriParser
#else
namespace Microsoft.OData.Core.UriParser.Visitors
#endif
{
    using Microsoft.OData.Core.UriParser.Syntactic;

    /// <summary>
    /// Visitor interface for walking the Path Tree.
    /// </summary>
    /// <typeparam name="T">Return type for the visitor methods on this visitor.</typeparam>
    internal interface IPathSegmentTokenVisitor<T>
    {
        /// <summary>
        /// Visit an SystemToken
        /// </summary>
        /// <param name="tokenIn">The SystemToken to visit</param>
        /// <returns>A user defined class</returns>
        T Visit(SystemToken tokenIn);

        /// <summary>
        /// Visit an NonSystemToken
        /// </summary>
        /// <param name="tokenIn">The NonSystemToken to visit</param>
        /// <returns>A user defined class</returns>
        T Visit(NonSystemToken tokenIn);
    }

    /// <summary>
    /// Visitor interface for walking the Path Tree.
    /// </summary>
    internal interface IPathSegmentTokenVisitor
    {
        /// <summary>
        /// Visit an SystemToken
        /// </summary>
        /// <param name="tokenIn">The SystemToken to visit</param>
        void Visit(SystemToken tokenIn);

        /// <summary>
        /// Visit an NonSystemToken
        /// </summary>
        /// <param name="tokenIn">The NonSystemToken to visit</param>
        void Visit(NonSystemToken tokenIn);
    }
}
