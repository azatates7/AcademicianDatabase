using HtmlAgilityPack;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;

namespace DataBaseProject
{
    public class Program
    {
        public static string cvfile = @"cv_list.txt";
        public static void Main(string[] args)
        {
            FileExistChecker(cvfile);
            GetAllCvLines(); // fill cv_list.txt

            string tempfile = @"temp.txt";
            FileExistChecker(tempfile);
            int i = 1
            String mainxpath = @"/html/body/div[2]";
            string lessonxpath = @"/html/body/div[2]/div[2]/div[2]";
            String line = "";

            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(cvfile);
                while ((line = file.ReadLine()) != null)
                {
                    String tempfile2 = line.Split('.')[0] + ".txt";
                    FileExistChecker(tempfile2);
                    GetMainInfos(@"http://" + line, mainxpath, tempfile);
                    GetLessonFiles(@"http://" + line, lessonxpath, tempfile2, i); i++;
                }
                file.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Detected : " + ex.Message);
            }
            finally
            {
                //File.Delete(cvfile); // C:\Users\${username}\source\repos\Project\GetCvInfo\bin\Debug
                Console.ReadKey();
            }
        }

        private static void FileExistChecker(string filename)
        {
            if (!File.Exists(filename))
            {
                FileInfo f = new FileInfo(filename);
            }
            else
            {
                File.Delete(filename);
                FileInfo f = new FileInfo(filename);
            }
            
        }

        private static void GetAllCvLines(string filename = "temp_file.txt")
        {
            FileExistChecker(filename);

            CvLinesAdder(@"http://cmf.nku.edu.tr/PersonelListesi/0/s/9790/801", @"/html/body/div[3]/div/div[2]/div[1]/div", filename);
            CvLinesAdder(@"http://gsf.nku.edu.tr/personel_/0/s/563/13393", @"/html/body/div[3]/div/div[2]/div[1]/div", filename);
            CvLinesAdder(@"http://dis.nku.edu.tr/Personel/0/s/11682/16256", @"/html/body/div[3]/div/div[2]/div[1]/div", filename);
            CvLinesAdder(@"http://hukuk.nku.edu.tr/Personel/0/s/9799/13736", @"/html/body/div[3]/div/div[2]/div[2]/div", filename);
            CvLinesAdder(@"http://iibf-ik.web.nku.edu.tr/AkademikKadro/0/s/3313/3369", @"/html/body/div[3]/div/div[2]/div", filename);
            CvLinesAdder(@"http://ilahiyat.nku.edu.tr/PersonelListesi/0/s/9804/8207", @"/html/body/div[3]/div/div[2]/div[2]/div", filename);
            CvLinesAdder(@"http://tip.nku.edu.tr/AkademikPersonel/0/s/455/22316", @"/html/body/div[3]/div/div[2]/div", filename);
            CvLinesAdder(@"http://veteriner.nku.edu.tr/akademikpersonel/0/s/7164/9090", @"/html/body/div[3]/div/div[2]/div[2]", filename);
            CvLinesAdder(@"http://ziraat.nku.edu.tr/PersonelListesi/0/s/9860/684", @"/html/body/div[3]/div/div[2]/div[1]/div", filename);
            CvLinesAdder(@"http://fened.nku.edu.tr/AkademikPersonel/0/s/329/19683", @"/html/body/div[3]/div/div[2]/div", filename);
        }

