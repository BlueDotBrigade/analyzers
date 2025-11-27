namespace BlueDotBrigade.Analyzers.Utilities
{
    using System.Collections.Immutable;
    using System.Threading;

    using BlueDotBrigade.Analyzers.Dsl;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AnalyzerOptionsHelperTests
    {
        #region GetTargetFileName Tests

        [TestMethod]
        public void GetTargetFileName_ReturnsDefault_WhenNotConfigured()
        {
            var options = CreateAnalyzerOptions(
                additionalFiles: ImmutableArray<AdditionalText>.Empty,
                globalOptions: ImmutableDictionary<string, string>.Empty);

            var result = AnalyzerOptionsHelper.GetTargetFileName(options);

            Assert.AreEqual(DslDefaults.DefaultDslFileName, result);
        }

        [TestMethod]
        public void GetTargetFileName_ReturnsConfiguredName_WhenSet()
        {
            var globalOptions = ImmutableDictionary<string, string>.Empty
                .Add("build_property.AnalyzerDslFileName", "custom.xml");
            var options = CreateAnalyzerOptions(
                additionalFiles: ImmutableArray<AdditionalText>.Empty,
                globalOptions: globalOptions);

            var result = AnalyzerOptionsHelper.GetTargetFileName(options);

            Assert.AreEqual("custom.xml", result);
        }

        [TestMethod]
        public void GetTargetFileName_TrimsWhitespace_FromConfiguredName()
        {
            var globalOptions = ImmutableDictionary<string, string>.Empty
                .Add("build_property.AnalyzerDslFileName", "  custom.xml  ");
            var options = CreateAnalyzerOptions(
                additionalFiles: ImmutableArray<AdditionalText>.Empty,
                globalOptions: globalOptions);

            var result = AnalyzerOptionsHelper.GetTargetFileName(options);

            Assert.AreEqual("custom.xml", result);
        }

        [TestMethod]
        public void GetTargetFileName_ReturnsDefault_WhenConfiguredNameIsWhitespace()
        {
            var globalOptions = ImmutableDictionary<string, string>.Empty
                .Add("build_property.AnalyzerDslFileName", "   ");
            var options = CreateAnalyzerOptions(
                additionalFiles: ImmutableArray<AdditionalText>.Empty,
                globalOptions: globalOptions);

            var result = AnalyzerOptionsHelper.GetTargetFileName(options);

            Assert.AreEqual(DslDefaults.DefaultDslFileName, result);
        }

        #endregion

        #region GetProjectDirectory Tests

        [TestMethod]
        public void GetProjectDirectory_ReturnsNull_WhenNotConfigured()
        {
            var options = CreateAnalyzerOptions(
                additionalFiles: ImmutableArray<AdditionalText>.Empty,
                globalOptions: ImmutableDictionary<string, string>.Empty);

            var result = AnalyzerOptionsHelper.GetProjectDirectory(options);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetProjectDirectory_ReturnsNormalizedPath_WhenSetViaGlobalOptions()
        {
            var sep = System.IO.Path.DirectorySeparatorChar;
            var globalOptions = ImmutableDictionary<string, string>.Empty
                .Add("build_property.MSBuildProjectDirectory", $"src{sep}project");
            var options = CreateAnalyzerOptions(
                additionalFiles: ImmutableArray<AdditionalText>.Empty,
                globalOptions: globalOptions);

            var result = AnalyzerOptionsHelper.GetProjectDirectory(options);

            Assert.AreEqual($"src{sep}project", result);
        }

        #endregion

        #region SelectDslAdditionalText Tests

        [TestMethod]
        public void SelectDslAdditionalText_ReturnsNull_WhenNoFilesMatch()
        {
            var options = CreateAnalyzerOptions(
                additionalFiles: ImmutableArray<AdditionalText>.Empty,
                globalOptions: ImmutableDictionary<string, string>.Empty);

            var result = AnalyzerOptionsHelper.SelectDslAdditionalText(options, "dsl.config.xml", null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void SelectDslAdditionalText_ReturnsFirstMatch_WhenNoProjectDir()
        {
            var additionalFile = new TestAdditionalText("solution/dsl.config.xml", "<dsl />");
            var additionalFiles = ImmutableArray.Create<AdditionalText>(additionalFile);
            var options = CreateAnalyzerOptions(
                additionalFiles: additionalFiles,
                globalOptions: ImmutableDictionary<string, string>.Empty);

            var result = AnalyzerOptionsHelper.SelectDslAdditionalText(options, "dsl.config.xml", null);

            Assert.IsNotNull(result);
            Assert.AreEqual("solution/dsl.config.xml", result.Path);
        }

        #endregion

        #region Helper Methods

        private static AnalyzerOptions CreateAnalyzerOptions(
            ImmutableArray<AdditionalText> additionalFiles,
            ImmutableDictionary<string, string> globalOptions)
        {
            var configOptionsProvider = new TestAnalyzerConfigOptionsProvider(globalOptions);
            return new AnalyzerOptions(additionalFiles, configOptionsProvider);
        }

        #endregion

        #region Test Doubles

        private sealed class TestAdditionalText : AdditionalText
        {
            private readonly string _path;
            private readonly SourceText _text;

            public TestAdditionalText(string path, string content)
            {
                _path = path;
                _text = SourceText.From(content);
            }

            public override string Path => _path;

            public override SourceText GetText(CancellationToken cancellationToken = default)
            {
                return _text;
            }
        }

        private sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
        {
            private readonly TestAnalyzerConfigOptions _globalOptions;

            public TestAnalyzerConfigOptionsProvider(ImmutableDictionary<string, string> options)
            {
                _globalOptions = new TestAnalyzerConfigOptions(options);
            }

            public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
            {
                return _globalOptions;
            }

            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
            {
                return _globalOptions;
            }
        }

        private sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
        {
            private readonly ImmutableDictionary<string, string> _options;

            public TestAnalyzerConfigOptions(ImmutableDictionary<string, string> options)
            {
                _options = options;
            }

            public override bool TryGetValue(string key, out string value)
            {
                return _options.TryGetValue(key, out value);
            }
        }

        #endregion
    }
}
