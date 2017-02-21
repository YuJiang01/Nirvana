using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.DataDumperImport.Parser;
using VariantAnnotation.DataStructures;
using ErrorHandling.Exceptions;
using Transcript = CacheUtils.DataDumperImport.DataStructures.Transcript;

namespace CacheUtils.DataDumperImport.Utilities
{
    public sealed class TranscriptMerger
    {
        private readonly string[] _whiteListPrefixes;

        /// <summary>
        /// constructor
        /// </summary>
        public TranscriptMerger(string[] whiteList)
        {
            _whiteListPrefixes = whiteList;
        }

        public void Merge(ImportDataStore originalDataStore, ImportDataStore mergedDataStore,
            FeatureStatistics statistics)
        {
            var transcriptDict = GetMergedTranscripts(originalDataStore);
            mergedDataStore.Transcripts.AddRange(transcriptDict.Values.ToList());
            statistics.Increment(mergedDataStore.Transcripts.Count, originalDataStore.Transcripts.Count);
        }

        /// <summary>
        /// returns true if the transcript contains a prefix from the specified list
        /// </summary>
        private bool FoundPrefix(string transcriptId)
        {
            // disable filtering if the whitelist is null
            if (_whiteListPrefixes == null) return true;

            return _whiteListPrefixes.Any(transcriptId.StartsWith);
        }

        private Dictionary<string, Transcript> GetMergedTranscripts(ImportDataStore other)
        {
            var transcriptDict = new Dictionary<string, Transcript>();

            foreach (var transcript in other.Transcripts)
            {
                if (string.IsNullOrEmpty(transcript.StableId))
                {
                    throw new GeneralException("Found a transcript with no ID.");
                }

                // apply whitelist filtering
                if (!FoundPrefix(transcript.StableId)) continue;

                // ignore transcripts with the name dupl
                if (transcript.StableId.Contains("dupl")) continue;

                // merge transcripts
                var transcriptKey = $"{transcript.StableId}.{transcript.Start}.{transcript.End}";
                Transcript prevTranscript;

                if (transcriptDict.TryGetValue(transcriptKey, out prevTranscript))
                {
                    MergeTranscript(prevTranscript, transcript);
                }
                else
                {
                    transcriptDict[transcriptKey] = transcript;
                }
            }

            return transcriptDict;
        }

        private static void MergeTranscript(Transcript prev, Transcript curr)
        {
            if (TranscriptEquals(prev, curr)) return;

            AddGeneSymbol(prev, curr);
            FixCanonical(prev, curr);
            FixBiotype(prev, curr);

            if (TranscriptEquals(prev, curr)) return;

            TranscriptDump(prev);
            TranscriptDump(curr);
            throw new GeneralException("Found different transcripts");
        }

        private static void FixBiotype(Transcript prev, Transcript curr)
        {
            if (prev.BioType == curr.BioType) return;

            var miRNA         = GetTranscript(BioType.miRNA, prev, curr);
            var rna           = GetTranscript(BioType.RNA, prev, curr);
            var mRNA          = GetTranscript(BioType.mRNA, prev, curr);
            var proteinCoding = GetTranscript(BioType.ProteinCoding, prev, curr);
            var rRNA          = GetTranscript(BioType.RibosomalRna, prev, curr);

            // fix the miRNA vs misc_RNA issue
            if (miRNA != null && rna != null)
            {
                rna.BioType = BioType.miRNA;
                Console.WriteLine($"--- fixed biotype: miRNA vs misc_RNA: {curr.StableId}");
            }

            // fix the mRNA vs protein_coding issue
            if (mRNA != null && proteinCoding != null)
            {
                mRNA.BioType            = proteinCoding.BioType;
                mRNA.Translation        = proteinCoding.Translation;
                mRNA.VariantEffectCache = proteinCoding.VariantEffectCache;
                mRNA.CompDnaCodingStart = proteinCoding.CompDnaCodingStart;
                mRNA.CompDnaCodingEnd   = proteinCoding.CompDnaCodingEnd;
                mRNA.ProteinId          = proteinCoding.ProteinId;
                Console.WriteLine($"--- fixed biotype: mRNA vs protein_coding: {curr.StableId}");
            }

            // fix the misc_RNA vs rRNA issue
            if (rna != null && rRNA != null)
            {
                rna.BioType = BioType.RibosomalRna;
                Console.WriteLine($"--- fixed biotype: rRNA vs misc_RNA: {curr.StableId}");
            }
        }

        private static Transcript GetTranscript(BioType bioType, Transcript prev, Transcript curr)
        {
            if (prev.BioType == bioType) return prev;
            if (curr.BioType == bioType) return curr;
            return null;
        }

        private static void FixCanonical(Transcript prev, Transcript curr)
        {
            if (prev.IsCanonical == curr.IsCanonical) return;
            if (!prev.IsCanonical) prev.IsCanonical = true;
            if (!curr.IsCanonical) curr.IsCanonical = true;
        }

        private static void AddGeneSymbol(Transcript prev, Transcript curr)
        {
            var prevHasSymbol = string.IsNullOrEmpty(prev.GeneSymbol);
            var currHasSymbol = string.IsNullOrEmpty(curr.GeneSymbol);

            if (prevHasSymbol == currHasSymbol) return;

            if (!prevHasSymbol) prev.GeneSymbol = curr.GeneSymbol;
            if (!currHasSymbol) curr.GeneSymbol = prev.GeneSymbol;
        }

        private static void TranscriptDump(Transcript t)
        {
            Console.WriteLine("==================================");
            Console.WriteLine($"ReferenceIndex:     {t.ReferenceIndex}");
            Console.WriteLine($"Start:              {t.Start}");
            Console.WriteLine($"End:                {t.End}");
            Console.WriteLine($"BioType:            {BioTypeUtilities.GetBiotypeDescription(t.BioType)}");
            Console.WriteLine($"OnReverseStrand:    {t.OnReverseStrand}");
            Console.WriteLine($"IsCanonical:        {t.IsCanonical}");
            Console.WriteLine($"CompDnaCodingStart: {t.CompDnaCodingStart}");
            Console.WriteLine($"CompDnaCodingEnd:   {t.CompDnaCodingEnd}");
            Console.WriteLine($"Version:            {t.Version}");
            Console.WriteLine($"ProteinId:          {t.ProteinId}");
            Console.WriteLine($"GeneStableId:       {t.GeneStableId}");
            Console.WriteLine($"StableId:           {t.StableId}");
            Console.WriteLine($"GeneSymbol:         {t.GeneSymbol}");
            Console.WriteLine($"HgncId:             {t.HgncId}");
            Console.WriteLine("==================================");
        }

        private static bool TranscriptEquals(Transcript prev, Transcript curr)
        {
            return prev.ReferenceIndex     == curr.ReferenceIndex     &&
                   prev.Start              == curr.Start              &&
                   prev.End                == curr.End                &&
                   prev.BioType            == curr.BioType            &&
                   prev.OnReverseStrand    == curr.OnReverseStrand    &&
                   prev.IsCanonical        == curr.IsCanonical        &&
                   prev.CompDnaCodingStart == curr.CompDnaCodingStart &&
                   prev.CompDnaCodingEnd   == curr.CompDnaCodingEnd   &&
                   prev.Version            == curr.Version            &&
                   prev.ProteinId          == curr.ProteinId          &&
                   prev.GeneStableId       == curr.GeneStableId       &&
                   prev.StableId           == curr.StableId           &&
                   prev.GeneSymbol         == curr.GeneSymbol         &&
                   prev.HgncId             == curr.HgncId;
        }
    }
}
