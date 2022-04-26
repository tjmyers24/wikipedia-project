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

            while(prevNode != null)
            {
                if(!prevNode.Name.Equals("#text"))
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
                if(numLeft > numRight)
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

            string class_name = "mw-parser-output";

            bool blnFound = false;

            bool blnDeadEnd = false;

            HtmlNode articleBody;

            HtmlNode firstLink = null;

            string strSiteUrl = "https://en.wikipedia.org";
            string strPageName = args[0];
            //string strPageName = "Hiragana";

            string strSitePageUrl = strSiteUrl + "/wiki/" + strPageName;

            string strPageTitle;

            var web = new HtmlWeb();
            HtmlDocument doc;


            // keep going through links

            do
            {
                blnFound = false;
                doc = web.Load(strSitePageUrl);

                HtmlNode titleNode = doc.DocumentNode.SelectSingleNode($"//title");

                strPageTitle = titleNode.InnerHtml.Replace(" - Wikipedia", "");

                strlstPages.Add(strPageTitle);

                if (strlstPages.Count(s => s.Equals(strPageTitle)) > 1)
                {
                    break; // stuck in loop
                }

                // second part of conditional is for the most extreme edge case the world has ever seen
                if(strPageTitle.Equals("Philosophy") && strlstPages.Count != 1)
                {
                    break;
                }

                articleBody = doc.DocumentNode.SelectSingleNode($"//*[@class=\"{class_name}\"]");

                // change this to accomodate lists -> see wiki/Branches_of_science
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


                // need to put this in a while loop and check for dead ends or infinite recursion
                if (firstLink != null)
                {
                    strSitePageUrl = strSiteUrl + firstLink.Attributes["href"].Value;
                }
                else
                {
                    // dead end
                    blnDeadEnd = true;
                }

                // check exit conditions
            } while (!blnDeadEnd);

            Console.Write("Starting Page - ");
            foreach(string strTitle in strlstPages)
            {
                Console.WriteLine(strTitle);
            }
            if(blnDeadEnd)
            {
                Console.WriteLine("Dead End");
            }
            else if(strlstPages.Contains("Philosophy") == false)
            {
                Console.WriteLine("Infinite Loop Encountered");
            }
            else
            {
                Console.WriteLine($"{strlstPages.Count - 1} degrees of seperation.");
            }
            // create data table of players
            Console.ReadLine();
        }
    }
}
