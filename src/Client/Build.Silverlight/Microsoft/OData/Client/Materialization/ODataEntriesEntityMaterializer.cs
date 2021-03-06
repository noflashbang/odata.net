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

namespace Microsoft.OData.Client.Materialization
{
    using System;
    using System.Collections.Generic;
    using Microsoft.OData.Client;
    using Microsoft.OData.Core;
    using DSClient = Microsoft.OData.Client;

    /// <summary>
    /// Materializes entities from a sequence of ODataEntry objects
    /// </summary>
    internal sealed class ODataEntriesEntityMaterializer : ODataEntityMaterializer
    {
        /// <summary>The format of the response being materialized.</summary>
        private readonly ODataFormat format;

        /// <summary>The entries enumerator</summary>
        private IEnumerator<ODataEntry> feedEntries;

        /// <summary>Is the enumerator finished.</summary>
        private bool isFinished;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntriesEntityMaterializer" /> class.
        /// </summary>
        /// <param name="entries">The entries.</param>
        /// <param name="materializerContext">The materializer context.</param>
        /// <param name="entityTrackingAdapter">The entity tracking adapter.</param>
        /// <param name="queryComponents">The query components.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="materializeEntryPlan">The materialize entry plan.</param>
        /// <param name="format">The format.</param>
        public ODataEntriesEntityMaterializer(
            IEnumerable<ODataEntry> entries,
            IODataMaterializerContext materializerContext,
            EntityTrackingAdapter entityTrackingAdapter, 
            QueryComponents queryComponents, 
            Type expectedType, 
            ProjectionPlan materializeEntryPlan, 
            ODataFormat format)
            : base(materializerContext, entityTrackingAdapter, queryComponents, expectedType, materializeEntryPlan)
        {
            this.format = format;
            this.feedEntries = entries.GetEnumerator();
        }

        /// <summary>
        /// Feed being materialized; possibly null.
        /// </summary>
        internal override ODataFeed CurrentFeed
        {
            get { return null; }
        }

        /// <summary>
        /// Entry being materialized; possibly null.
        /// </summary>
        internal override ODataEntry CurrentEntry
        {
            get
            {
                this.VerifyNotDisposed();
                return this.feedEntries.Current;
            }
        }

        /// <summary>
        /// The count tag's value, if requested
        /// </summary>
        /// <returns>The count value returned from the server</returns>
        internal override long CountValue
        {
            get
            {
                throw new InvalidOperationException(DSClient.Strings.MaterializeFromAtom_CountNotPresent);
            }
        }

        /// <summary>
        /// Returns true if the underlying object used for counting is available
        /// </summary>
        internal override bool IsCountable
        {
            get { return false; }
        }

        /// <summary>
        /// Whether we have finished processing the current data stream.
        /// </summary>
        internal override bool IsEndOfStream
        {
            get { return this.isFinished; }
        }

        /// <summary>
        /// Returns true if the materializer has been disposed
        /// </summary>
        protected override bool IsDisposed
        {
            get { return this.feedEntries == null; }
        }

        /// <summary>
        /// The format of the response being materialized.
        /// </summary>
        protected override ODataFormat Format
        {
            get { return this.format; }
        }

        /// <summary>
        /// Reads the next feed or entry.
        /// </summary>
        /// <returns>
        /// True if an entry was read, otherwise false
        /// </returns>
        protected override bool ReadNextFeedOrEntry()
        {
            if (!this.isFinished)
            {
                if (!this.feedEntries.MoveNext())
                {
                    this.isFinished = true;
                }
            }

            return !this.isFinished;
        }

        /// <summary>
        /// Called when IDisposable.Dispose is called.
        /// </summary>
        protected override void OnDispose()
        {
            if (this.feedEntries != null)
            {
                this.feedEntries.Dispose();
                this.feedEntries = null;
            }
        }
    }
}
