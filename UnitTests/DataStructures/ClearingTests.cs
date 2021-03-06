﻿using System.Reflection;
using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("ChromosomeRenamer")]
    public sealed class ClearingTests
    {
        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public ClearingTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

		[Fact]
		public void ClearJsonVariant()
		{
			var t = typeof (JsonVariant);

			var propInfos = t.GetProperties();

			// Assert.Equal(76, propInfos.Length);

			var jv = new JsonVariant();

			// add garbage values to a json varint object
			foreach (var propertyInfo in propInfos)
			{
				if (propertyInfo.PropertyType == typeof(string) && propertyInfo.SetMethod !=null)
					propertyInfo.SetValue(jv, "0.001");
				
			}
			
			jv.Clear();
			foreach (var propertyInfo in propInfos)
			{
				if (propertyInfo.PropertyType == typeof(string))
					Assert.Null(propertyInfo.GetValue(jv));
				
			}

			Assert.Empty(jv.ClinVarEntries);
			Assert.Empty(jv.CosmicEntries);
			Assert.Empty(jv.RegulatoryRegions);
		}

		[Fact]
		public void ClearVariantFeature()
		{
			var t = typeof(VariantFeature);

			var propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			Assert.Equal(25, propInfos.Length);

			// populating the alternate alleles array
			const string vcfLine = "17	4634317	.	C	A,T	256	PASS	SNVSB=-27.1;SNVHPOL=7	GT	1/2";

		    var variant = VcfUtilities.GetVariant(vcfLine, _renamer);

			// add garbage values to a json varint object
			foreach (var propertyInfo in propInfos)
			{
				if (propertyInfo.PropertyType == typeof(string))
					propertyInfo.SetValue(variant, "bob");
				if (propertyInfo.PropertyType == typeof(int))
					propertyInfo.SetValue(variant, 100);
				if (propertyInfo.PropertyType == typeof(bool))
					propertyInfo.SetValue(variant, true);
			}
			
			variant.Clear();
			foreach (var propertyInfo in propInfos)
			{
				if (propertyInfo.PropertyType == typeof(string))
					Assert.Null(propertyInfo.GetValue(variant));
				if (propertyInfo.PropertyType == typeof(int))
					Assert.Equal(0, propertyInfo.GetValue(variant));
				if (propertyInfo.PropertyType == typeof(bool))
					Assert.Equal(false,propertyInfo.GetValue(variant));

				if (propertyInfo.PropertyType == typeof(string[]))
					Assert.Null(propertyInfo.GetValue(variant));
			}

			Assert.Empty(variant.AlternateAlleles);
		}

		
	}
}
