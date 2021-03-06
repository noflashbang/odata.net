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
    using System;

    /// <summary>Indicates that a class that is an entity type has a related named binary stream.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class NamedStreamAttribute : Attribute
    {
        /// <summary>Creates a new instance of the <see cref="T:Microsoft.OData.Client.NamedStreamAttribute" /> class.</summary>
        /// <param name="name">The name of a binary stream that belongs to the attributed entity.</param>
        public NamedStreamAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>The name of a binary stream that belongs to the attributed entity.</summary>
        /// <returns>The name of the binary stream.</returns>
        public string Name
        {
            get;
            private set;
        }
    }
}
