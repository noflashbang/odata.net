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

namespace Microsoft.OData.Core
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Microsoft.OData.Edm;
    #endregion Namespaces

    /// <summary>
    /// Builder class to construct the context url for the various payload kinds.
    /// </summary>
    internal sealed class ODataContextUriBuilder
    {
        /// <summary>
        /// The base context Url
        /// </summary>
        private readonly Uri baseContextUrl;

        /// <summary>
        /// Whether to throw exception when error(missing fields) occurs.
        /// </summary>
        private readonly bool throwIfMissingInfo;

        /// <summary>
        /// Stores the validation method mapping for supported payload kind.
        /// </summary>
        private static readonly Dictionary<ODataPayloadKind, Action<ODataContextUrlInfo>> ValidationDictionary = new Dictionary<ODataPayloadKind, Action<ODataContextUrlInfo>>(EqualityComparer<ODataPayloadKind>.Default)
        {
            { ODataPayloadKind.ServiceDocument,         null },
            { ODataPayloadKind.EntityReferenceLink,     null },
            { ODataPayloadKind.EntityReferenceLinks,    null },
            { ODataPayloadKind.IndividualProperty,      ValidateResourcePath },
            { ODataPayloadKind.Collection,              ValidateCollectionType },
            { ODataPayloadKind.Property,                ValidateType },
            { ODataPayloadKind.Entry,                   ValidateNavigationPath },
            { ODataPayloadKind.Feed,                    ValidateNavigationPath },
            { ODataPayloadKind.Delta,                   ValidateDelta },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataContextUriBuilder"/> class.
        /// </summary>
        /// <param name="baseContextUrl">Base context URI.</param>
        /// <param name="throwIfMissingInfo">Indicates whether to throw exception when error(e.g. required fields missing) occurs.</param>
        private ODataContextUriBuilder(Uri baseContextUrl, bool throwIfMissingInfo)
        {
            this.baseContextUrl = baseContextUrl;
            this.throwIfMissingInfo = throwIfMissingInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataContextUriBuilder"/> class.
        /// </summary>
        /// <param name="baseContextUrl">Base context URI.</param>
        /// <param name="throwIfMissingInfo">Indicates whether to throw exception when error(e.g. required fields missing) occurs.</param>
        /// <returns>The context uri builder for use.</returns>
        internal static ODataContextUriBuilder Create(Uri baseContextUrl, bool throwIfMissingInfo)
        {
            if (baseContextUrl == null && throwIfMissingInfo)
            {
                throw new ODataException(Strings.ODataOutputContext_MetadataDocumentUriMissing);
            }

            return new ODataContextUriBuilder(baseContextUrl, throwIfMissingInfo);
        }

        /// <summary>
        /// Create context URL from ODataPayloadKind and ODataContextUrlInfo.
        /// BUG 1659341: should make the context uri correct for null primitive / null enum value / normal enum value
        /// ODataEnumValue is allowed to have null or arbitrary TypeName, but the output ContextUri must have correct type name.
        /// </summary>
        /// <param name="payloadKind">The ODataPayloadKind for the context URI.</param>
        /// <param name="contextInfo">The ODataContextUrlInfo to be used.</param>
        /// <returns>The generated context url.</returns>
        internal Uri BuildContextUri(ODataPayloadKind payloadKind, ODataContextUrlInfo contextInfo = null)
        {
            if (this.baseContextUrl == null)
            {
                return null;
            }

            Action<ODataContextUrlInfo> verifyAction;
            if (ValidationDictionary.TryGetValue(payloadKind, out verifyAction))
            {
                if (verifyAction != null && throwIfMissingInfo)
                {
                    Debug.Assert(contextInfo != null, "contextInfo != null");
                    verifyAction(contextInfo);
                }
            }
            else
            {
                throw new ODataException(Strings.ODataContextUriBuilder_UnsupportedPayloadKind(payloadKind.ToString()));
            }

            switch (payloadKind)
            {
                case ODataPayloadKind.ServiceDocument:
                    return this.baseContextUrl;
                case ODataPayloadKind.EntityReferenceLink:
                    return new Uri(this.baseContextUrl, ODataConstants.SingleEntityReferencesContextUrlSegment);
                case ODataPayloadKind.EntityReferenceLinks:
                    return new Uri(this.baseContextUrl, ODataConstants.CollectionOfEntityReferencesContextUrlSegment);
            }

            return CreateFromContextUrlInfo(contextInfo);
        }

        /// <summary>
        /// Create context URL from ODataContextUrlInfo.
        /// </summary>
        /// <param name="info">The ODataContextUrlInfo to be used.</param>
        /// <returns>The generated context url.</returns>
        private Uri CreateFromContextUrlInfo(ODataContextUrlInfo info)
        {
            StringBuilder builder = new StringBuilder();

            // #
            builder.Append(ODataConstants.ContextUriFragmentIndicator);

            if (!string.IsNullOrEmpty(info.ResourcePath))
            {
                builder.Append(info.ResourcePath);
            }
            else if (!string.IsNullOrEmpty(info.NavigationPath))
            {
                // #ContainerName.NavigationSourceName
                builder.Append(info.NavigationPath);

                if (info.DeltaKind == ODataDeltaKind.None || info.DeltaKind == ODataDeltaKind.Feed || info.DeltaKind == ODataDeltaKind.Entry)
                {
                    // #ContainerName.NavigationSourceName  ==>  #ContainerName.NavigationSourceName/Namespace.DerivedTypeName
                    if (!string.IsNullOrEmpty(info.TypeCast))
                    {
                        builder.Append(ODataConstants.UriSegmentSeparatorChar);
                        builder.Append(info.TypeCast);
                    }

                    // #ContainerName.NavigationSourceName  ==>  #ContainerName.NavigationSourceName(selectedPropertyList)
                    if (!string.IsNullOrEmpty(info.QueryClause))
                    {
                        builder.Append(info.QueryClause);
                    }
                }

                switch (info.DeltaKind)
                {
                    case ODataDeltaKind.None:
                    case ODataDeltaKind.Entry:
                        if (info.IncludeFragmentItemSelector)
                        {
                            // #ContainerName.NavigationSourceName  ==>  #ContainerName.NavigationSourceName/$entity
                            builder.Append(ODataConstants.ContextUriFragmentItemSelector);
                        }

                        break;
                    case ODataDeltaKind.Feed:
                        builder.Append(ODataConstants.ContextUriDeltaFeed);
                        break;
                    case ODataDeltaKind.DeletedEntry:
                        builder.Append(ODataConstants.ContextUriDeletedEntry);
                        break;
                    case ODataDeltaKind.Link:
                        builder.Append(ODataConstants.ContextUriDeltaLink);
                        break;
                    case ODataDeltaKind.DeletedLink:
                        builder.Append(ODataConstants.ContextUriDeletedLink);
                        break;
                }
            }
            else if (!string.IsNullOrEmpty(info.TypeName))
            {
                // #TypeName
                builder.Append(info.TypeName);
            }
            else
            {
                return null;
            }

            return new Uri(this.baseContextUrl, builder.ToString());
        }

        /// <summary>
        /// Validate TypeName for given ODataContextUrlInfo for property.
        /// </summary>
        /// <param name="contextUrlInfo">The ODataContextUrlInfo to evaluate on.</param>
        private static void ValidateType(ODataContextUrlInfo contextUrlInfo)
        {
            if (string.IsNullOrEmpty(contextUrlInfo.TypeName))
            {
                throw new ODataException(Strings.ODataContextUriBuilder_TypeNameMissingForProperty);
            }
        }

        /// <summary>
        /// Validate TypeName for given ODataContextUrlInfo for collection.
        /// </summary>
        /// <param name="contextUrlInfo">The ODataContextUrlInfo to evaluate on.</param>
        private static void ValidateCollectionType(ODataContextUrlInfo contextUrlInfo)
        {
            if (string.IsNullOrEmpty(contextUrlInfo.TypeName))
            {
                throw new ODataException(Strings.ODataContextUriBuilder_TypeNameMissingForTopLevelCollection);
            }
        }

        /// <summary>
        /// Validate NavigationPath for given ODataContextUrlInfo for entry or feed
        /// </summary>
        /// <param name="contextUrlInfo">The ODataContextUrlInfo to evaluate on.</param>
        private static void ValidateNavigationPath(ODataContextUrlInfo contextUrlInfo)
        {
            if (string.IsNullOrEmpty(contextUrlInfo.NavigationPath))
            {
                throw new ODataException(Strings.ODataContextUriBuilder_NavigationSourceMissingForEntryAndFeed);
            }
        }

        /// <summary>
        /// Validate ResourcePath for given ODataContextUrlInfo for individual property.
        /// </summary>
        /// <param name="contextUrlInfo">The ODataContextUrlInfo to evaluate on.</param>
        private static void ValidateResourcePath(ODataContextUrlInfo contextUrlInfo)
        {
            if (string.IsNullOrEmpty(contextUrlInfo.ResourcePath))
            {
                throw new ODataException(Strings.ODataContextUriBuilder_ODataUriMissingForIndividualProperty);
            }
        }

        /// <summary>
        /// Validate the given ODataContextUrlInfo for delta
        /// </summary>
        /// <param name="contextUrlInfo">The ODataContextUrlInfo to evaluate on.</param>
        private static void ValidateDelta(ODataContextUrlInfo contextUrlInfo)
        {
            Debug.Assert(contextUrlInfo.DeltaKind != ODataDeltaKind.None, "contextUrlInfo.DeltaKind != ODataDeltaKind.None");
        }
    }
}
