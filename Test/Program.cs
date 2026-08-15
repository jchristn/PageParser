using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PageParser;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                while (true)
                {
                    Console.Write("URL: ");
                    string url = Console.ReadLine();
                    if (String.IsNullOrEmpty(url)) continue;

                    HtmlPageParser p = new HtmlPageParser(url);

                    string source = p.GetRawSource();
                    string title = p.GetPageTitle();
                    string desc = p.GetMetaDescription();
                    string keywords = p.GetMetaKeywords();
                    string htmlBody = p.GetHtmlBody();
                    string htmlStrippedBody = p.GetHtmlBodyWithoutJsCss();
                    List<string> tokens = p.GetTokens();
                    Dictionary<string, int> tokensFreq = p.GetTokensWithFreq(3, 3, true, true, true);
                    List<string> imageUrls = p.GetImageUrls();
                    string openGraphImage = p.GetMetaImageOpenGraph();
                    string openGraphDescription = p.GetMetaDescriptionOpenGraph();
                    List<string> openGraphVideoTags = p.GetMetaVideoTagsOpenGraph();

                    // Console.WriteLine("Source         : " + source);
                    Console.WriteLine("Title          : " + title);
                    Console.WriteLine("Description    : " + desc);
                    Console.WriteLine("Keywords       : " + keywords);
                    Console.WriteLine("Image URLs     : " + imageUrls.Count);
                    foreach (string curr in imageUrls) Console.WriteLine("  " + curr);
                    Console.WriteLine("OG Image       : " + openGraphImage);
                    Console.WriteLine("OG Description : " + openGraphDescription);
                    Console.WriteLine("OG Video Tags  : " + openGraphVideoTags.Count);
                    foreach (string curr in openGraphVideoTags) Console.WriteLine("  " + curr);
                    // Console.WriteLine("HTML Body     : " + htmlBody);
                    // Console.WriteLine("HTML Stripped : " + htmlStrippedBody);

                    Console.WriteLine("Tokens         : " + tokens.Count);
                    // foreach (string curr in tokens) Console.Write(curr + " ");
                    // Console.WriteLine("");

                    Console.WriteLine("Tokens w/ Freq : " + tokensFreq.Count);
                    foreach (KeyValuePair<string, int> currToken in tokensFreq) Console.Write(currToken.Key + ":" + currToken.Value + " ");
                    Console.WriteLine("");
                }
            }
            catch (Exception e)
            {
                PrintException(e);
            }
            finally
            {
                Console.WriteLine("");
                Console.Write("Press ENTER to exit.");
                Console.ReadLine();
            }
        }

        static void PrintException(Exception e)
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine(" = Exception Type: " + e.GetType().ToString());
            Console.WriteLine(" = Exception Dat " + e.Data);
            Console.WriteLine(" = Inner Exception: " + e.InnerException);
            Console.WriteLine(" = Exception Message: " + e.Message);
            Console.WriteLine(" = Exception Source: " + e.Source);
            Console.WriteLine(" = Exception StackTrace: " + e.StackTrace);
            Console.WriteLine("================================================================================");
        }
    }
}
