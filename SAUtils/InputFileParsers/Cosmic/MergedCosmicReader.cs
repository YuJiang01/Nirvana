﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;

namespace SAUtils.InputFileParsers.Cosmic
{
    public sealed class MergedCosmicReader : IEnumerable<CosmicItem>
    {
        #region members

        private readonly string _vcfFileName;
        private readonly string _tsvFileName;
        private string _geneName;
        private int? _sampleCount;

        private int _mutationIdIndex = -1;
        private int _primarySiteIndex = -1;
        private int _primaryHistologyIndex = -1;
        private int _studyIdIndex = -1;

        private const string MutationIdTag = "Mutation ID";
        private const string PrimarySiteTag = "Primary site";
        private const string HistologyTag = "Primary histology";
        private const string StudyIdTag = "ID_STUDY";

        private readonly ChromosomeRenamer _renamer;
        private readonly Dictionary<string, HashSet<CosmicItem.CosmicStudy>> _studies;

        #endregion

        // constructor
        public MergedCosmicReader(string vcfFileName, string tsvFileName, ChromosomeRenamer renamer)
        {
            _vcfFileName = vcfFileName;
            _tsvFileName = tsvFileName;
            _renamer     = renamer;
            _studies     = new Dictionary<string, HashSet<CosmicItem.CosmicStudy>>();
        }

        public MergedCosmicReader()
        {
            _studies = new Dictionary<string, HashSet<CosmicItem.CosmicStudy>>();
        }

        public IEnumerator<CosmicItem> GetEnumerator()
        {
            return GetCosmicItems();
        }

        private IEnumerator<CosmicItem> GetCosmicItems()
        {
            //taking up all studies in to the dictionary
            using (var tsvReader = GZipUtilities.GetAppropriateStreamReader(_tsvFileName))
            {
                string line;
                while ((line = tsvReader.ReadLine()) != null)
                {
                    if (IsHeaderLine(line))
                        GetColumnIndexes(line);//the first line is supposed to be a the header line
                    else AddCosmicStudy(line);
                }
            }

            using (var reader = GZipUtilities.GetAppropriateStreamReader(_vcfFileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    // Skip comments.
                    if (line.StartsWith("#")) continue;
                    var cosmicItems = ExtractCosmicItems(line);
                    if (cosmicItems == null) continue;

                    foreach (var cosmicItem in cosmicItems)
                    {
                        yield return cosmicItem;
                    }
                }
            }
        }

        private void AddCosmicStudy(string line)
        {
            var columns = line.Split('\t');

            var mutationId  = columns[_mutationIdIndex];
            var studyId     = columns[_studyIdIndex];
            var primarySite = columns[_primarySiteIndex];
            var histology   = columns[_primaryHistologyIndex];

            if (string.IsNullOrEmpty(mutationId)) return;

            var study = new CosmicItem.CosmicStudy(studyId, histology, primarySite);
			HashSet<CosmicItem.CosmicStudy> studySet;
			if (_studies.TryGetValue(mutationId, out studySet))
                studySet.Add(study);
            else _studies[mutationId] = new HashSet<CosmicItem.CosmicStudy> { study };
        }

        private static bool IsHeaderLine(string line)
        {
            return line.Contains(StudyIdTag);
        }

        private void GetColumnIndexes(string headerLine)
        {
            //Gene name       Accession Number        Gene CDS length HGNC ID Sample name     ID_sample       ID_tumour       Primary site    Site subtype 1  Site subtype 2  Site subtype 3  Primary histology       Histology subtype 1     Histology subtype 2     Histology subtype 3     Genome-wide screen      Mutation ID     Mutation CDS    Mutation AA     Mutation Description    Mutation zygosity       LOH     GRCh    Mutation genome position        Mutation strand SNP     FATHMM prediction       FATHMM score    Mutation somatic status Pubmed_PMID     ID_STUDY        Sample source   Tumour origin   Age

            _mutationIdIndex       = -1;
            _studyIdIndex          = -1;
            _primarySiteIndex      = -1;
            _primaryHistologyIndex = -1;

            var columns = headerLine.Split('\t');
            for (int i = 0; i < columns.Length; i++)
            {
                switch (columns[i])
                {
                    case MutationIdTag:
                        _mutationIdIndex = i;
                        break;
                    case StudyIdTag:
                        _studyIdIndex = i;
                        break;
                    case PrimarySiteTag:
                        _primarySiteIndex = i;
                        break;
                    case HistologyTag:
                        _primaryHistologyIndex = i;
                        break;
                }
            }

            //Console.WriteLine($"TSV column indices \n MutationID:{_mutationIdIndex}, Study Id:{_studyIdIndex}, Primary Site: {_primarySiteIndex}, Histology:{_primaryHistologyIndex} ");

            if (_mutationIdIndex == -1)
                throw new InvalidDataException("Column for mutation Id could not be detected");
            if (_studyIdIndex == -1)
                throw new InvalidDataException("Column for study Id could not be detected");
            if (_primarySiteIndex == -1)
                throw new InvalidDataException("Column for primary site could not be detected");
            if (_primaryHistologyIndex == -1)
                throw new InvalidDataException("Column for primary histology could not be detected");
        }

        public List<CosmicItem> ExtractCosmicItems(string vcfLine)
        {
            var splitLine = vcfLine.Split(new[] { '\t' }, 8);

            var chromosome = splitLine[VcfCommon.ChromIndex];
            if (!InputFileParserUtilities.IsDesiredChromosome(chromosome, _renamer)) return null;

            var position   = int.Parse(splitLine[VcfCommon.PosIndex]);
            var cosmicId   = splitLine[VcfCommon.IdIndex];
            var refAllele  = splitLine[VcfCommon.RefIndex];
            var altAlleles = splitLine[VcfCommon.AltIndex].Split(',');
            var infoField  = splitLine[VcfCommon.InfoIndex];

            Clear();

            ParseInfoField(infoField);

            var cosmicItems = new List<CosmicItem>();

            foreach (var altAllele in altAlleles)
            {
                if (_studies.ContainsKey(cosmicId))
                    foreach (var study in _studies[cosmicId])
                    {
                        cosmicItems.Add(new CosmicItem(chromosome, position, cosmicId, refAllele, altAllele, _geneName, new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy(study.ID, study.Histology, study.PrimarySite) }, _sampleCount));
                    }
                else cosmicItems.Add(new CosmicItem(chromosome, position, cosmicId, refAllele, altAllele, _geneName, null, _sampleCount));
            }

            return cosmicItems;
        }

        private void Clear()
        {
            _geneName    = null;
            _sampleCount = null;
        }

        private void ParseInfoField(string infoFields)
        {
            if (infoFields == "" || infoFields == ".") return;

            var infoItems = infoFields.Split(';');
            foreach (var infoItem in infoItems)
            {
                if (string.IsNullOrEmpty(infoItem)) continue;

                var infoKeyValue = infoItem.Split('=');
                if (infoKeyValue.Length == 2)//sanity check
                {
                    var key = infoKeyValue[0];
                    var value = infoKeyValue[1];

                    SetInfoField(key, value);
                }
            }
        }

        private void SetInfoField(string vcfId, string value)
        {
            switch (vcfId)
            {
                case "GENE":
                    _geneName = value;
                    break;
                case "CNT":
                    _sampleCount = Convert.ToInt32(value);
                    break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
