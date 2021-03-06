﻿using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures.VCF;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("ChromosomeRenamer")]
    public sealed class SampleFieldExtractorTests
    {
        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public SampleFieldExtractorTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        [Fact]
        public void FormatIndicesTest()
        {
            const string formatColumn = "AU:GU:TAR:FT:GQ:DP:VF:CU:TU:TIR:GT:GQX:BOB:DPI:NV:NR";
            var formatIndicies = FormatIndices.Extract(formatColumn);

            Assert.Equal(0, formatIndicies.AU);
            Assert.Equal(7, formatIndicies.CU);
            Assert.Equal(1, formatIndicies.GU);
            Assert.Equal(8, formatIndicies.TU);
            Assert.Equal(15, formatIndicies.NR);
            Assert.Equal(2, formatIndicies.TAR);
            Assert.Equal(9, formatIndicies.TIR);
            Assert.Equal(3, formatIndicies.FT);
            Assert.Equal(10, formatIndicies.GT);
            Assert.Equal(4, formatIndicies.GQ);
            Assert.Equal(11, formatIndicies.GQX);
            Assert.Equal(5, formatIndicies.DP);
            Assert.Equal(6, formatIndicies.VF);
            Assert.Equal(13, formatIndicies.DPI);
            Assert.Equal(14, formatIndicies.NV);

            Assert.Null(FormatIndices.Extract(null));

            formatIndicies = FormatIndices.Extract("TEMP:DPI:BOB");
            Assert.Equal(1, formatIndicies.DPI);
            Assert.Null(formatIndicies.AU);
        }

        [Theory]
        [InlineData("GT:TIR:TAR", "1/1:18,19:37,38", new[] { "37", "18" })]
        [InlineData("GT:NR:NV", "1/1:10:7", new[] { "3", "7" })]
        [InlineData("GT:AU:CU:GU:TU:AD", "1/1:10,11:20,21:30,31:40,41:11,13", new[] { "20", "40" })]
        [InlineData("GT:AD", "1/1:11,13", new[] { "11", "13" })]
        [InlineData("GT:AU:CU:GU:TU:AD", "1/1:.:20,21:30,31:40,41:11,13", new[] { "11", "13" })]
        [InlineData("AD", ".", null)]
        [InlineData("AD", "", null)]
        public void AlleleDepths(string formatCol, string sampleCol, string[] expectedAlleleDepths)
        {
            string vcfLine = $"chr1\t5592503\t.\tC\tT\t900.00\tPASS\t.\t{formatCol}\t{sampleCol}";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples();

            Assert.Equal(1, samples.Count);

            var sample = samples[0];
            var observedAlleleDepths = sample?.AlleleDepths;
            Assert.Equal(expectedAlleleDepths, observedAlleleDepths);
        }

        [Theory]
        [InlineData("GT:TIR:TAR", "1/1:18,19:37,38", null)]
        [InlineData("GT:NR:NV", "1/1:10:7", null)]
        [InlineData("GT:TIR:TAR:AD", "1/1:.:37,38:11,13,17", new[] { "11", "13", "17" })]
        public void AlleleDepthsMultiAllelic(string formatCol, string sampleCol, string[] expectedAlleleDepths)
        {
            string vcfLine = $"chr1\t5592503\t.\tC\tT,A\t900.00\tPASS\t.\t{formatCol}\t{sampleCol}";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples();

            Assert.Equal(1, samples.Count);

            var sample = samples[0];
            var observedAlleleDepths = sample?.AlleleDepths;
            Assert.Equal(expectedAlleleDepths, observedAlleleDepths);
        }

        [Theory]
        [InlineData("1/1:208:47:70:3:F", true)]
        [InlineData("1/1:208:47:70:3:.", false)]
        [InlineData(".", false)]
        [InlineData("", false)]
        public void FailedFilter(string sampleCol, bool? expectedFailedFilter)
        {
            string vcfLine = $"chr1\t5592503\t.\tC\tT\t900.00\tPASS\t.\tGT:GQ:GQX:DP:DPF:FT\t{sampleCol}";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples();

            Assert.Equal(1, samples.Count);

            var sample = samples[0];
            var observedFailedFilter = sample?.FailedFilter;
            Assert.Equal(expectedFailedFilter, observedFailedFilter);
        }

        [Theory]
        [InlineData("1/1:208:47:70:3:0,70", "1/1")]
        [InlineData(".:208:47:70:3:0,70", null)]
        [InlineData(".", null)]
        [InlineData("", null)]
        [InlineData("./.", "./.")]
        public void Genotype(string sampleCol, string expectedGenotype)
        {
            string vcfLine = $"chr1\t5592503\t.\tC\tT\t900.00\tPASS\t.\tGT:GQ:GQX:DP:DPF:AD\t{sampleCol}";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples();

            Assert.Equal(1, samples.Count);

            var sample = samples[0];
            var observedGenotype = sample?.Genotype;
            Assert.Equal(expectedGenotype, observedGenotype);
        }

        [Theory]
        [InlineData("GT:GQ:GQX:DP:DPF:AD", "1/1:208:47:70:3:0,70", "47")]
        [InlineData("GT:GQ:DP:DPF:AD", "1/1:208:70:3:0,70", "208")]
        [InlineData("GT:GQ:DP:DPF:AD", "1/1:.:70:3:0,70", null)]
        [InlineData("GQ", ".", null)]
        [InlineData("GQX", "", null)]
        [InlineData("GQX", "./.", null)]
        public void GenotypeQuality(string formatCol, string sampleCol, string expectedGenotypeQuality)
        {
            string vcfLine = $"chr1\t5592503\t.\tC\tT\t900.00\tPASS\t.\t{formatCol}\t{sampleCol}";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples();

            Assert.Equal(1, samples.Count);

            var sample = samples[0];
            var observedGenotypeQuality = sample?.GenotypeQuality;
            Assert.Equal(expectedGenotypeQuality, observedGenotypeQuality);
        }

        [Theory]
        [InlineData("GT:TIR:TAR:DP:DPF:AD", "1/1:22,22:3,4:70:3:0,70", "25")]
        [InlineData("GT:NR:NV", "1/1:10:7", "10")]
        [InlineData("GT:AU:CU:GU:TU:DP:DPF:AD", "1/1:10,11:20,21:30,31:40,41:70:3:0,70", "100")]
        [InlineData("GT:DPI:DP:DPF:AD", "1/1:17:70:3:0,70", "17")]
        [InlineData("GT:DP:DPF:AD", "1/1:70:3:0,70", "70")]
        [InlineData("GT:AU:CU:GU:TU:DPF:AD", "1/1:.:20,21:30,31:40,41:3:0,70", null)]
        [InlineData("GT:DP:DPF:AD", "1/1:.:3:0,70", null)]
        [InlineData("DP", ".", null)]
        [InlineData("DPI", "", null)]
        public void TotalDepth(string formatCol, string sampleCol, string expectedTotalDepth)
        {
            string vcfLine = $"chr1\t5592503\t.\tC\tT\t900.00\tPASS\t.\t{formatCol}\t{sampleCol}";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples();

            Assert.Equal(1, samples.Count);

            var sample = samples[0];
            var observedTotalDepth = sample?.TotalDepth;
            Assert.Equal(expectedTotalDepth, observedTotalDepth);
        }

        [Theory]
        [InlineData("GT:NR:NV", "1/1:10:7", null)]
        public void TotalDepthMultiAllelic(string formatCol, string sampleCol, string expectedTotalDepth)
        {
            string vcfLine = $"chr1\t5592503\t.\tC\tT,A\t900.00\tPASS\t.\t{formatCol}\t{sampleCol}";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples();

            Assert.Equal(1, samples.Count);

            var sample = samples[0];
            var observedTotalDepth = sample?.TotalDepth;
            Assert.Equal(expectedTotalDepth, observedTotalDepth);
        }

        [Fact]
        public void PiscesTotalDepth()
        {
            const string vcfLine = "chr1\t115251293\t.\tGA\tG\t100\tSB;LowVariantFreq\tDP=7882\tGT:GQ:AD:VF:NL:SB:GQX\t0/1:100:7588,294:0:20:-100.0000:100";
            var jsonVariant = VcfUtilities.GetVariant(vcfLine, _renamer);

            // get the sample info
            var samples = jsonVariant.ExtractSampleInfo();
            Assert.Equal(1, samples.Count);

            var sample = samples[0];
            var observedTotalDepth = sample.TotalDepth;
            const string expectedTotalDepth = "7882";
            Assert.Equal(expectedTotalDepth, observedTotalDepth);
        }

        [Theory]
        [InlineData("GT:GQ:GQX:DP:DPF:AD:VF", "1/1:208:47:70:3:0,70:0.75", "1")]
        [InlineData("GT:TIR:TAR", "1/1:10,11:20,21", "0.3333")]
        [InlineData("GT:NR:NV", "1/1:10:7", "0.7")]
        [InlineData("GT:AU:CU:GU:TU", "1/1:10,11:20,21:30,31:40,41", "0.8")]
        [InlineData("GT:AD", "1/1:3,70", "0.9589")]
        [InlineData("GT:AU:CU:GU:TU:AD", "1/1:.:20,21:30,31:40,41:7,11", "0.6111")]
        [InlineData("GT:AD", "1/1:.", null)]
        [InlineData("VF", ".", null)]
        [InlineData("AD", "", null)]
        [InlineData("GT:GQ:AD:VF:NL:SB:GQX", "0/1:100:7588,294:0:20:-100.0000:100", "0.0373")]
        public void VariantFrequency(string formatCol, string sampleCol, string expectedVariantFrequency)
        {
            string vcfLine = $"chr1\t5592503\t.\tC\tT\t900.00\tPASS\t.\t{formatCol}\t{sampleCol}";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples();

            Assert.Equal(1, samples.Count);

            var sample = samples[0];
            var observedVariantFrequency = sample?.VariantFrequency;
            Assert.Equal(expectedVariantFrequency, observedVariantFrequency);
        }

        [Theory]
        [InlineData("GT:NR:NV", "1/1:10:7", null)]
        public void VariantFrequencyMultiAllelic(string formatCol, string sampleCol, string expectedVariantFrequency)
        {
            string vcfLine = $"chr1\t5592503\t.\tC\tT,A\t900.00\tPASS\t.\t{formatCol}\t{sampleCol}";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples();

            Assert.Equal(1, samples.Count);

            var sample = samples[0];
            var observedVariantFrequency = sample?.VariantFrequency;
            Assert.Equal(expectedVariantFrequency, observedVariantFrequency);
        }

        [Fact]
        public void MajorChromosomeCopyTest()
        {
            // data from NIR-1095
            // for NIR-1218
            var vcfLine = "1	9314202	Canvas:GAIN:1:9314202:9404148	N	<CNV>	36	PASS	SVTYPE=CNV;END=9404148;ensembl_gene_id=ENSG00000049239,ENSG00000252841,ENSG00000171621	RC:BC:CN:MCC	.	151:108:6:4";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples();

            Assert.Equal(2, samples.Count);

            var sample = samples[1];

            var observedMcc = sample?.IsLossOfHeterozygosity;
            Assert.Equal(false, observedMcc);
        }

        [Fact]
        public void EmptySamples()
        {

            // for NIR-1306
            var vcfLine = "chrX	2735147	.	G	A	38.25	VQSRTrancheSNP99.90to100.00	AC=3;AF=0.500;AN=6;BaseQRankSum=-0.602;DP=56;Dels=0.00;FS=30.019;HaplotypeScore=7.7259;MLEAC=3;MLEAF=0.500;MQ=41.18;MQ0=0;MQRankSum=0.098;QD=1.06;ReadPosRankSum=0.266;SB=-8.681e-03;VQSLOD=-6.0901;culprit=QD	GT:AD:DP:GQ:PL	0:7,0:7:3:0,3,39	./.	0/1:14,3:17:35:35,0,35	1/1:9,10:19:3:41,3,0";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples();

            Assert.Equal(4, samples.Count);

            var sample = samples[1];
            var observedGenotype = sample.Genotype;
            var observedVariantFrequency = sample.VariantFrequency;

            Assert.Equal("./.", observedGenotype);
            Assert.Null(observedVariantFrequency);
        }


        [Fact]

        public void GatkGenomeVcfSample()
        {

            // For NIR-1320
            // the original AD= 15,11,0
            var vcfLine =
                "1	30923	rs140337953	G	T,<NON_REF>	264.77	PASS	BaseQRankSum=0.259;DB;DP=26;MLEAC=1,0;MLEAF=0.500,0.00;MQ=43.87;MQ0=0;MQRankSum=-0.830;ReadPosRankSum=-0.156	GT:AD:GQ:PL:SB	0/1:15,11,20:99:293,0,330,337,363,700:8,7,3,8";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples(true);

            Assert.Equal(1, samples.Count);
            var sample = samples[0];
            var observedVariantFrequency = sample?.VariantFrequency;
            var expectedVariantFrequency = "0.4231";
            Assert.Equal(expectedVariantFrequency, observedVariantFrequency);
            var expectedAlleleDepths = new[] { "15", "11" };
            var observedAlleleDepths = sample?.AlleleDepths;
            Assert.Equal(expectedAlleleDepths, observedAlleleDepths);
        }

        [Theory]
        [InlineData("GT:TIR:TAR", "1/1:0,11:0,21", "0")]
        [InlineData("GT:NR:NV", "1/1:0:0", "0")]
        [InlineData("GT:AU:CU:GU:TU", "1/1:0,11:0,21:0,31:0,41", "0")]
        [InlineData("GT:AD", "1/1:0,0", "0")]
        [InlineData("GT:AU:CU:GU:TU:AD", "1/1:.:20,21:30,31:40,41:0,0", "0")]
        [InlineData("GT:AD", "1/1:.", null)]
        [InlineData("VF", ".", null)]
        [InlineData("AD", "", null)]
        public void VariantFrequencyNan(string formatCol, string sampleCol, string expectedVariantFrequency)
        {
            // NIR-1338
            string vcfLine = $"chr1\t5592503\t.\tC\tT\t900.00\tPASS\t.\t{formatCol}\t{sampleCol}";
            var vcfColumns = vcfLine.Split('\t');

            var extractor = new SampleFieldExtractor(vcfColumns);
            var samples = extractor.ExtractSamples();

            Assert.Equal(1, samples.Count);

            var sample = samples[0];
            var observedVariantFrequency = sample?.VariantFrequency;
            Assert.Equal(expectedVariantFrequency, observedVariantFrequency);
        }

		[Fact]
	    public void SplitReadCounts()
	    {
		    var vcfLine =
			    "chr7	127717248	MantaINV:267944:0:1:2:0:0	T	<INV>	.	PASS	END=140789466;SVTYPE=INV;SVLEN=13072218;INV5	PR:SR	78,0:65,0	157,42:252,63";

			var vcfColumns = vcfLine.Split('\t');

			var extractor = new SampleFieldExtractor(vcfColumns);
			var samples = extractor.ExtractSamples();

			Assert.Equal(2, samples.Count);
		    var sample1 = samples[0];
			Assert.Equal(new[] { "78", "0" }, sample1.PairEndReadCounts);
			Assert.Equal(new[] { "65", "0" }, sample1.SplitReadCounts);

			var sample2 = samples[1];
			Assert.Equal(new[] { "157", "42" }, sample2.PairEndReadCounts);
			Assert.Equal(new[] { "252", "63" }, sample2.SplitReadCounts);

		}
    }
}