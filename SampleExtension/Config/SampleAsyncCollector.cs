// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SampleExtension.Config
{

    /// <summary>
    /// Provide the implementation for a collector.
    /// For the sample, we're writing <see cref="SampleItem"/>s to disk. 
    /// Collectors are used for emitting a series of discrete messages (ie, an output binding).
    /// </summary>
    internal class SampleAsyncCollector : IAsyncCollector<SampleItem>
    {
        // Root path for where to write. This cna be a combination of the extension configuration 
        // and the attribute. 
        private readonly string _root;

        public SampleAsyncCollector(string root)
        {
            _root = root;
        }

        public Task AddAsync(SampleItem item, CancellationToken cancellationToken = default(CancellationToken))
        {
            var path = Path.Combine(_root, item.Name);
            File.WriteAllText(path, item.Contents);
            return Task.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}