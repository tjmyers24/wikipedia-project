using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace web_scraping
{
    class Program
    {
        /*
         * Figure out if the link is in parenthesis or if it is italicized, if they are return false
         * Otherwise return true
         */
        public static bool IsFirstLink(HtmlNode htmlLink)
        {
            int numRight = 0;
            int numLeft = 0;

            string strPreviousText = "";
            string strTemp = "";



            HtmlNode prevNode = htmlLink.PreviousSibling;

            while (prevNode != null)
            {
                if (!prevNode.Name.Equals("#text"))
                {
                    prevNode = prevNode.PreviousSibling;
                    continue;
                }

                // build previous text
                strTemp = strPreviousText;
                strPreviousText = prevNode.InnerText + strTemp;

                numRight = strPreviousText.Count(s => (s == ')'));
                numLeft = strPreviousText.Count(s => (s == '('));

                // link is in parenthesis
                if (numLeft > numRight)
                {
                    return false;
                }

                prevNode = prevNode.PreviousSibling;
            }

            return true;
        }


        // Clicking on the first non - parenthesized, non - italicized link
        // Ignoring external links, links to the current page, or red links(links to non-existent pages)
        // Stopping when reaching "Philosophy", a page with no links or a page that does not exist, or when a loop occurs
        static void Main(string[] args)
        {

            // 'global' variables
            List<string> strlstPages = new List<string>();

            string strNodeClass = "mw-parser-output";
            //string strXPath = $"//*[@class=\"{class_name}\"]";
            string strParentID = "mw-content-text";
            string strXPath = $"//div[@id=\"{strParentID}\"]/div[@class=\"{strNodeClass}\"]"; // wikipedia is dumb

            string strSiteUrl = "https://en.wikipedia.org";
            string strSitePageUrl = strSiteUrl + "/wiki/";
            string strPageTitle;

            bool blnFound = false;
            bool blnDeadEnd = false;

            HtmlNode articleBody;
            HtmlNode firstLink = null;
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc;

            strSitePageUrl += Console.ReadLine();

            do
            {
                blnFound = false;
                doc = web.Load(strSitePageUrl);

                HtmlNode titleNode = doc.DocumentNode.SelectSingleNode($"//title");

                strPageTitle = titleNode.InnerHtml.Replace(" - Wikipedia", "");

                strlstPages.Add(strPageTitle);

                // look for repeated page titles - stuck in loop
                // Bob_Dylan
                if (strlstPages.Count(s => s.Equals(strPageTitle)) > 1)
                {
                    break;
                }

                if (strPageTitle.Equals("Philosophy"))
                {
                    break;
                }

                // until I get better at XPaths this is the best I can do
                articleBody = doc.DocumentNode.SelectSingleNode(strXPath);

                // change this to accomodate lists -> see wiki/Branches_of_science
                if (articleBody == null)
                {
                    break;
                }

                IEnumerable<HtmlNode> paragraphs = articleBody.ChildNodes.Where(c => c.Name.Equals("p"));

                // run through each paragraph and look for first link that follows the rules listed above
                foreach (HtmlNode paragraph in paragraphs)
                {
                    var links = paragraph.ChildNodes.Where(c => c.Name.Equals("a"));
                    // check each link and see if it follows the rules above
                    foreach (HtmlNode link in links)
                    {
                        blnFound = IsFirstLink(link);
                        if (blnFound)
                        {
                            firstLink = link;
                            break;
                        }
                    }
                    if (blnFound)
                    {
                        break;
                    }
                }


                if (firstLink != null)
                {
                    strSitePageUrl = strSiteUrl + firstLink.Attributes["href"].Value;
                }
                else
                {
                    blnDeadEnd = true;
                }

                // check exit conditions
            } while (!blnDeadEnd);

            Console.Write("Starting Page - ");
            foreach (string strTitle in strlstPages)
            {
                Console.WriteLine(strTitle);
            }
            if (blnDeadEnd)
            {
                Console.WriteLine("Dead End");
            }
            else if (strlstPages.Contains("Philosophy") == false)
            {
                Console.WriteLine("Infinite Loop Encountered");
            }
            else
            {
                Console.WriteLine($"{strlstPages.Count - 1} degrees of seperation.");
            }
            Console.ReadLine();
        }
    }
}
