﻿using System.Collections.Generic;
using System.IO;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling.CustomInterval;
using Xunit;

namespace UnitTests.FileHandling.SaFileParsers
{
    public sealed class CustomIntervalDbRead
    {
        [Fact]
        public void ReadCustomIntervals()
        {
            var customFile   = new FileInfo(Resources.TopPath("chr1.nci"));
            var customReader = new CustomIntervalReader(customFile.FullName);
            var intervals    = new List<VariantAnnotation.DataStructures.CustomInterval>();

            var customInterval = customReader.GetNextCustomInterval();
            while (customInterval != null)
            {
                intervals.Add(customInterval);
                customInterval = customReader.GetNextCustomInterval();
            }

            Assert.Equal(11, intervals.Count); // 11 custom intervals were written.

            Assert.Equal(2, intervals[0].StringValues.Count);
            Assert.Equal(2, intervals[0].NonStringValues.Count);

            Assert.Equal("NOC2L", intervals[2].StringValues["gene"]);
        }
    }
}
