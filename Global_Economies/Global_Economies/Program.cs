/*
 * Program:    Global_Economies.exe
 * Module:      Program.cs
 * Course:       INFO-3138
 * Date:           Aug 03, 2022
 * Description:  A complete version of Global Economies program 
 *                      using XPath with C# to report the given XML File    
 */

using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;   // XmlReader (parser) and XmlDocument (DOM) classes
using System.Xml.XPath;
using System.Collections.Generic;


namespace Global_Economies 
{
    internal class Program
    {
        const string XML_FILE = @"global_economies.xml"; // A copy of the file is in bin\Debug\net6.0
        public static List<string> _regions = new List<string>();
        public static List<string> _MetricList = new List<string>();
        public static List<string> _labels = new List<string>();
        static void Main(string[] args)
        {
            //create necessary variables
            int yearBegin = 2017;
            int yearEnd = 2021;

            try
            {
                //using XPath  to get regions' names from regions elements of the XML file
                XmlDocument xmlFile = new XmlDocument();
                xmlFile.Load(XML_FILE);
                XmlNodeList regionNodes = xmlFile.GetElementsByTagName("region");
                foreach (XmlElement element in regionNodes)
                {
                    //append the value of rname to _regions
                    _regions.Add(element.GetAttribute("rname"));
                }

                _MetricList.Add("Economic Metric");
                //get metric name by using XPath to obtain from the labels element of the XML file
                XmlNode inflation = xmlFile.SelectSingleNode("//labels/inflation");
                if (inflation != null)
                {
                    XmlAttributeCollection attrs = inflation.Attributes;
                    XmlNode CPINode = attrs.GetNamedItem("consumer_prices_percent");
                    XmlNode GDPNode = attrs.GetNamedItem("gdp_deflator_percent");
                    _MetricList.Add(CPINode.InnerText);
                    _MetricList.Add(GDPNode.InnerText);
                }
                else Console.WriteLine("No Node obtained!!");
                //get metric name from //labels/interest
                XmlNode interest = xmlFile.SelectSingleNode("//labels/interest");
                if (interest != null)
                {
                    XmlAttributeCollection attrs = interest.Attributes;
                    XmlNode real = attrs.GetNamedItem("real");
                    XmlNode lending = attrs.GetNamedItem("lending");
                    XmlNode deposit = attrs.GetNamedItem("deposit");
                    _MetricList.Add(real.InnerText);
                    _MetricList.Add(lending.InnerText);
                    _MetricList.Add(deposit.InnerText);
                }
                else Console.WriteLine("No Node obtained!!");
                //get metric name from //labels/unemployment
                XmlNode unemployment = xmlFile.SelectSingleNode("//labels/unemployment");
                if (unemployment != null)
                {
                    XmlAttributeCollection attrs = unemployment.Attributes;
                    XmlNode national = attrs.GetNamedItem("national_estimate");
                    XmlNode modeled = attrs.GetNamedItem("modeled_ILO_estimate");
                    _MetricList.Add(national.InnerText);
                    _MetricList.Add(modeled.InnerText);
                }
                else Console.WriteLine("No Node obtained!!");

                //create XPathNavigator object for performing XPath queries
                XPathNavigator? nav = xmlFile.CreateNavigator();

                //print out the first title
                Console.WriteLine("World Economic Data");
                Console.WriteLine("================\n");

                //start the main menu selection
                char mainMenuSelection = ' ';
                while (mainMenuSelection != 'X')
                {
                    mainMenuSelection = char.ToUpper(PrintMainMenu(yearBegin, yearEnd));
                    switch (mainMenuSelection)
                    {
                        case 'Y':
                            Console.Write("\nStarting year (1970 to 2021): ");
                            while (!int.TryParse(Console.ReadLine(), out yearBegin) || yearBegin <= 1970 || yearBegin > 2021)
                                Console.Write("Please enter year in range 1970 to 2021: ");
                            Console.WriteLine();
                            yearEnd = GetYearRange(yearBegin);
                            break;

                        case 'R':
                            PrintRegionSummary(nav);
                            PrintEconomicInfo(xmlFile, yearBegin, yearEnd);
                            break;

                        case 'M':
                            PrintMetric(xmlFile, yearBegin, yearEnd);
                            break;
                        default:
                            break;
                    }//end switch
                }//end while
                Console.WriteLine("\nAll done!!!");
            }
            catch (XmlException err)
            {
                Console.WriteLine($"\nXML ERROR: {err.Message}");
            }
            catch (XPathException err)
            {
                Console.WriteLine($"\nXPATH ERROR: {err.Message}");
            }
            catch (Exception err)
            {
                Console.WriteLine($"\nERROR: {err.Message}");
            }
        }//end main

