﻿using System.Collections.Generic;
using SAUtils.InputFileParsers.DGV;
using UnitTests.Fixtures;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.FileHandling.SaFileParsers
{
    [Collection("ChromosomeRenamer")]
    public class DgvTests
    {
        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public DgvTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        [Fact]
        public void ExtractDgvCnv()
        {
            const string dgvLine = "nsv482937	1	1	2300000	CNV	loss	Iafrate_et_al_2004	15286789	BAC aCGH,FISH			nssv2995976	M		39	0	1		ACAP3,AGRN,WASH7P	";

            var dgvItem = DgvReader.ExtractDgvItem(dgvLine, _renamer);
            Assert.True(dgvItem.IsInterval);

            var dgvInterval = dgvItem.GetSupplementaryInterval(_renamer);

            Assert.Equal(1, dgvInterval.Start);
            Assert.Equal(2300000, dgvInterval.End);
            Assert.Equal("copy_number_loss", dgvInterval.VariantType.ToString());
            Assert.Equal("DGV", dgvInterval.Source);
            Assert.Equal("nsv482937", dgvInterval.StringValues["id"]);
            Assert.Equal("0.02564", dgvInterval.PopulationFrequencies["variantFreqAll"].ToString("0.#####"));
            Assert.Equal(39, dgvInterval.IntValues["sampleSize"]);
            Assert.Equal(1, dgvInterval.IntValues["observedLosses"]);
            Assert.False(dgvInterval.IntValues.ContainsKey("observedGains"));
        }

        [Fact]
        public void ExtractDgvComplex()
        {
            const string dgvLine = "esv2421662	1	12841928	12971833	OTHER	complex	Altshuler_et_al_2010	20811451	SNP array			essv5038349,essv5012238	M		1184	20	70		HNRNPCL1,LOC649330,PRAMEF1,PRAMEF10,PRAMEF11,PRAMEF2,PRAMEF4	NA10838,NA10847";

            var dgvItem = DgvReader.ExtractDgvItem(dgvLine, _renamer);
            Assert.True(dgvItem.IsInterval);

            var dgvInterval = dgvItem.GetSupplementaryInterval(_renamer);

            Assert.Equal(12841928, dgvInterval.Start);
            Assert.Equal(12971833, dgvInterval.End);
            Assert.Equal("complex_structural_alteration", dgvInterval.VariantType.ToString());
            Assert.Equal("DGV", dgvInterval.Source);
            Assert.Equal("esv2421662", dgvInterval.StringValues["id"]);
            Assert.Equal("0.07601", dgvInterval.PopulationFrequencies["variantFreqAll"].ToString("0.#####"));
            Assert.Equal(1184, dgvInterval.IntValues["sampleSize"]);
            Assert.Equal(70, dgvInterval.IntValues["observedLosses"]);
            Assert.Equal(20, dgvInterval.IntValues["observedGains"]);
        }

        [Fact]
        public void EmptyObservedLossesAndGains()
        {
            const string dgvLine = "nsv161172	1	88190	89153	CNV	deletion	Mills_et_al_2006	16902084	Sequencing			nssv179750	M		24					";

            var dgvItem = DgvReader.ExtractDgvItem(dgvLine, _renamer);
            Assert.True(dgvItem.IsInterval);

            var dgvInterval = dgvItem.GetSupplementaryInterval(_renamer);

            Assert.Equal("1", dgvInterval.ReferenceName);
            Assert.Equal(88190, dgvInterval.Start);
            Assert.Equal(89153, dgvInterval.End);
            Assert.Equal("copy_number_loss", dgvInterval.VariantType.ToString());
            Assert.Equal("DGV", dgvInterval.Source);
            Assert.Equal("nsv161172", dgvInterval.StringValues["id"]);
            Assert.Equal(24, dgvInterval.IntValues["sampleSize"]);
            Assert.False(dgvInterval.IntValues.ContainsKey("observedGains"));
            Assert.False(dgvInterval.IntValues.ContainsKey("observedLosses"));
            Assert.False(dgvInterval.PopulationFrequencies.ContainsKey("variantFreqAll"));

        }

        [Fact]
        public void EqualityAndHash()
        {
            var dgvItem = new DgvItem("dgv101", "chr1", 100, 200, 123, 34, 32, VariantType.complex_structural_alteration);

            var dgvHash = new HashSet<DgvItem> { dgvItem };

            Assert.Equal(1, dgvHash.Count);
            Assert.True(dgvHash.Contains(dgvItem));
        }
    }
}