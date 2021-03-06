﻿using System;
using System.Collections.Generic;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    public class ChromosomeRenamerTests
    {
        #region members

        private readonly ChromosomeRenamer _renamer = new ChromosomeRenamer();

        #endregion

        public ChromosomeRenamerTests()
        {
            var referenceMetadata = new List<ReferenceMetadata>
            {
                new ReferenceMetadata("chr1", "1", true),
                new ReferenceMetadata("chrX", "X", true),
                new ReferenceMetadata("chrM", "MT", true),
                new ReferenceMetadata("chr1", "1", true),
                new ReferenceMetadata(null, null, true),
                new ReferenceMetadata("chrEBV", "EBV", false)
            };

            _renamer.AddReferenceMetadata(referenceMetadata);
        }

        [Theory]
        [InlineData("1", "chr1")]
        [InlineData("X", "chrX")]
        [InlineData("MT", "chrM")]
        [InlineData("dummy", "dummy")]
        public void GetUcscReferenceName(string ensemblReferenceName, string expectedReferenceName)
        {
            var observedReferenceName = _renamer.GetUcscReferenceName(ensemblReferenceName);
            Assert.Equal(expectedReferenceName, observedReferenceName);
        }

        [Fact]
        public void DisableOriginalOnFailedLookup()
        {
            var observedReferenceName = _renamer.GetUcscReferenceName("dummy", false);
            Assert.Equal(null, observedReferenceName);

            observedReferenceName = _renamer.GetEnsemblReferenceName("dummy", false);
            Assert.Equal(null, observedReferenceName);
        }

        [Fact]
        public void BeforeInitialization()
        {
            var emptyChromosomeNamer = new ChromosomeRenamer();
            Assert.Throws<InvalidOperationException>(() => emptyChromosomeNamer.GetUcscReferenceName("1"));
            Assert.Throws<InvalidOperationException>(() => emptyChromosomeNamer.GetEnsemblReferenceName("chr1"));
        }

        [Fact]
        public void AddReferenceNameUcscEmpty()
        {
            const string ensemblReferenceName = "1";

            var emptyChromosomeNamer = new ChromosomeRenamer();
            var referenceMetadata = new List<ReferenceMetadata>
            {
                new ReferenceMetadata(ensemblReferenceName, null, true)
            };
            emptyChromosomeNamer.AddReferenceMetadata(referenceMetadata);

            var observedUcscReferenceName = emptyChromosomeNamer.GetUcscReferenceName(ensemblReferenceName);
            var observedEnsemblReferenceName = emptyChromosomeNamer.GetEnsemblReferenceName(null);

            Assert.Null(observedEnsemblReferenceName);
            Assert.Equal(ensemblReferenceName, observedUcscReferenceName);
        }

        [Fact]
        public void AddReferenceNameEnsemblEmpty()
        {
            const string ucscReferenceName = "chr1";

            var emptyChromosomeNamer = new ChromosomeRenamer();
            var referenceMetadata = new List<ReferenceMetadata>
            {
                new ReferenceMetadata(null, ucscReferenceName, true)
            };
            emptyChromosomeNamer.AddReferenceMetadata(referenceMetadata);

            var observedUcscReferenceName = emptyChromosomeNamer.GetUcscReferenceName(null);
            var observedEnsemblReferenceName = emptyChromosomeNamer.GetEnsemblReferenceName(ucscReferenceName);

            Assert.Equal(ucscReferenceName, observedEnsemblReferenceName);
            Assert.Null(observedUcscReferenceName);
        }

        [Theory]
        [InlineData("chr1", "1")]
        [InlineData("chrX", "X")]
        [InlineData("chrM", "MT")]
        [InlineData("dummy", "dummy")]
        public void GetEnsemblReferenceName(string ucscReferenceName, string expectedReferenceName)
        {
            var observedReferenceName = _renamer.GetEnsemblReferenceName(ucscReferenceName);
            Assert.Equal(expectedReferenceName, observedReferenceName);
        }

        [Theory]
        [InlineData("chr1", true)]
        [InlineData("1", true)]
        [InlineData("chrM", true)]
        [InlineData("chrEBV", false)]
        [InlineData("dummy", false)]
        public void InReferenceAndVep(string referenceName, bool expectedResult)
        {
            var observedResult = _renamer.InReferenceAndVep(referenceName);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}
