using Assets.Scripts.Scripting;
using NUnit.Framework;
using System.Linq;

// EditMode tests for part-name lookup rules, on a small hand-built index (no scene needed).
public class PartIndexTests
{
    private PartIndex index;

    private PartEntry Add(string name, Side side = Side.Any, bool label = false, params string[] synonyms)
    {
        var entry = new PartEntry { Name = name, Side = side, IsLabel = label };
        index.Add(entry, name, PartIndex.PrimaryName);
        foreach (var synonym in synonyms)
            index.Add(entry, synonym, PartIndex.PrimarySynonym);
        return entry;
    }

    [SetUp]
    public void SetUp()
    {
        index = new PartIndex();
        Add("Heart", synonyms: new[] { "Cardia" });
        Add("Left ventricle");
        Add("Right ventricle");
        Add("Kidney", Side.Left);
        Add("Kidney", Side.Right);
        Add("Lungs");
        Add("Biceps brachii muscle");
        Add("Biceps femoris muscle");
        Add("Clavicle", Side.Left);
        Add("Clavicle", Side.Right);
        Add("Cranium", synonyms: new[] { "Skull" });
        Add("Femur", Side.Left);
        Add("Femur", label: true);
        // Latin "Femur" is the thigh region -- must lose to the English bone.
        var thigh = new PartEntry { Name = "Thigh" };
        index.Add(thigh, "Thigh", PartIndex.PrimaryName);
        index.Add(thigh, "Femur", PartIndex.OtherLanguage);
        Add("Inferior occipital gyrus and sulcus");
        index.AddAlias("collarbone", new[] { "clavicle" });
        index.AddAlias("heart chambers", new[] { "left ventricle", "right ventricle" });
    }

    private PartLookup Resolve(string query) => index.Resolve(query);

    [Test]
    public void ExactNameAndSynonym()
    {
        Assert.AreEqual("Heart", Resolve("heart").MatchedName);
        Assert.AreEqual("Heart", Resolve("The Heart").MatchedName);
        Assert.AreEqual("Cranium", Resolve("skull").MatchedName);
    }

    [Test]
    public void BothSidesUnlessOneIsNamed()
    {
        Assert.AreEqual(2, Resolve("kidney").Entries.Count);
        var left = Resolve("left kidney");
        Assert.AreEqual(1, left.Entries.Count);
        Assert.AreEqual(Side.Left, left.Entries[0].Side);
        Assert.AreEqual(Side.Right, Resolve("kidney right").Entries.Single().Side);
    }

    [Test]
    public void SideWordsInsideARealNameAreKept()
    {
        var lookup = Resolve("left ventricle");
        Assert.AreEqual(PartMatchKind.Exact, lookup.Kind);
        Assert.AreEqual("Left ventricle", lookup.MatchedName);
    }

    [Test]
    public void AliasesAndGroups()
    {
        Assert.AreEqual(2, Resolve("collarbone").Entries.Count);
        Assert.AreEqual(1, Resolve("left collarbone").Entries.Count);
        Assert.AreEqual("Left ventricle, Right ventricle", Resolve("heart chambers").MatchedName);
    }

    [Test]
    public void PluralAndSingular()
    {
        Assert.AreEqual("Lungs", Resolve("lung").MatchedName);
        Assert.AreEqual("Kidney", Resolve("kidneys").MatchedName);
    }

    [Test]
    public void PrimaryLanguageBeatsOtherLanguages()
    {
        Assert.AreEqual("Femur", Resolve("femur").MatchedName);
    }

    [Test]
    public void PartsBeatLabelsWithTheSameName()
    {
        Assert.IsFalse(Resolve("femur").Entries.Any(e => e.IsLabel));
    }

    [Test]
    public void FuzzySpelling()
    {
        var lookup = Resolve("hart");
        Assert.AreEqual(PartMatchKind.Fuzzy, lookup.Kind);
        Assert.AreEqual("Heart", lookup.MatchedName);
        Assert.AreEqual("Clavicle", Resolve("clavical").MatchedName);
    }

    [Test]
    public void UniquePrefixMatches()
    {
        Assert.AreEqual("Cranium", Resolve("cran").MatchedName);
    }

    [Test]
    public void AmbiguousPrefixListsCandidatesInsteadOfGuessing()
    {
        var lookup = Resolve("biceps");
        Assert.AreEqual(PartMatchKind.Ambiguous, lookup.Kind);
        Assert.IsFalse(lookup.Found);
        CollectionAssert.AreEquivalent(new[] { "Biceps brachii muscle", "Biceps femoris muscle" }, lookup.Candidates);
        StringAssert.Contains("could be", lookup.Problem);
    }

    [Test]
    public void WordInsideSeveralNamesIsAmbiguousNotAFuzzyGuess()
    {
        var lookup = Resolve("ventricle");
        Assert.AreEqual(PartMatchKind.Ambiguous, lookup.Kind);
        CollectionAssert.AreEquivalent(new[] { "Left ventricle", "Right ventricle" }, lookup.Candidates);
    }

    [Test]
    public void NamesContainingAndAreFound()
    {
        Assert.AreEqual(PartMatchKind.Exact, Resolve("inferior occipital gyrus and sulcus").Kind);
    }

    [Test]
    public void UnknownNameSuggestsClosest()
    {
        var lookup = Resolve("hearth valve");
        Assert.IsFalse(lookup.Found);
        StringAssert.Contains("no part named", lookup.Problem);
    }
}
