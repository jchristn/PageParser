namespace Test.Shared
{
    /// <summary>
    /// Static, self-contained HTML fixtures used by the test descriptors.
    /// Keeping the HTML inline makes every test deterministic and free of
    /// any network dependency.
    /// </summary>
    public static class Html
    {
        /// <summary>
        /// Base URL used when resolving relative image URLs.
        /// </summary>
        public const string BaseUrl = "https://example.com";

        /// <summary>
        /// A rich, well-formed document exercising most metadata getters.
        /// </summary>
        public const string Full =
@"<!DOCTYPE html>
<html>
<head>
    <title>Hello World Title</title>
    <meta name=""description"" content=""A short description of the page."" />
    <meta name=""keywords"" content=""alpha,beta,gamma"" />
    <meta property=""og:image"" content=""https://cdn.example.com/og-image.png"" />
    <meta property=""og:description"" content=""Open Graph description here."" />
    <meta property=""og:video:tag"" content=""tag-one"" />
    <meta property=""og:video:tag"" content=""tag-two"" />
    <meta property=""og:video:tag"" content=""tag-three"" />
    <style>.hidden { display: none; }</style>
    <script>var a = 1; console.log('script body');</script>
</head>
<body>
    <h1>Main Heading</h1>
    <p>hello hello world world world foo</p>
    <img src=""https://cdn.example.com/absolute.png"" alt=""absolute"" />
    <img src=""/root-relative.png"" alt=""root"" />
    <img src=""//protocol.example.com/proto.png"" alt=""proto"" />
    <img src=""./dot-relative.png"" alt=""dot"" />
    <img src=""ftp://weird.example.com/skip.png"" alt=""skip"" />
    <script>var b = 2;</script>
</body>
</html>";

        /// <summary>
        /// A minimal document with a title that has surrounding whitespace.
        /// </summary>
        public const string TitleWithWhitespace =
@"<html><head><title>   Trimmed Title   </title></head><body>x</body></html>";

        /// <summary>
        /// A document containing no title, meta, images, script, or style tags.
        /// </summary>
        public const string BareBody =
@"<html><head></head><body>just some plain body text here</body></html>";

        /// <summary>
        /// A document with a head but no body element at all.
        /// </summary>
        public const string NoBody =
@"<html><head><title>No Body</title></head></html>";

        /// <summary>
        /// A document whose body contains repeated words, stop words, and a
        /// numeric-only token, used to exercise the tokenizer.
        /// </summary>
        public const string Tokens =
@"<html><head><title>t</title></head><body>the cat and the dog cat 123 cat</body></html>";

        /// <summary>
        /// A document with a single Open Graph image but no inline images.
        /// </summary>
        public const string OnlyOgImage =
@"<html><head><meta property=""og:image"" content=""https://cdn.example.com/only-og.png"" /></head><body>x</body></html>";

        /// <summary>
        /// A completely empty string (valid input to FromHtml).
        /// </summary>
        public const string Empty = "";
    }
}