        //print out the menu of the program on the screen and prompt the user to input 
        //one option and return the choice to the program
        static char PrintMainMenu(int yearBegin, int yearEnd)
        {
            Console.WriteLine($"'Y' to adjust the range of years (currently {yearBegin}  to {yearEnd })");
            Console.WriteLine("'R' to print a regional summary");
            Console.WriteLine("'M' to print a specific metric for all regions");
            Console.WriteLine("'X' to exit the program");
            //get user input
            Console.Write("Your Selection: ");
            char menuSelection = char.ToUpper(Console.ReadKey().KeyChar);
            Console.WriteLine("\n");
            return menuSelection;
        }//end PrintMainMenu


        //funciton to get the end year and validate the range of year
        //based on user's yearBegin input
        static int GetYearRange(int yearBegin)
        {
            Console.Write("Ending year (1970 to 2021): ");
            int yearEnd = 0;
            while (!int.TryParse(Console.ReadLine(), out yearEnd) || yearEnd > (yearBegin + 4) || yearEnd < yearBegin)
            {
                Console.WriteLine(yearEnd);
                Console.WriteLine("ERROR: Ending year must be an integer between " + yearBegin + " and " + (yearBegin + 4));
                Console.Write("Ending year (1970 to 2021): ");
            }
            Console.WriteLine("\n");
            return yearEnd;
        }// end GetYearRange

        //print to the screen with the full list of available regions
        //by using XPathNavigator then use Iterator to get trhough all
        //values
        static void PrintRegionSummary(XPathNavigator nav)
        {
            Console.WriteLine("\nSelect a region by number as shown below...");
            int index = 1;
            string queryText = "//region/@rname";
            //create XPathNodeIterator
            XPathNodeIterator nodeIt = nav.Select(queryText);

            if (nodeIt.Count > 0)
            {
                while (nodeIt.MoveNext())
                {
                    if (nodeIt.Current is not null)
                        Console.WriteLine($"\t{index,3}. {nodeIt.Current.Value}");
                    else Console.Write("\t -");
                    index++;
                }
            }
            else Console.Write("\tNo value");

        }//end PrintRegionSummary

        //get the user input for the wanted country
        //then pass the input value to a helper function to use XPathMavigator to obtain 
        //require information and format the output value and screen
        static void PrintEconomicInfo(XmlDocument xmlFile, int yearBegin,int yearEnd)
        {
            //get region number
            Console.Write("Enter a region #: ");
            //number is read in string, convert to int
            string? getRegion;
            bool valid;
            int index;
            int temp = yearBegin;
            //get and validate the choice of country of user
            do
            {
                getRegion = Console.ReadLine();
                index = 0;
                valid = Int32.TryParse(getRegion, out index) || index <= _regions.Count && index > 0;
                if (!valid)
                    Console.WriteLine($"ERROR: Input must be an integer between 1 and {_regions.Count}.\n");
            } while (!valid);
            Console.WriteLine("\n");

            //get region name and print out with the use of StringBuilder 
            string title = $"Economic Information for {_regions.ElementAt(index - 1)}";
            int titleLength = title.Length;
            Console.WriteLine(title);
            StringBuilder sb = new StringBuilder();
            sb.Append('-', titleLength);
            Console.WriteLine(sb.ToString());

            //write out the first header with years
            Console.Write("{0,30}", _MetricList.ElementAt(0));
            for (int i = yearBegin; i <= yearEnd; i++)
            {
                Console.Write("{0,8}", i);
            }
            Console.WriteLine("\n");

            //create XPathNavigator
            XPathNavigator nav = xmlFile.CreateNavigator();

            //get the information for each requirements
            Console.Write("{0,30}", _MetricList.ElementAt(1));
            PrintValueForMetric(nav, index, yearBegin, yearEnd, "inflation", "consumer_prices_percent");

            Console.Write("{0,30}", _MetricList.ElementAt(2));
            PrintValueForMetric(nav, index, yearBegin, yearEnd, "inflation", "gdp_deflator_percent");

            Console.Write("{0,30}", _MetricList.ElementAt(3));
            PrintValueForMetric(nav, index, yearBegin, yearEnd, "interest_rates", "real");

            Console.Write("{0,30}", _MetricList.ElementAt(4));
            PrintValueForMetric(nav, index, yearBegin, yearEnd, "interest_rates", "deposit");

            Console.Write("{0,30}", _MetricList.ElementAt(5));
            PrintValueForMetric(nav, index, yearBegin, yearEnd, "interest_rates", "consumer_prices_percent");

            Console.Write("{0,30}", _MetricList.ElementAt(6));
            PrintValueForMetric(nav, index, yearBegin, yearEnd, "unemployment_rates", "national_estimate");

            Console.Write("{0,30}", _MetricList.ElementAt(7));
            PrintValueForMetric(nav, index, yearBegin, yearEnd, "unemployment_rates", "modeled_ILO_estimate");

            Console.WriteLine();

        }