        private static void CvLinesAdder(string url, string xpath, string filename)
        {
            FileExistChecker(filename);
            HtmlWeb web = new HtmlWeb
            {
                OverrideEncoding = Encoding.UTF8
            };
            HtmlDocument doc = web.Load(url);
            List<HtmlNode> HeaderNames = doc.DocumentNode.SelectNodes(xpath).ToList();

            foreach (HtmlNode item in HeaderNames)
            {
                using (StreamWriter sw = File.AppendText(filename))
                {
                    sw.WriteLine(item.InnerText.Replace("&nbsp;", ""));
                }
            }

            Regex regex = new Regex("([a-z]+)@([a-z]+)*");
            using (StreamReader reader = new StreamReader(filename))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = regex.Match(line);
                    if (match.Success & line != "")
                    {
                        using (StreamWriter sw = File.AppendText(cvfile))
                        {
                            string[] parts = line.Trim().Split('@');
                            string a = parts[0];
                            string b = parts[1];
                            sw.WriteLine(a + b.Replace(b, ".cv.nku.edu.tr"));
                        }
                    }
                }
            }
            File.Delete(filename);
        }

        private static void GetMainInfos(string url, string xpath, string filename)
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(new List<string>() { "no-sandbox", "headless" });
            ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
            ChromeDriver driver = new ChromeDriver(chromeDriverService, chromeOptions);
            driver.Navigate().GoToUrl(url);

            ReadOnlyCollection<IWebElement> element = driver.FindElements(By.XPath(xpath));
            foreach (IWebElement item in element)
            {
                using (StreamWriter sw = File.AppendText(filename))
                {
                    sw.WriteLine(item.Text);
                }
            }
            driver.Quit();
            string title = "", name = "", phone = "", mail = url.Replace("http://", "").Split('.')[0] + "@nku.edu.tr", webaddress = url.Trim(), faculty = "", branch = "", main_branch = "", info_last_update_date = "N/A";
            int lineCount = 0;
            string line = string.Empty;
            using (StreamReader readerlines = File.OpenText(filename))
            {
                while ((line = readerlines.ReadLine()) != null)
                {
                    if (!line.Equals(string.Empty))
                    {
                        lineCount++;
                    }
                }
            }

            string[] lines = File.ReadAllLines(filename);
            name = lines[0].Replace(" NKUAVES", "").Trim();
            if (name != "Kalite") // for catch invalid pages
            {
                if (name.StartsWith("Araş. Gör. "))
                {
                    title = "Araş. Gör. ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("Arş. Gör. Dr. "))
                {
                    title = "Arş. Gör. Dr. ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("Arş. Gör. "))
                {
                    title = "Arş. Gör. ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("Doç. Dr. "))
                {
                    title = "Doç. Dr. ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("Doç. "))
                {
                    title = "Doç. ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("Dr. Öğr. Üyesi "))
                {
                    title = "Dr. Öğr. Üyesi ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("Dr. "))
                {
                    title = "Dr. ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("MİMAR "))
                {
                    title = "MİMAR ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("Öğr. Gör. Dr. "))
                {
                    title = "Öğr. Gör. Dr. ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("Öğr. Gör. "))
                {
                    title = "Öğr. Gör. ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("Öğretmen "))
                {
                    title = "Öğretmen ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("Prof. Dr. "))
                {
                    title = "Prof. Dr. ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("Prof. "))
                {
                    title = "Prof. ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else if (name.StartsWith("Serbest "))
                {
                    title = "Serbest ";
                    name = name.Replace(title, "|").Split('|')[1];
                }
                else
                {
                    title = "Undefined";
                }
                if (lineCount > 20) // for check full columns
                {
                    if (lines[15] == "")
                    {
                        phone = lines[20].Trim().Replace("(", "").Replace(")", "").Replace(" ", "").Split(':')[1];
                        faculty = lines[24].Split(':')[1].Trim();
                        branch = lines[25].Trim().Split(':')[1].Trim();
                        main_branch = lines[26].Trim().Split(':')[1].Trim();
                    }
                    else
                    {
                        phone = lines[21].Trim().Replace("(", "").Replace(")", "").Replace(" ", "").Split(':')[1];
                        faculty = lines[25].Split(':')[1].Trim();
                        branch = lines[26].Trim().Split(':')[1].Trim();
                        main_branch = lines[27].Trim().Split(':')[1].Trim();
                        info_last_update_date = lines[15].Trim().Replace(" ", "").Split(':')[1];
                    }

                }
                else if (lineCount > 16 && lineCount < 20) // for check info_last_update_date valid
                {
                    info_last_update_date = lines[15].Trim().Replace(" ", "").Split(':')[1];
                }
                else // for check null
                {

                }
                AddDatabase(title, name, phone, mail, webaddress, faculty, branch, main_branch, info_last_update_date);
            }
            else
            {
                AddDatabase("Invalid", "Page", phone, mail, webaddress, faculty, branch, main_branch, info_last_update_date);
            }
            File.Delete(filename);
            driver.Quit();
        } 

        private static void AddDatabase(string title, string name, string phone, string mail, string webaddress, string faculty, string branch, string main_branch, string info_last_update_date)
        {
            String connectionstring = @"Server=AA7\SQLEXPRESS; Database=ProjectDB; Trusted_Connection=True;";
            string InsertQuery = @"INSERT INTO dbo.academicians(title, name, phone, mail, webaddress, faculty, branch, main_branch, info_last_update_date) VALUES(@title, @name, @phone, @mail, @webaddress, @faculty, @branch, @main_branch, @info_last_update_date)";
            try
            {

                using (SqlConnection connection = new SqlConnection(connectionstring))
                {
                    using (SqlCommand command = new SqlCommand(InsertQuery, connection))
                    {
                        command.Parameters.Add("@title", SqlDbType.NChar).Value = title;
                        command.Parameters.Add("@name", SqlDbType.NChar).Value = name;
                        command.Parameters.Add("@phone", SqlDbType.NChar).Value = phone;
                        command.Parameters.Add("@mail", SqlDbType.NChar).Value = mail;
                        command.Parameters.Add("@webaddress", SqlDbType.NChar).Value = webaddress;
                        command.Parameters.Add("@faculty", SqlDbType.NChar).Value = faculty;
                        command.Parameters.Add("@branch", SqlDbType.NChar).Value = branch;
                        command.Parameters.Add("@main_branch", SqlDbType.NChar).Value = main_branch;
                        command.Parameters.Add("@info_last_update_date", SqlDbType.NChar).Value = info_last_update_date;

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eror Detected : " + ex.Message);
            }
        }

        private static void GetLessonFiles(string url, string xpath, string filename, int i)
        {
            FileExistChecker(filename);

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(new List<string>() { "no-sandbox", "headless" });
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            ChromeDriver driver = new ChromeDriver(chromeDriverService, chromeOptions);
            driver.Navigate().GoToUrl(url);
            using (StreamWriter sw = File.AppendText(filename))
            {
                sw.WriteLine("empty line for create file");
            }
            try
            {
                var html = @"/html/body/div[2]/div[2]/div[1]/ul/li[6]/a";
                string outherhtml = driver.FindElement(By.XPath(html)).GetAttribute("outerHTML");
                driver.FindElement(By.LinkText("Verdiği Dersler")).Click();
                string funcname = outherhtml.Substring(32, 40).Replace("#dersler\" onclick=\"", "").Split(';')[0].ToString();
                Boolean result = funcname.Any(char.IsDigit); // for check function return null 
                if (result)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript(funcname);
                    ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(i + ".png"); // for trigger function
                    ReadOnlyCollection<IWebElement> element = driver.FindElements(By.XPath(xpath));
                    foreach (var item in element)
                    {
                        using (StreamWriter sw = File.AppendText(filename))
                        {
                            sw.WriteLine(item.Text);
                        }
                    }
                    File.Delete(i + ".png");
                    FileAnalyzer(filename, i);
                }
                else
                {
                    AddDatabase2(i, "", "", "", "");
                }
            }
            catch (Exception)
            {
                AddDatabase2(i, "", "", "", "");
            }
            finally
            {
                driver.Quit();
                File.Delete(filename);
            }
        }

        private static void FileAnalyzer(string filename, int i)
        {
            string lessonname = "", lessontype = "", lessonterm = "", lessonhour = "";
            var lines = File.ReadAllLines(filename);
            int l1 = 0, l2 = 0, l3 = 0, l4 = 0;
            int lineslength = lines.Length;
            for (var k = 0; k < lineslength; k += 1)
            {
                var liness = lines[k].Trim();
                if (liness == "Doktora")
                {
                    l1 = k;
                }
                else if (liness == "Yüksek Lisans")
                {
                    l2 = k;
                }
                else if (liness == "Lisans")
                {
                    l3 = k;
                }
                else if (liness == "Ön Lisans")
                {
                    l4 = k;
                }
                else
                {

                }
            }
            int plus = l1 + l2 + l3 + l4;
            if (plus == 0)
            {
                AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
            }
            if ((l1 > 0) & (l2 > 0) & (l3 > 0) & (l4 > 0))
            {
                for (int lessonline = l1 + 2; lessonline < l2; lessonline++)
                {
                    lessontype = "Doktora";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
                for (int lessonline = l2 + 2; lessonline < l3; lessonline++)
                {
                    lessontype = "Yüksek Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
                for (int lessonline = l3 + 2; lessonline < l4; lessonline++)
                {
                    lessontype = "Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
                for (int lessonline = l4 + 2; lessonline < lineslength; lessonline++)
                {
                    lessontype = "Ön Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
            }
            else if ((l1 > 0) & (l2 > 0) & (l3 > 0) & (l4 < 1))
            {
                for (int lessonline = l1 + 2; lessonline < l2; lessonline++)
                {
                    lessontype = "Doktora";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
                for (int lessonline = l2 + 2; lessonline < l3; lessonline++)
                {
                    lessontype = "Yüksek Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
                for (int lessonline = l3 + 2; lessonline < lineslength; lessonline++)
                {
                    lessontype = "Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
            }
            else if ((l1 > 0) & (l2 > 0) & (l3 < 1) & (l4 < 1))
            {
                for (int lessonline = l1 + 2; lessonline < l2; lessonline++)
                {
                    lessontype = "Doktora";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
                for (int lessonline = l2 + 2; lessonline < lineslength; lessonline++)
                {
                    lessontype = "Yüksek Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
            }
            else if ((l1 > 0) & (l2 < 1) & (l3 < 1) & (l4 < 1))
            {
                for (int lessonline = l1 + 2; lessonline < lineslength; lessonline++)
                {
                    lessontype = "Doktora";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
            }
            else if ((l1 < 1) & (l2 > 0) & (l3 > 0) & (l4 > 0))
            {
                for (int lessonline = l2 + 2; lessonline < l3; lessonline++)
                {
                    lessontype = "Yüksek Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
                for (int lessonline = l3 + 2; lessonline < l4; lessonline++)
                {
                    lessontype = "Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
                for (int lessonline = l4 + 2; lessonline < lineslength; lessonline++)
                {
                    lessontype = "Ön Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
            }
            else if ((l1 < 1) & (l2 > 0) & (l3 > 0) & (l4 < 1))
            {
                for (int lessonline = l2 + 2; lessonline < l3; lessonline++)
                {
                    lessontype = "Yüksek Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
                for (int lessonline = l3 + 2; lessonline < lineslength; lessonline++)
                {
                    lessontype = "Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
            }
            else if ((l1 < 1) & (l2 > 0) & (l3 < 1) & (l4 < 1))
            {
                for (int lessonline = l2 + 2; lessonline < lineslength; lessonline++)
                {
                    lessontype = "Yüksek Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
            }
            else if ((l1 < 1) & (l2 < 1) & (l3 > 0) & (l4 > 0))
            {
                for (int lessonline = l3 + 2; lessonline < l4; lessonline++)
                {
                    lessontype = "Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
                for (int lessonline = l4 + 2; lessonline < lineslength; lessonline++)
                {
                    lessontype = "Ön Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
            }
            else if ((l1 < 1) & (l2 < 1) & (l3 > 0) & (l4 < 1))
            {
                for (int lessonline = l3 + 2; lessonline < lineslength; lessonline++)
                {
                    lessontype = "Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
            }
            else if ((l1 < 1) & (l2 < 1) & (l3 < 1) & (l4 > 0))
            {
                for (int lessonline = l4 + 2; lessonline < lineslength; lessonline++)
                {
                    lessontype = "Ön Lisans";
                    lessonterm = lines[lessonline].Split(' ')[0];
                    lessonhour = lines[lessonline].Substring(lines[lessonline].Length - 1);
                    lessonname = lines[lessonline].Replace(lessonterm, "").Replace(lessonhour, "").Trim();
                    AddDatabase2(i, lessonname, lessontype, lessonterm, lessonhour);
                }
            }
        }

        private static void AddDatabase2(int i, string lessonname, string lessontype, string lessonterm, string lessonhour)
        {
            String connectionstring = @"Server=AA7\SQLEXPRESS; Database=ProjectDB; Trusted_Connection=True;";
            string InsertQuery = @"INSERT INTO dbo.lessons(academicianid, lessonname, lessontype, lessonterm, lessonhour) VALUES(@academicianid, @lessonname, @lessontype, @lessonterm, @lessonhour)";

            using (SqlConnection connection = new SqlConnection(connectionstring))
            {
                using (SqlCommand command = new SqlCommand(InsertQuery, connection))
                {
                    command.Parameters.Add("@academicianid", SqlDbType.Int).Value = i;
                    command.Parameters.Add("@lessonname", SqlDbType.NChar).Value = lessonname;
                    command.Parameters.Add("@lessontype", SqlDbType.NChar).Value = lessontype;
                    command.Parameters.Add("@lessonterm", SqlDbType.NChar).Value = lessonterm;
                    command.Parameters.Add("@lessonhour", SqlDbType.NChar).Value = lessonhour;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
    }
}