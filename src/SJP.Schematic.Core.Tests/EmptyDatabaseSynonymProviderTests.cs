﻿using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class EmptyDatabaseSynonymProviderTests
    {
        [Test]
        public static void GetSynonym_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyDatabaseSynonymProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetSynonym(null));
        }

        [Test]
        public static void GetSynonymAsync_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyDatabaseSynonymProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetSynonymAsync(null));
        }

        [Test]
        public static void GetSynonym_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyDatabaseSynonymProvider();
            var synonym = provider.GetSynonym("synonym_name");

            Assert.IsTrue(synonym.IsNone);
        }

        [Test]
        public static async Task GetSynonymAsync_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyDatabaseSynonymProvider();
            var synonym = provider.GetSynonymAsync("synonym_name");
            var synonymIsNone = await synonym.IsNone.ConfigureAwait(false);

            Assert.IsTrue(synonymIsNone);
        }

        [Test]
        public static void Synonyms_PropertyGet_HasCountOfZero()
        {
            var provider = new EmptyDatabaseSynonymProvider();
            var synonyms = provider.Synonyms;

            Assert.Zero(synonyms.Count);
        }

        [Test]
        public static void Synonyms_WhenEnumerated_ContainsNoValues()
        {
            var provider = new EmptyDatabaseSynonymProvider();
            var synonyms = provider.Synonyms.ToList();

            Assert.Zero(synonyms.Count);
        }

        [Test]
        public static async Task SynonymsAsync_PropertyGet_HasCountOfZero()
        {
            var provider = new EmptyDatabaseSynonymProvider();
            var synonyms = await provider.SynonymsAsync().ConfigureAwait(false);

            Assert.Zero(synonyms.Count);
        }

        [Test]
        public static async Task SynonymsAsync_WhenEnumerated_ContainsNoValues()
        {
            var provider = new EmptyDatabaseSynonymProvider();
            var synonyms = await provider.SynonymsAsync().ConfigureAwait(false);
            var synonymsList = synonyms.ToList();

            Assert.Zero(synonymsList.Count);
        }
    }
}
