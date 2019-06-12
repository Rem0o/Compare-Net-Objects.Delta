using KellermanSoftware.CompareNetObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CompareNetObjects.Delta
{
    public class ComparisonResult<T> : ComparisonResult
    {
        internal ComparisonResult(ComparisonResult comparisonResult): this(comparisonResult.Config)
        {
            this.Differences = comparisonResult.Differences;
            this.DifferencesString = comparisonResult.DifferencesString;
        }

        public ComparisonResult(ComparisonConfig config) : base(config)
        {
        }
    }
}
