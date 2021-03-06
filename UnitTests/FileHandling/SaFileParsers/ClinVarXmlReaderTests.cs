﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.InputFileParsers.ClinVar;
using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.FileHandling.SaFileParsers
{
    [Collection("ChromosomeRenamer")]
    public class ClinVarXmlReaderTests
    {
        private readonly ICompressedSequence _sequence;
        private readonly CompressedSequenceReader _reader;

        /// <summary>
        /// constructor
        /// </summary>
        public ClinVarXmlReaderTests(ChromosomeRenamerFixture fixture)
        {
            _sequence = fixture.Sequence;
            _reader = fixture.Reader;
        }

        [Fact]
        public void BasicReadTest()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000077146.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.Equal("RCV000077146.3", clinVarItem.ID);

                switch (clinVarItem.ID)
                {
                    case "RCV000077146.3":
                        Assert.Equal("17", clinVarItem.Chromosome);
                        Assert.Equal(41234419, clinVarItem.Start);
                        Assert.Equal("A", clinVarItem.ReferenceAllele);
                        Assert.Equal("C", clinVarItem.AltAllele);
                        Assert.Equal(ClinVarXmlReader.ParseDate("2016-07-31"), clinVarItem.LastUpdatedDate);
                        Assert.True(clinVarItem.AlleleOrigins.SequenceEqual(new List<string> { "germline" }));
                        Assert.Equal("C2676676", clinVarItem.MedGenIDs.First());
                        Assert.Equal("145", clinVarItem.OrphanetIDs.First());
                        Assert.Equal("604370", clinVarItem.OmimIDs.First());
                        Assert.Equal("Breast-ovarian cancer, familial 1", clinVarItem.Phenotypes.First());
                        Assert.Null(clinVarItem.PubmedIds);
                        break;
                }
            }
        }

        [Fact]
        public void MissingAltAllele()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000120902.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.Equal("C", clinVarItem.ReferenceAllele);
                Assert.Equal("G", clinVarItem.AltAllele);
            }
        }

        [Fact]
        public void MultiEntryXmlParsing()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("MultiClinvar.xml")), _reader, _sequence);

            var clinvarList = new List<ClinVarItem>();
            foreach (var clinVarItem in reader)
            {

                switch (clinVarItem.ID)
                {
                    case "RCV000000064.5":
                        Assert.Equal(ClinVarXmlReader.ParseDate("2016-02-17"), clinVarItem.LastUpdatedDate);
                        Assert.Equal("risk factor", clinVarItem.Significance);
                        break;
                    case "RCV000000068.3":
                        Assert.Equal(ClinVarXmlReader.ParseDate("2016-02-17"), clinVarItem.LastUpdatedDate);
                        Assert.Equal("pathogenic", clinVarItem.Significance);
                        Assert.Equal("C3150419", clinVarItem.MedGenIDs.First());
                        break;
                    case "RCV000000069.3":
                        Assert.Equal(ClinVarXmlReader.ParseDate("2016-02-17"), clinVarItem.LastUpdatedDate);
                        Assert.Equal("pathogenic", clinVarItem.Significance);
                        Assert.Equal("C3150419", clinVarItem.MedGenIDs.First());
                        Assert.Equal(20179356, clinVarItem.PubmedIds.First());
                        break;
                    default:
                        throw new InvalidDataException("Unexpected clinvar id encountered");
                }
                clinvarList.Add(clinVarItem);
            }

            clinvarList.Sort();
            Assert.Equal(2, clinvarList.Count);
            Assert.Equal("2", clinvarList[0].Chromosome);
            Assert.Equal("22", clinvarList[1].Chromosome);
        }

        [Fact]
        public void MultiVariantEntry()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000007484.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                switch (clinVarItem.Start)
                {
                    case 8045031:
                        Assert.Equal("G", clinVarItem.ReferenceAllele);
                        Assert.Equal("A", clinVarItem.AltAllele);
                        break;
                    case 8021911:
                        Assert.Equal("GTGCTGGACGGTGTCCCT", clinVarItem.AltAllele);
                        var sa = new SupplementaryAnnotationPosition(clinVarItem.Start);
                        var saCreator = new SupplementaryPositionCreator(sa);

                        clinVarItem.SetSupplementaryAnnotations(saCreator);
                        Assert.Equal("iGTGCTGGACGGTGTCCCT", clinVarItem.SaAltAllele);
                        break;
                    default:
                        throw new InvalidDataException($"Unexpected clinvar item start point : {clinVarItem.Start}");
                }

            }
        }

        [Fact]
        public void NonEnglishChars()
        {
            //NIR-900
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000087262.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.Equal("Pelger-Huët anomaly", clinVarItem.Phenotypes.First());
            }
        }

        [Fact]
        public void WrongPosition()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000073701.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                switch (clinVarItem.Start)
                {
                    case 112064826:
                        Assert.Equal("G", clinVarItem.ReferenceAllele);
                        Assert.Equal("C", clinVarItem.AltAllele);
                        break;
                    default:
                        throw new InvalidDataException($"Unexpected clinvar item start point : {clinVarItem.Start}");
                }
            }
        }

        [Fact(Skip = "Requires a reference sequence.")]
        public void PubmedTest1()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000152657.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 12114475, 18836774, 22357542, 24033266 }));
            }
        }

        [Fact(Skip = "Requires a reference sequence.")]
        public void PubmedTest2()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000016673.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 6714226, 6826539, 9113933, 9845707, 12000828, 12383672 }));
            }
        }

        [Fact]
        public void PubmedTest3()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000038438.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 17285735, 17877814, 22848293, 24033266 }));
            }
        }

        [Fact]
        public void PubmedTest4()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000021819.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 8099202 }));
            }
        }

        [Fact]
        public void PubmedTest5()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000000734.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.Null(clinVarItem.PubmedIds);
            }
        }

        [Fact]
        public void PubmedTest6()
        {
            //extracting from SCV record
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000120902.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 24728327 }));
            }
        }

        [Fact(Skip = "Requires a reference sequence.")]
        public void MultiScvPubmed()
        {
            //extracting from SCV record
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000194003.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 25741868, 26092869 }));
            }
        }

        [Fact]
        public void NoClinVarItem()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000000101.xml")), _reader, _sequence);

            var clinVarList = new List<ClinVarItem>();

            foreach (var clinVarItem in reader)
            {
                clinVarList.Add(clinVarItem);
            }

            Assert.Equal(0, clinVarList.Count);
        }

        [Fact]
        public void ClinVarForRef()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000124712.xml")), _reader, _sequence);

            var clinVarList = new List<ClinVarItem>();
            foreach (var clinVarItem in reader)
            {
                clinVarList.Add(clinVarItem);
                Assert.Equal(clinVarItem.ReferenceAllele, clinVarItem.AltAllele);
            }

            Assert.Equal(1, clinVarList.Count);
        }

        [Fact]
        public void MultiplePhenotypes()
        {
            //no citations show up for this RCV in the website. But the XML has these pubmed ids under fields that we parse pubmed ids from
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000144179.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                var expectedPhenotypes = new List<string> { "Single ventricle", "small Atrial septal defect" };
                Assert.True(expectedPhenotypes.SequenceEqual(clinVarItem.Phenotypes));
            }
        }

        [Fact]
        public void MultipleOrigins()
        {
            //no citations show up for this RCV in the website. But the XML has these pubmed ids under fields that we parse pubmed ids from
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000080071.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                var expectedOrigins = new List<string> { "germline", "maternal", "unknown" };
                Assert.True(expectedOrigins.SequenceEqual(clinVarItem.AlleleOrigins));
            }
        }


        [Fact]
        public void SkipGeneralCitations()
        {
            //no citations show up for this RCV in the website. But the XML has these pubmed ids under fields that we parse pubmed ids from
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000003254.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 12023369, 17068223, 17447842, 17587057, 17786191, 17804789, 18438406, 19122664, 20228799 }));
            }
        }

        [Fact(Skip = "Requires a reference sequence.")]
        public void IndelTest()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000032548.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.Equal("RCV000032548.5", clinVarItem.ID);

                switch (clinVarItem.ID)
                {
                    case "RCV000032548.5":
                        Assert.Equal("4", clinVarItem.Chromosome);
                        Assert.Equal(187122303, clinVarItem.Start);
                        Assert.Equal(17, clinVarItem.ReferenceAllele.Length);
                        Assert.Equal("GC", clinVarItem.AltAllele);
                        Assert.Equal(ClinVarXmlReader.ParseDate("2016-08-29"), clinVarItem.LastUpdatedDate);
                        break;
                }
            }
        }


        [Fact]
        [Trait("jira", "NIR-2034")]
        public void MultiScvPubmeds()
        {
            //extracting from SCV record
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000203290.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 23806086, 24088041, 25736269 }));
            }
        }

        [Fact]
        [Trait("jira", "NIR-2034")]
        public void MultipleAlleleOrigins()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000112977.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.Equal(2, clinVarItem.AlleleOrigins.Count());
                Assert.NotEqual(clinVarItem.AlleleOrigins.First(), clinVarItem.AlleleOrigins.Last());

                foreach (var origin in clinVarItem.AlleleOrigins)
                {
                    Assert.True(origin == "unknown" || origin == "germline");
                }
            }
        }

        [Fact]
        [Trait("jira", "NIR-2035")]
        public void EmptyRefAndAlt()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000083638.xml")), _reader, _sequence);

            var clinvarItems = reader.GetEnumerator();
            Assert.Null(clinvarItems.Current);
            clinvarItems.Dispose();
        }

        [Fact]
        [Trait("jira", "NIR-2036")]
        public void SkipMicrosattelite()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000005426.xml")), _reader, _sequence);

            var clinvarItems = reader.GetEnumerator();
            Assert.Null(clinvarItems.Current);
            clinvarItems.Dispose();
        }

        [Fact(Skip = "Requires a reference sequence.")]
        [Trait("jira", "NIR-2072")]
        public void MissingClinvarInsertion()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000179026.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.Equal(2337968, clinVarItem.Start);
            }
        }

        [Fact(Skip = "Requires a reference sequence.")]
        [Trait("jira", "NIR-2072")]
        public void MissingClinvarInsertionShift()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000207071.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.Equal(3751646, clinVarItem.Start);
            }
        }

        [Fact(Skip = "Requires a reference sequence.")]
        [Trait("jira", "NIR-2072")]
        public void MissingClinvarInsertionShift2()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000017510.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.Equal(9324413, clinVarItem.Start);
            }
        }

        [Fact(Skip = "Requires a reference sequence.")]
        [Trait("jira", "NIR-2045")]
        public void AlternatePhenotype()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000032707.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.NotNull(clinVarItem.Phenotypes);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void IupacBases()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000113363.xml")), _reader, _sequence);

            var altAlleles = new List<string>();
            foreach (var clinVarItem in reader)
            {
                altAlleles.Add(clinVarItem.AltAllele);
            }

            Assert.Equal(2, altAlleles.Count);
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void OmitOmimFromAltPhenotypes()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000030349.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.Equal(1, clinVarItem.OmimIDs.Count());
            }
        }

        [Fact(Skip = "Requires a reference sequence.")]
        [Trait("jira", "NIR-2099")]
        public void ClinvarInsertion()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000153339.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.Equal(122318387, clinVarItem.Start);
            }
        }


        [Fact]
        public void Remove9DigitsPubmedId()
        {
            var reader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000207504.xml")), _reader, _sequence);

            foreach (var clinVarItem in reader)
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 16329078, 16372351, 19213030, 21438134, 25741868 }));
            }
        }
    }
}
