using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using HtmlAgilityPack;
using NUglify;

namespace PageParser
{
    public class HtmlPageParser
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private string Url { get; set; }
        private byte[] RawData { get; set; }
        private HtmlDocument HtmlDoc { get; set; }
        private string Html { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiate the parser by retrieving the HTML at the supplied URL over HTTP.
        /// </summary>
        /// <param name="url">The URL to retrieve.</param>
        public HtmlPageParser(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            Url = url;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "text/html");

                HttpResponseMessage resp;
                try
                {
                    resp = client.GetAsync(url).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to retrieve data from server (null response)", e);
                }

                if (resp == null) throw new Exception("Unable to retrieve data from server (null response)");
                if ((int)resp.StatusCode != 200) throw new Exception("Non-200 status returned from server: " + (int)resp.StatusCode);

                RawData = resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

                if (RawData == null || RawData.Length < 1) throw new Exception("No data retrieved from server");
            }

            Html = Encoding.UTF8.GetString(RawData);
            HtmlDoc = new HtmlDocument();
            HtmlDoc.LoadHtml(Html);
        }

        /// <summary>
        /// Private constructor used by the FromHtml factory to build a parser from
        /// already-retrieved HTML, without performing any network I/O.
        /// </summary>
        private HtmlPageParser(string url, string html, byte[] rawData)
        {
            Url = url;
            Html = html;
            RawData = rawData;
            HtmlDoc = new HtmlDocument();
            HtmlDoc.LoadHtml(Html);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a parser from raw HTML without performing any network request.
        /// The optional base URL is used only to resolve relative image URLs.
        /// </summary>
        /// <param name="html">The raw HTML to parse.</param>
        /// <param name="url">Optional base URL used for resolving relative image URLs.</param>
        /// <returns>An initialized <see cref="HtmlPageParser"/>.</returns>
        public static HtmlPageParser FromHtml(string html, string url = null)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            byte[] data = Encoding.UTF8.GetBytes(html);
            return new HtmlPageParser(url, html, data);
        }

        public string GetRawSource()
        {
            if (RawData == null || RawData.Length < 1) return null;
            return Encoding.UTF8.GetString(RawData);
        }

        public string GetPageTitle()
        {
            Match m = Regex.Match(Html, @"<title>\s*(.+?)\s*</title>");
            if (m.Success) return HttpUtility.UrlDecode(m.Groups[1].Value);
            else return "";
        }

        public string GetMetaDescription()
        {
            HtmlNode mdnode = HtmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']");
            if (mdnode != null)
            {
                HtmlAttribute desc = mdnode.Attributes["content"];
                return HttpUtility.UrlDecode(desc.Value);
            }

            return null;
        }

        public string GetMetaKeywords()
        {
            try
            {
                return HttpUtility.UrlDecode(HtmlDoc.DocumentNode.SelectSingleNode("//meta[@name='keywords']").Attributes["content"].Value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetMetaImageOpenGraph()
        {
            try
            {
                return HttpUtility.UrlDecode(HtmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']").Attributes["content"].Value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetMetaDescriptionOpenGraph()
        {
            try
            {
                return HttpUtility.UrlDecode(HtmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']").Attributes["content"].Value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<string> GetMetaVideoTagsOpenGraph()
        {
            List<string> ret = new List<string>();

            try
            {
                var metas = HtmlDoc.DocumentNode.SelectNodes("//meta[@property='og:video:tag']");
                foreach (HtmlNode curr in metas)
                {
                    ret.Add(HttpUtility.UrlDecode(curr.Attributes["content"].Value));
                }

                return ret;
            }
            catch (Exception)
            {
                return ret;
            }
        }

        public string GetHtmlBody()
        {
            HtmlNode body = HtmlDoc.DocumentNode.SelectSingleNode("//body");
            if (body == null) return null;
            return body.InnerText;
        }

        public string GetHtmlBodyWithoutJsCss()
        {
            HtmlDocument removed = new HtmlDocument();
            removed.LoadHtml(Html);

            var nodes = removed.DocumentNode.SelectNodes("//script|//style");

            if (nodes != null)
            {
                foreach (var node in nodes)
                    node.ParentNode.RemoveChild(node);
            }

            return removed.DocumentNode.OuterHtml;
        }

        public List<string> GetTokens()
        {
            List<string> ret = new List<string>();
            var result = Uglify.HtmlToText(Html);
            List<string> unfiltered = result.Code.Split(' ').ToList();

            if (unfiltered != null && unfiltered.Count > 0)
            {
                foreach (string curr in unfiltered)
                {
                    if (String.IsNullOrEmpty(curr)) continue;
                    if (String.IsNullOrWhiteSpace(curr)) continue;

                    string token = "";
                    foreach (char c in curr)
                    {
                        if ((int)c < 65) continue;
                        if ((int)c > 90 && (int)c < 97) continue;
                        if ((int)c > 122) continue;
                        token += c;
                    }

                    ret.Add(token.ToLower());
                }
            }

            return ret;
        }

        public Dictionary<string, int> GetTokensWithFreq(int minLength, int minFreq, bool removeStopWords, bool removeNumbers, bool removeStopChars)
        {
            List<string> tokens = GetTokens();
            Dictionary<string, int> filtered = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            Dictionary<string, int> ret = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

            if (tokens != null && tokens.Count > 0)
            {
                //
                // build master list
                //
                foreach (string token in tokens)
                {
                    int currentCount = 0;
                    filtered.TryGetValue(token, out currentCount);

                    currentCount++;
                    filtered[token] = currentCount;
                }

                //
                // remove stop characters
                //
                if (removeStopChars)
                {
                    foreach (KeyValuePair<string, int> currKvp in filtered)
                    {
                        string key = "";
                        foreach (char c in currKvp.Key)
                        {
                            if ((int)c < 65) continue;
                            if ((int)c > 90 && (int)c < 97) continue;
                            if ((int)c > 122) continue;
                            key += c;
                        }
                        if (!String.IsNullOrEmpty(key))
                        {
                            if (ret.ContainsKey(key))
                            {
                                int currValue = ret[key];
                                ret.Remove(key);
                                ret.Add(key, currValue + currKvp.Value);
                            }
                            else
                            {
                                ret.Add(key, currKvp.Value);
                            }
                        }
                    }
                }

                //
                // filter by length
                //
                filtered = CloneDictionary(ret);
                ret = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int> currKvp in filtered)
                {
                    if (currKvp.Key.Length < minLength) continue;
                    if (currKvp.Value < minFreq) continue;
                    ret.Add(currKvp.Key, currKvp.Value);
                }

                //
                // remove stop words
                //
                if (removeStopWords)
                {
                    filtered = CloneDictionary(ret);
                    ret = new Dictionary<string, int>();

                    foreach (KeyValuePair<string, int> currKvp in filtered)
                    {
                        if (!IsStopWord(currKvp.Key)) ret.Add(currKvp.Key, currKvp.Value);
                    }
                }

                //
                // remove numbers
                //
                if (removeNumbers)
                {
                    filtered = CloneDictionary(ret);
                    ret = new Dictionary<string, int>();
                    int testInt = 0;
                    decimal testDecimal = 0m;

                    foreach (KeyValuePair<string, int> currKvp in filtered)
                    {
                        if (Int32.TryParse(currKvp.Key, out testInt)) continue;
                        if (Decimal.TryParse(currKvp.Key, out testDecimal)) continue;
                        ret.Add(currKvp.Key, currKvp.Value);
                    }
                }
            }

            ret = ret.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            return ret;
        }

        public List<string> GetImageUrls()
        {
            List<string> links = new List<string>();

            //
            // using regex
            //
            string regexImgSrc = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";
            MatchCollection matchesImgSrc = Regex.Matches(Html, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match m in matchesImgSrc)
            {
                string href = m.Groups[1].Value;
                href = href.Trim();

                string formatted = "";

                if (href.StartsWith("http"))
                {
                    formatted = href;
                }
                else if (String.IsNullOrEmpty(Url))
                {
                    //
                    // cannot resolve a relative URL without a base URL
                    //
                    continue;
                }
                else if (href.StartsWith("//"))
                {
                    if (Url.EndsWith("/"))
                    {
                        formatted = Url + href.Substring(2);
                    }
                    else
                    {
                        formatted = Url + href.Substring(1);
                    }
                }
                else if (href.StartsWith("/"))
                {
                    if (Url.EndsWith("/"))
                    {
                        formatted = Url + href.Substring(1);
                    }
                    else
                    {
                        formatted = Url + href;
                    }
                }
                else if (href.StartsWith("./"))
                {
                    if (Url.EndsWith("/"))
                    {
                        formatted = Url + href.Substring(2);
                    }
                    else
                    {
                        formatted = Url + "/" + href.Substring(2);
                    }
                }
                else
                {
                    // Console.WriteLine("Unknown URL format for image: " + href);
                    continue;
                }

                if (links.Contains(HttpUtility.UrlDecode(formatted))) continue;
                links.Add(HttpUtility.UrlDecode(formatted));
            }

            //
            // using opengraph
            //
            string openGraphImage = GetMetaImageOpenGraph();
            if (!String.IsNullOrEmpty(openGraphImage))
            {
                //
                // already URL decoded
                //
                if (!links.Contains(openGraphImage)) links.Add(openGraphImage);
            }
            return links;
        }

        #endregion

        #region Private-Methods

        private Dictionary<string, int> CloneDictionary(Dictionary<string, int> dict)
        {
            Dictionary<string, int> ret = new Dictionary<string, int>();
            if (dict == null || dict.Count < 1) return ret;

            foreach (KeyValuePair<string, int> curr in dict)
            {
                ret.Add(curr.Key, curr.Value);
            }

            return ret;
        }

        private bool IsStopWord(string word)
        {
            if (String.IsNullOrEmpty(word)) return false;
            List<string> stops = new List<string>()
            {
                "a","about","above","after","again","against","all","am","an","and","any","are","aren't","as","at","be","because","been","before","being",
                "below","between","both","but","by","can't","cannot","could","couldn't","did","didn't","do","does","doesn't","doing","don't","down","during",
                "each","few","for","from","further","had","hadn't","has","hasn't","have","haven't","having","he","he'd","he'll","he's","her","here","here's",
                "hers","herself","him","himself","his","how","how's","i","i'd","i'll","i'm","i've","if","in","into","is","isn't","it","it's","its","itself",
                "let's","me","more","most","must","mustn't","my","myself","no","nor","not","of","off","on","once","only","or","other","ought","our","ours",
                "ourselves","out","over","own","same","shall","shan't","she","she'd","she'll","she's","should","shouldn't","so","some","such","than","that",
                "that's","the","their","theirs","them","themselves","then","there","there's","these","they","they'd","they'll","they're","they've","this",
                "those","through","to","too","under","until","up","very","was","wasn't","we","we'd","we'll","we're","we've","were","weren't","what","what's",
                "when","when's","where","where's","which","while","who","who's","whom","why","why's","with","won't","would","wouldn't","you","you'd","you'll",
                "you're","you've","your","yours","yourself","yourselves"
            };
            if (stops.Contains(word.ToLower())) return true;
            return false;
        }

        #endregion

        #region Public-Static-Methods

        #endregion

        #region Private-Static-Methods

        #endregion
    }
}