        /*
           *      - Build an XPath expression to select the country's metric info
           *      - Run this XPath expression using an XPathNavigator object and report the results
        */
        static void PrintValueForMetric(XPathNavigator nav,int index, int yearBegin, int yearEnd, string dis1, string dis2)
        {
            for (int year = yearBegin; year <= yearEnd; year++)
            {
                string queryText = String.Format($"//region[{index}]/year[@yid={year} ]/{dis1}/@{dis2}");
                //create a navigator
                XPathNodeIterator nodeIt = nav.Select(queryText);
                if (nodeIt.Count > 0)
                {
                    while (nodeIt.MoveNext())
                    {
                        if (nodeIt.Current is not null)
                            Console.Write($"\t{nodeIt.Current.Value,6}");
                    }
                }
                else
                {
                        Console.Write($"\t {'-',5}");
                }
            }
            Console.WriteLine();
        }

        //opition M: print a specific metric for all regions
        //get the user's input for the metric
        //then based on the input, select the appropriate description then pass 
        //to another helper method to build a query to obtain the data from XML file
        //and format the output then print on the screen
        static void PrintMetric(XmlDocument xmlFile,int yearBegin, int yearEnd)
        {
            Console.WriteLine("Select a metric by number as shown below...");
            for (int i = 1; i < _MetricList.Count; i++)
            {
                Console.Write(" ");
                Console.Write(i);
                Console.Write(". ");
                Console.WriteLine(_MetricList.ElementAt(i));
            }
            //get metric number
            int metricNumber = 0;
            Console.Write("\nEnter a metric #: ");
            while (!int.TryParse(Console.ReadLine(), out metricNumber) && metricNumber <= 0 || metricNumber > 7)
                Console.Write("Please enter one input from 1 to 7!!! ");
            Console.WriteLine("\n");

            string dis1 = " ";            //description 1 for the metric 
            string dis2 = " ";            //description 2 for the metric

            //based on the input, choose right description to build the correct query
            switch (metricNumber)
            {
                case 1:
                    dis1 = "inflation";
                    dis2 = "consumer_prices_percent";
                    break;
                case 2:
                    dis1 = "inflation";
                    dis2 = "gdp_deflator_percent";
                    break;
                case 3:
                    dis1 = "interest_rates";
                    dis2 = "real";
                    break;
                case 4:
                    dis1 = "interest_rates";
                    dis2 = "lending";
                    break;
                case 5:
                    dis1 = "interest_rates";
                    dis2 = "deposit";
                    break;
                case 6:
                    dis1 = "unemployment_rates";
                    dis2 = "national_estimate";
                    break;
                case 7:
                    dis1 = "unemployment_rates";
                    dis2 = "modeled_ILO_estimate";
                    break;
            }
            XPathNavigator nav = xmlFile.CreateNavigator();

            //print out title of the metric summary using StringBuilder
            string title = $"{_MetricList.ElementAt(metricNumber)} By Region";
            int titleLength = title.Length;
            Console.WriteLine(title);
            StringBuilder sb = new StringBuilder();
            sb.Append('-', titleLength);
            Console.WriteLine(sb.ToString());

            PrintSummanyForMetric(nav, _regions.Count(), yearBegin, yearEnd, dis1, dis2);

        }

        //get a summary of each metric by using XPathNavigator
        //and format the output 
        static void PrintSummanyForMetric(XPathNavigator nav, int size, int yearBegin, int yearEnd, string dis1, string dis2)
        {

            Console.Write("{0,55}", "Region");
            for (int i = yearBegin; i <= yearEnd; i++)
            {
                Console.Write("{0,8}", i);
            }
            Console.WriteLine("\n");
            for (int index = 1; index < size; index++)
            {
                Console.Write("{0,55}", _regions.ElementAt(index-1));
                for (int year = yearBegin; year <= yearEnd; year++)
                {
                    string queryText = String.Format($"//region[{index}]/year[@yid={year} ]/{dis1}/@{dis2}");
                    //create a navigator
                    XPathNodeIterator nodeIt = nav.Select(queryText);
                    if (nodeIt.Count > 0)
                    {
                        while (nodeIt.MoveNext())
                        {
                            if (nodeIt.Current is not null)
                                Console.Write($"\t{nodeIt.Current.Value,6}");
                        }
                    }
                    else
                    {
                        Console.Write($"\t {'-',5}");
                    }
                }
                Console.WriteLine();
            }
        }
    }
}