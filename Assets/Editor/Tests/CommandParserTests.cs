using Assets.Scripts.Scripting;
using NUnit.Framework;
using System.Linq;

// EditMode tests for the strict command parser. Pure text in/out -- no scene, no microphone.
public class CommandParserTests
{
    private static string[] Parse(string text) => CommandParser.Parse(text).Commands.Select(c => c.ToString()).ToArray();

    [Test]
    public void SeparatesOnSemicolonsLinesAndConnectors()
    {
        CollectionAssert.AreEqual(new[] { "reset", "show heart", "focus", "rotate slow" },
            Parse("reset\nshow heart; focus then rotate slow"));
    }

    [Test]
    public void NonVerbSegmentJoinsPreviousCommand()
    {
        var commands = CommandParser.Parse("show heart and lungs").Commands;
        Assert.AreEqual(1, commands.Count);
        CollectionAssert.AreEqual(new[] { "heart", "lungs" }, commands[0].Items);
    }

    [Test]
    public void CommasListItems()
    {
        var commands = CommandParser.Parse("show heart, aorta, vena cava").Commands;
        CollectionAssert.AreEqual(new[] { "heart", "aorta", "vena cava" }, commands[0].Items);
    }

    [Test]
    public void IgnoresLineNumbersAndComments()
    {
        CollectionAssert.AreEqual(new[] { "reset", "wait 3" }, Parse("1 reset   # start clean\n2. wait 3\n# whole-line comment"));
    }

    [Test]
    public void FoldsNumberWords()
    {
        var commands = CommandParser.Parse("slice at forty; rotate twenty five degrees; zoom in one hundred").Commands;
        Assert.IsTrue(commands[0].TryGetNumber(out float a)); Assert.AreEqual(40, a);
        Assert.IsTrue(commands[1].TryGetNumber(out float b)); Assert.AreEqual(25, b);
        Assert.IsTrue(commands[2].TryGetNumber(out float c)); Assert.AreEqual(100, c);
    }

    [Test]
    public void AppliesPhraseRewritesAndAliases()
    {
        CollectionAssert.AreEqual(new[] { "rotate slow", "slice slow", "slice off", "view front", "rotate 90", "reset" },
            Parse("slow rotate; slow slice; remove slice; front; spin 90; start over"));
    }

    [Test]
    public void LongestVerbWins()
    {
        var commands = CommandParser.Parse("focus in").Commands;
        Assert.AreEqual("focus", commands[0].Verb);
        Assert.AreEqual("", commands[0].Args);
    }

    [Test]
    public void TrailingAmpersandRunsInParallel()
    {
        var commands = CommandParser.Parse("rotate slow &\nslice at 40").Commands;
        Assert.IsTrue(commands[0].Parallel);
        Assert.IsFalse(commands[1].Parallel);
    }

    [Test]
    public void QuotedTextIsKeptAndNotSplit()
    {
        var commands = CommandParser.Parse("say \"show the heart and rotate; then zoom\"").Commands;
        Assert.AreEqual(1, commands.Count);
        Assert.AreEqual("show the heart and rotate; then zoom", commands[0].Quoted[0]);
    }

    [Test]
    public void UnknownVerbIsReportedWithLineNumberAndSuggestion()
    {
        var result = CommandParser.Parse("reset\nrotat slow");
        Assert.AreEqual(1, result.Commands.Count);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual(2, result.Errors[0].Line);
        StringAssert.Contains("rotate", result.Errors[0].Message);
    }

    [Test]
    public void InvalidArgumentsFailValidation()
    {
        var result = CommandParser.Parse("wait; slice at; pan sideways; zoom");
        Assert.AreEqual(0, result.Commands.Count);
        Assert.AreEqual(4, result.Errors.Count);
    }

    [Test]
    public void EveryVerbHasHelp()
    {
        foreach (var command in CommandRegistry.All)
            Assert.IsFalse(string.IsNullOrEmpty(command.Info.Help), command.Info.Verb + " has no Help text");
    }
}
