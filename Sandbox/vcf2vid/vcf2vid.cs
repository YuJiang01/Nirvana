using System;
using ErrorHandling.Exceptions;
using GffComparison;
using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;

namespace vcf2vid
{
    class Vcf2Vid : AbstractCommandLineHandler
    {
        public static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "ref|r=",
                    "input compressed reference sequence {path}",
                    v => ConfigurationSettings.CompressedReferencePath = v
                },
                {
                    "vcf=",
                    "vcf {line}",
                    v => ConfigurationSettings.VcfLine = v
                },
            };

            var commandLineExample = "-r <reference path> --vcf <vcf line>";

            var vid = new Vcf2Vid("Generates VIDs given a VCF line", ops, commandLineExample, Constants.Authors);
            vid.Execute(args);
            return vid.ExitCode;
        }

        /// <summary>
        /// constructor
        /// </summary>
        private Vcf2Vid(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.CompressedReferencePath, "reference", "--ref");
            HasRequiredParameter(ConfigurationSettings.VcfLine, "vcf line", "--vcf");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            CompressedSequence compressedSequence;
            DataFileManager dataFileManager;

            using (var reader = FileUtilities.GetReadStream(ConfigurationSettings.CompressedReferencePath))
            {
                compressedSequence           = new CompressedSequence();
                var compressedSequenceReader = new CompressedSequenceReader(reader, compressedSequence);
                dataFileManager              = new DataFileManager(compressedSequenceReader, compressedSequence);
            }

            var vid            = new VID();
            var variant        = CreateVcfVariant(ConfigurationSettings.VcfLine);
            var variantFeature = new VariantFeature(variant, compressedSequence.Renamer, vid);

            // load the reference sequence
            dataFileManager.LoadReference(variantFeature.ReferenceIndex, () => {});

            variantFeature.AssignAlternateAlleles();
            foreach(var altAllele in variantFeature.AlternateAlleles) Console.WriteLine(altAllele.VariantId);
        }

        private static VcfVariant CreateVcfVariant(string vcfLine)
        {
            var fields = vcfLine.Split('\t');

            if (fields.Length < VcfCommon.MinNumColumns)
            {
                throw new UserErrorException($"Expected at least {VcfCommon.MinNumColumns} tab-delimited columns in the VCF line, but found only {fields.Length}");
            }

            return new VcfVariant(fields, vcfLine, false);
        }
    }
}