namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using PageParser;
    using Touchstone.Core;

    /// <summary>
    /// Central source of truth for all PageParser tests. Every test is expressed
    /// once here as a Touchstone descriptor and is exercised, unchanged, by the
    /// CLI runner (Test.Automated), the xUnit adapter (Test.Xunit), and the NUnit
    /// adapter (Test.Nunit).
    ///
    /// A test fails when its delegate throws; otherwise it passes. Assertions are
    /// expressed with the framework-agnostic <see cref="Check"/> helper.
    /// </summary>
    public static class PageParserSuites
    {
        #region Suite-Ids

        private const string ConstructionSuite = "Construction";
        private const string RawSourceSuite = "RawSource";
        private const string TitleSuite = "Title";
        private const string MetaSuite = "Meta";
        private const string BodySuite = "Body";
        private const string TokensSuite = "Tokens";
        private const string ImagesSuite = "Images";

        #endregion

        #region Public-Members

        /// <summary>
        /// All suites, exposed to every runner/adapter.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return new List<TestSuiteDescriptor>
                {
                    ConstructionSuiteDescriptor(),
                    RawSourceSuiteDescriptor(),
                    TitleSuiteDescriptor(),
                    MetaSuiteDescriptor(),
                    BodySuiteDescriptor(),
                    TokensSuiteDescriptor(),
                    ImagesSuiteDescriptor(),
                };
            }
        }

        #endregion

        #region Construction

        private static TestSuiteDescriptor ConstructionSuiteDescriptor()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(ConstructionSuite, "NullUrlThrows", "Constructor with null URL throws ArgumentNullException", () =>
                {
                    Check.Throws<ArgumentNullException>(() => { var _ = new HtmlPageParser((string)null); },
                        "Null URL should throw ArgumentNullException");
                }),

                Case(ConstructionSuite, "EmptyUrlThrows", "Constructor with empty URL throws ArgumentNullException", () =>
                {
                    Check.Throws<ArgumentNullException>(() => { var _ = new HtmlPageParser(String.Empty); },
                        "Empty URL should throw ArgumentNullException");
                }),

                Case(ConstructionSuite, "FromHtmlNullThrows", "FromHtml with null HTML throws ArgumentNullException", () =>
                {
                    Check.Throws<ArgumentNullException>(() => { var _ = HtmlPageParser.FromHtml(null); },
                        "Null HTML should throw ArgumentNullException");
                }),

                Case(ConstructionSuite, "FromHtmlValid", "FromHtml with valid HTML returns an instance", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Full, Html.BaseUrl);
                    Check.NotNull(parser, "FromHtml should return a non-null parser");
                }),

                Case(ConstructionSuite, "FromHtmlEmptyValid", "FromHtml with empty (but non-null) HTML returns an instance", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Empty);
                    Check.NotNull(parser, "FromHtml with empty string should return a non-null parser");
                }),

                Case(ConstructionSuite, "LiveNetworkFetch", "Constructor retrieves a live page over HTTP",
                    () =>
                    {
                        HtmlPageParser parser = new HtmlPageParser("https://www.example.com");
                        Check.NotNull(parser.GetRawSource(), "Live fetch should return raw source");
                    },
                    skip: true,
                    skipReason: "Requires live network access; the network path is exercised by the interactive Test console app."),
            };

            return new TestSuiteDescriptor(ConstructionSuite, "Construction & Factory", cases);
        }

        #endregion

        #region RawSource

        private static TestSuiteDescriptor RawSourceSuiteDescriptor()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(RawSourceSuite, "RoundTrips", "GetRawSource round-trips the supplied HTML", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Full, Html.BaseUrl);
                    Check.Equal(Html.Full, parser.GetRawSource(), "Raw source should equal the supplied HTML");
                }),

                Case(RawSourceSuite, "EmptyReturnsNull", "GetRawSource returns null for empty HTML", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Empty);
                    Check.Null(parser.GetRawSource(), "Raw source of empty HTML should be null");
                }),
            };

            return new TestSuiteDescriptor(RawSourceSuite, "Raw Source", cases);
        }

        #endregion

        #region Title

        private static TestSuiteDescriptor TitleSuiteDescriptor()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(TitleSuite, "Present", "GetPageTitle returns the title text", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Full);
                    Check.Equal("Hello World Title", parser.GetPageTitle(), "Title should be extracted");
                }),

                Case(TitleSuite, "WhitespaceTrimmed", "GetPageTitle trims surrounding whitespace", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.TitleWithWhitespace);
                    Check.Equal("Trimmed Title", parser.GetPageTitle(), "Title should be trimmed");
                }),

                Case(TitleSuite, "AbsentReturnsEmpty", "GetPageTitle returns empty string when no title", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.BareBody);
                    Check.NullOrEmpty(parser.GetPageTitle(), "Title should be empty when absent");
                }),
            };

            return new TestSuiteDescriptor(TitleSuite, "Page Title", cases);
        }

        #endregion

        #region Meta

        private static TestSuiteDescriptor MetaSuiteDescriptor()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                // Positive cases
                Case(MetaSuite, "DescriptionPresent", "GetMetaDescription returns content", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Full);
                    Check.Equal("A short description of the page.", parser.GetMetaDescription(), "Meta description should be extracted");
                }),

                Case(MetaSuite, "KeywordsPresent", "GetMetaKeywords returns content", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Full);
                    Check.Equal("alpha,beta,gamma", parser.GetMetaKeywords(), "Meta keywords should be extracted");
                }),

                Case(MetaSuite, "OgImagePresent", "GetMetaImageOpenGraph returns content", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Full);
                    Check.Equal("https://cdn.example.com/og-image.png", parser.GetMetaImageOpenGraph(), "og:image should be extracted");
                }),

                Case(MetaSuite, "OgDescriptionPresent", "GetMetaDescriptionOpenGraph returns content", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Full);
                    Check.Equal("Open Graph description here.", parser.GetMetaDescriptionOpenGraph(), "og:description should be extracted");
                }),

                Case(MetaSuite, "OgVideoTagsPresent", "GetMetaVideoTagsOpenGraph returns all tags", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Full);
                    List<string> tags = parser.GetMetaVideoTagsOpenGraph();
                    Check.Equal(3, tags.Count, "Should extract three og:video:tag values");
                    Check.Contains("tag-one", tags, "og:video:tag should contain tag-one");
                    Check.Contains("tag-two", tags, "og:video:tag should contain tag-two");
                    Check.Contains("tag-three", tags, "og:video:tag should contain tag-three");
                }),

                // Negative cases
                Case(MetaSuite, "DescriptionAbsent", "GetMetaDescription returns null when absent", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.BareBody);
                    Check.Null(parser.GetMetaDescription(), "Meta description should be null when absent");
                }),

                Case(MetaSuite, "KeywordsAbsent", "GetMetaKeywords returns null when absent", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.BareBody);
                    Check.Null(parser.GetMetaKeywords(), "Meta keywords should be null when absent");
                }),

                Case(MetaSuite, "OgImageAbsent", "GetMetaImageOpenGraph returns null when absent", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.BareBody);
                    Check.Null(parser.GetMetaImageOpenGraph(), "og:image should be null when absent");
                }),

                Case(MetaSuite, "OgDescriptionAbsent", "GetMetaDescriptionOpenGraph returns null when absent", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.BareBody);
                    Check.Null(parser.GetMetaDescriptionOpenGraph(), "og:description should be null when absent");
                }),

                Case(MetaSuite, "OgVideoTagsAbsent", "GetMetaVideoTagsOpenGraph returns empty list when absent", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.BareBody);
                    List<string> tags = parser.GetMetaVideoTagsOpenGraph();
                    Check.NotNull(tags, "og:video:tag list should never be null");
                    Check.Equal(0, tags.Count, "og:video:tag list should be empty when absent");
                }),
            };

            return new TestSuiteDescriptor(MetaSuite, "Metadata", cases);
        }

        #endregion

        #region Body

        private static TestSuiteDescriptor BodySuiteDescriptor()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(BodySuite, "BodyPresent", "GetHtmlBody returns the body inner text", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Full);
                    string body = parser.GetHtmlBody();
                    Check.NotNull(body, "Body should not be null");
                    Check.StringContains("Main Heading", body, "Body should contain the heading text");
                }),

                Case(BodySuite, "BodyAbsentReturnsNull", "GetHtmlBody returns null when no body element", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.NoBody);
                    Check.Null(parser.GetHtmlBody(), "Body should be null when no body element present");
                }),

                Case(BodySuite, "StripsScriptAndStyle", "GetHtmlBodyWithoutJsCss removes script and style content", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Full);
                    string stripped = parser.GetHtmlBodyWithoutJsCss();
                    Check.NotNull(stripped, "Stripped HTML should not be null");
                    Check.StringContains("Main Heading", stripped, "Stripped HTML should retain visible content");
                    Check.StringDoesNotContain("console.log", stripped, "Stripped HTML should not contain script content");
                    Check.StringDoesNotContain("display: none", stripped, "Stripped HTML should not contain style content");
                }),

                Case(BodySuite, "NoScriptOrStyleDoesNotThrow", "GetHtmlBodyWithoutJsCss handles HTML without script/style", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.BareBody);
                    string stripped = parser.GetHtmlBodyWithoutJsCss();
                    Check.NotNull(stripped, "Stripped HTML should not be null");
                    Check.StringContains("plain body text", stripped, "Stripped HTML should retain the original body text");
                }),
            };

            return new TestSuiteDescriptor(BodySuite, "Body Extraction", cases);
        }

        #endregion

        #region Tokens

        private static TestSuiteDescriptor TokensSuiteDescriptor()
        {
            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(TokensSuite, "BasicTokens", "GetTokens returns lowercased word tokens", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Tokens);
                    List<string> tokens = parser.GetTokens();
                    Check.Contains("cat", tokens, "Tokens should contain 'cat'");
                    Check.Contains("dog", tokens, "Tokens should contain 'dog'");
                    int catCount = tokens.FindAll(t => t == "cat").Count;
                    Check.Equal(3, catCount, "'cat' should appear three times");
                }),

                Case(TokensSuite, "EmptyBodyNoTokens", "GetTokens returns an empty list for empty HTML", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Empty);
                    List<string> tokens = parser.GetTokens();
                    Check.NotNull(tokens, "Tokens list should never be null");
                    Check.Equal(0, tokens.Count, "Empty HTML should yield no tokens");
                }),

                Case(TokensSuite, "FrequencyAndMinFreq", "GetTokensWithFreq counts frequency and honors minFreq", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Tokens);
                    Dictionary<string, int> freq = parser.GetTokensWithFreq(3, 2, false, false, true);
                    Check.True(freq.ContainsKey("cat"), "Result should contain 'cat'");
                    Check.Equal(3, freq["cat"], "'cat' frequency should be 3");
                    Check.True(freq.ContainsKey("the"), "Result should contain 'the'");
                    Check.Equal(2, freq["the"], "'the' frequency should be 2");
                    Check.False(freq.ContainsKey("dog"), "'dog' (freq 1) should be excluded by minFreq");
                    Check.False(freq.ContainsKey("and"), "'and' (freq 1) should be excluded by minFreq");
                }),

                Case(TokensSuite, "MinLengthFilter", "GetTokensWithFreq honors minLength", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Tokens);
                    Dictionary<string, int> freq = parser.GetTokensWithFreq(4, 1, false, false, true);
                    Check.Equal(0, freq.Count, "No token in the fixture is 4+ characters");
                }),

                Case(TokensSuite, "RemoveStopWords", "GetTokensWithFreq removes stop words when requested", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Tokens);
                    Dictionary<string, int> freq = parser.GetTokensWithFreq(1, 1, true, false, true);
                    Check.False(freq.ContainsKey("the"), "'the' is a stop word and should be removed");
                    Check.False(freq.ContainsKey("and"), "'and' is a stop word and should be removed");
                    Check.True(freq.ContainsKey("cat"), "'cat' is not a stop word and should remain");
                    Check.True(freq.ContainsKey("dog"), "'dog' is not a stop word and should remain");
                }),

                Case(TokensSuite, "NumericTokensStripped", "GetTokensWithFreq excludes numeric tokens", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Tokens);
                    Dictionary<string, int> freq = parser.GetTokensWithFreq(1, 1, false, true, true);
                    Check.False(freq.ContainsKey("123"), "Numeric token '123' should not survive tokenization/filtering");
                    Check.True(freq.ContainsKey("cat"), "'cat' should still be present");
                }),
            };

            return new TestSuiteDescriptor(TokensSuite, "Tokenization", cases);
        }

        #endregion

        #region Images

        private static TestSuiteDescriptor ImagesSuiteDescriptor()
        {
            const string dedupeHtml =
                "<html><body>" +
                "<img src=\"https://cdn.example.com/dup.png\" />" +
                "<img src=\"https://cdn.example.com/dup.png\" />" +
                "</body></html>";

            List<TestCaseDescriptor> cases = new List<TestCaseDescriptor>
            {
                Case(ImagesSuite, "AllResolvedForms", "GetImageUrls resolves absolute, root-, protocol-, and dot-relative URLs plus og:image", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Full, Html.BaseUrl);
                    List<string> urls = parser.GetImageUrls();
                    Check.Equal(5, urls.Count, "Should resolve five image URLs (ftp scheme skipped)");
                    Check.Contains("https://cdn.example.com/absolute.png", urls, "Absolute image URL should be present");
                    Check.Contains("https://example.com/root-relative.png", urls, "Root-relative image URL should be resolved");
                    Check.Contains("https://example.com/protocol.example.com/proto.png", urls, "Protocol-relative image URL should be resolved");
                    Check.Contains("https://example.com/dot-relative.png", urls, "Dot-relative image URL should be resolved");
                    Check.Contains("https://cdn.example.com/og-image.png", urls, "og:image should be included");
                }),

                Case(ImagesSuite, "UnknownSchemeSkipped", "GetImageUrls skips unsupported URL schemes", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.Full, Html.BaseUrl);
                    List<string> urls = parser.GetImageUrls();
                    foreach (string u in urls)
                        Check.StringDoesNotContain("skip.png", u, "ftp-scheme image should be skipped");
                }),

                Case(ImagesSuite, "Deduplicates", "GetImageUrls removes duplicate image URLs", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(dedupeHtml, Html.BaseUrl);
                    List<string> urls = parser.GetImageUrls();
                    Check.Equal(1, urls.Count, "Duplicate image URLs should be collapsed to one");
                }),

                Case(ImagesSuite, "OnlyOgImage", "GetImageUrls returns og:image when no inline images exist", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.OnlyOgImage, Html.BaseUrl);
                    List<string> urls = parser.GetImageUrls();
                    Check.Equal(1, urls.Count, "Only the og:image should be returned");
                    Check.Contains("https://cdn.example.com/only-og.png", urls, "og:image should be present");
                }),

                Case(ImagesSuite, "NoImagesEmpty", "GetImageUrls returns an empty list when there are no images", () =>
                {
                    HtmlPageParser parser = HtmlPageParser.FromHtml(Html.BareBody, Html.BaseUrl);
                    List<string> urls = parser.GetImageUrls();
                    Check.NotNull(urls, "Image URL list should never be null");
                    Check.Equal(0, urls.Count, "No images should yield an empty list");
                }),
            };

            return new TestSuiteDescriptor(ImagesSuite, "Image URLs", cases);
        }

        #endregion

        #region Private-Helpers

        private static TestCaseDescriptor Case(
            string suiteId,
            string caseId,
            string displayName,
            Action body,
            bool skip = false,
            string skipReason = null)
        {
            return new TestCaseDescriptor(
                suiteId,
                caseId,
                displayName,
                ct => { body(); return Task.CompletedTask; },
                tags: null,
                skip: skip,
                skipReason: skipReason);
        }

        #endregion
    }
}
