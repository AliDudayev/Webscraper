using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text.RegularExpressions;

namespace Selenium
{

    class Scraper
    {
        // Variabelen voor ChromeDriver.
        private IWebDriver driver;

        // String variabelen voor XPath.
        private string xpath = "";

        // Hetgeen dat gebruikers intypen om op te zoeken wordt in searchword gezet.
        private string searchWord;

        // Deze lijst is waar de response aan wordt toegevoegd en wordt doorgegeven aan de logger.
        private List<string> text;

        // De eerste keer heeft YouTube een pop-up en de tweede keer niet. Deze variabel dient daarvoor.
        private bool youtubePopup = true;

        // Dit is de constructor die wordt aangeroepen wanneer een object van de Scraper-klasse wordt gemaakt.
        public Scraper()
        {
            // Maak een nieuw Options-object aan, zodat we opties aan de ChromeDriver kunnen doorgeven.
            ChromeOptions options = new ChromeOptions();

            // Op mijn manga-site krijg ik veel waarschuwingen omdat de site problemen heeft met certificaten.
            // Deze waarschuwingen worden afgedrukt op mijn console, en dat wil ik vermijden, vandaar dat ik de foutmeldingen negeer.
            // https://stackoverflow.com/questions/41566892/chromeoptions-ignore-certificate-errors-does-not-get-rid-of-err-cert-authori
            options.AddArgument("--ignore-certificate-errors");
            // Open een ChromeDriver met de opgegeven opties.
            driver = new ChromeDriver(options);
        }

        // Klasse die wordt aangeroepen wanneer de gebruiker optie 1 kiest (om YouTube te scrapen).
        public void ScrapeYoutube()
        {
            // Gewoon afdrukken.
            Console.Write("wait...");
            // Open de URL van YouTube, ga dus naar YouTube.
            driver.Url = "https://www.youtube.com/";

            // Dit is voor de pop-up van YouTube. Eerst wordt de XPath in de variabelen gezet. De eerste keer dat je YouTube scrape't, is youtubePopup true,
            // waardoor je in de 'if' terechtkomt en op de pop-up drukt, en de variabelen op false zet, zodat je niet meer in deze 'if' terechtkomt.
            xpath = "/html/body/ytd-app/ytd-consent-bump-v2-lightbox/tp-yt-paper-dialog/div[4]/div[2]/div[6]/div[1]/ytd-button-renderer[1]/yt-button-shape/button/yt-touch-feedback-shape/div/div[2]";
            if (youtubePopup)
            {
                Click();
                youtubePopup = false;
            }

            // Met dit stuk zoek ik een video op basis van de input van de gebruiker.
            // Ik wil "wait..." vervangen door "search:", dus ik verwijder die regel en print "search:".
            // https://stackoverflow.com/questions/8946808/can-console-clear-be-used-to-only-clear-a-line-instead-of-whole-console
            Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
            Console.Write("Search: ");

            // Deze methode laat gebruikers hun zoekopdracht invoeren, en deze zoekopdracht wordt opgeslagen in searchWord.
            SetSearchWord();

            // Zoek de zoekbalk. Ik zet de XPath in de variabel, en die variabel wordt gebruikt in de search() methode.
            xpath = "/html/body/ytd-app/div[1]/div/ytd-masthead/div[4]/div[2]/ytd-searchbox/form/div[1]/div[1]/input";
            // De search is een methode die de XPath zoekt in een while-loop. Als het de XPath niet vindt, wacht het 1 seconde en
            // probeert het opnieuw tot het lukt.
            Search();


            // Als je op 'nieuwste' en 'video' filtert, wordt deze string aan de link toegevoegd. Dus als ik deze string handmatig toevoeg,
            // wordt de filter automatisch toegepast.
            driver.Url = driver.Url + "&sp=CAISAhAB";

            // 'text' is de lijst die ik doorgeef aan de logger en waar de resultaten in staan.
            // Ik maak de lijst gewoon opnieuw aan omdat "text.Clear()" mij problemen gaf.
            text = new List<string>();

            text.Add("Title");
            text.Add("Channel");
            text.Add("Views");
            text.Add("Link");

            // Ik voeg ook een string 'NextLine' toe. Dit is zodat mijn loggers weten wanneer de headers en elementen per video/job/manga eindigen.
            // Zo weet het dat het bijvoorbeeld een lege lijn moet zetten tijdens het printen naar de console. Ik heb dit zo gemaakt zodat mijn loggers
            // dynamisch zijn en niet hardgecodeerd. Als ik meer elementen wil tonen, zoals of een YouTuber geverifieerd is of wanneer de video is geüpload,
            // dan hoef ik alleen de headers ervoor toe te voegen en niets te veranderen aan mijn code. Het werkt nog steeds perfect.
            text.Add("NextLine");

            // Deze for-loop zorgt ervoor dat ik alleen de eerste 5 video's pak.
            for (int video = 1; video < 6; video++)
            {
                // Dit is de XPath. Ik heb gezien dat ytd-video-renderer[" + video + "] verandert bij elke video, dus ik kan dit gebruiken om door
                // de video's te gaan.
                xpath = "/html/body/ytd-app/div[1]/ytd-page-manager/ytd-search/div[1]/ytd-two-column-search-results-renderer/div/ytd-section-list-renderer/div[2]/ytd-item-section-renderer/div[3]/ytd-video-renderer[" + video + "]/div[1]";

                // De 'if'-voorwaarde controleert of elk element aanwezig is met een try-catch. Als ik een foutmelding krijg van de try-catch, return ik false,
                // waardoor ik niet in de 'if' terechtkom. Als het element bestaat, return ik true en kan ik in de 'if' doorgaan.
                // Deze 'if'-voorwaarde is er voor het geval een zoekopdracht minder dan 5 video's oplevert.
                // Als dit niet zou bestaan, zou ik een foutmelding krijgen als er minder dan 5 video's zijn, omdat de video en zijn elementen dan niet bestaan.
                if (IsElementPresent())
                {
                    // Hier zet ik de elementen in variabelen door de XPath plus een extra XPath te gebruiken die naar het element verwijst.
                    var title = driver.FindElement(By.XPath(xpath + "/div/div[1]/div/h3/a"));
                    var channel = driver.FindElement(By.XPath(xpath + "/div/div[2]/ytd-channel-name/div/div/yt-formatted-string/a"));
                    var views = driver.FindElement(By.XPath(xpath + "/div/div[1]/ytd-video-meta-block/div[1]/div[2]/span[1]"));
                    // De link voor de video bevindt zich in een attribuut op dezelfde locatie als het title, dus ik kan eenvoudig
                    // het attribuut verkrijgen met 'GetAttribute'.
                    // https://stackoverflow.com/questions/15388057/extract-link-from-xpath-using-selenium-webdriver-and-python
                    var link = title.GetAttribute("href");

                    // Ik voeg de strings van de variabelen toe aan de 'text'-lijst.
                    text.Add(title.Text);
                    text.Add(channel.Text);
                    text.Add(views.Text);
                    text.Add(link);
                    // "NextLine" zodat het systeem weet dat het volgende element in de lijst bij een andere video hoort of gewoon het einde van de lijst is.
                    text.Add("NextLine");
                }
            }

            // Schrijf de 'text'-lijst weg aan de hand van de keuze van de gebruiker door de 'Write'-methode van de logger op te roepen.
            // Hier krijgt de gebruiker de keuze van wat hij wilt doen met de resultaten.
            Logger.Write(text);
        }

        public void ScrapeJobs()
        {
            // Gewoon afdrukken.
            Console.Write("wait...");
            // Open de URL van ICTJob, ga dus naar ICTJob.
            driver.Url = "https://www.ictjob.be/en/";

            // Met dit stuk zoek ik jobs op basis van de input van de gebruiker.
            // Ik wil "wait..." vervangen door "search:", dus ik verwijder die regel en print "search:".
            // https://stackoverflow.com/questions/8946808/can-console-clear-be-used-to-only-clear-a-line-instead-of-whole-console
            Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
            Console.Write("Search: ");

            // Deze methode laat gebruikers hun zoekopdracht invoeren, en deze zoekopdracht wordt opgeslagen in searchWord.
            SetSearchWord();

            // Zoek de zoekbalk. Ik zet de XPath in de variabel, en die variabel wordt gebruikt in de search() methode.
            xpath = "/html/body/section/div[1]/div/div[2]/div[1]/div/section/form/div/div/div[3]/div[2]/div[1]/div[1]/div/div[1]/label/input";
            // De search is een methode die de XPath zoekt in een while-loop.
            // Als het de XPath niet vindt, wacht het 1 seconde en probeert het opnieuw tot het lukt.
            Search();

            // Dit is de XPath van de datumknop. Ik zet het in een variabel om er meteen naar te kunnen zoeken.
            xpath = "/html/body/section/div[1]/div/div[2]/div/div/form/div[2]/div/div/div[2]/section/div/div[1]/div[2]/div/div[2]/span[2]/a";
            // Ik gebruik deze methode om op de datumknop te klikken.
            // Hierin zit een lus zodat het systeem telkens een seconde wacht en opnieuw zoekt als het de knop niet vindt.
            // Ik geef true mee omdat ik ook op het scherm moet scrollen, anders vindt het de knop niet.
            Click(true);

            // 'text' is de lijst die ik doorgeef aan de logger en waar de resultaten in staan.
            // Ik maak de lijst gewoon opnieuw aan omdat "text.Clear()" mij problemen gaf.
            text = new List<string>();

            text.Add("Titel");
            text.Add("Bedrijf");
            text.Add("Locatie");
            text.Add("Keywords");
            text.Add("Link");
            // Ik voeg ook een string genaamd 'NextLine' toe. Dit is zodat mijn loggers weten wanneer de headers en elementen per job eindigen.
            // Zo weten ze dat ze bijvoorbeeld een lege regel moeten invoegen tijdens het printen naar de console.
            // Ik heb dit zo gemaakt zodat mijn loggers dynamisch zijn en niet hardcoded.
            // Als ik meer elementen wil tonen, zoals wanneer de job op de website is geplaatst, dan hoef ik alleen de headers ervoor toe te voegen
            // en niets te veranderen aan mijn code. Het werkt nog steeds perfect.
            text.Add("NextLine");

            // Ik laat mijn programma 15 seconden wachten op deze website omdat de website erg traag is.
            // De extra 15 seconden zorgen ervoor dat alles zeker is ingeladen en gevonden kan worden.
            Thread.Sleep(15000);

            // Deze for-loop zorgt ervoor dat ik alleen de eerste 5 jobs pak.
            // Je merkt waarschijnlijk op dat dit 6 elementen zijn, dus normaal gezien zou het 6 jobs moeten pakken,
            // maar bij het 4de element wordt de 'if'-voorwaarde overgeslagen, omdat ik die optie uitsluit
            // (de 4de optie is altijd een optie om een "job alert" te maken).
            for (int job = 1; job < 7; job++)
            {
                // Dit is de XPath. Ik heb gezien dat ul/li[" + job + "] verandert bij elke job, dus ik kan dit gebruiken om door de jobs te gaan.
                xpath = "/html/body/section/div[1]/div/div[2]/div/div/form/div[2]/div/div/div[2]/section/div/div[2]/div[1]/div/ul/li[" + job + "]";
                // De 'if'-voorwaarde controleert of elk element aanwezig is met een try-catch.
                // Als ik een foutmelding krijg van de try-catch, return ik false, waardoor ik niet in de 'if' terechtkom.
                // Als het element bestaat, return ik true en kan ik in de 'if' doorgaan. Deze 'if'-voorwaarde is er voor het geval een zoekopdracht
                // minder dan 5 jobs oplevert en als het bij het 4de element in de 'for'-loop zit.
                // Als dit niet zou bestaan, zou ik een foutmelding krijgen als er minder dan 5 jobs zijn, omdat de job en zijn elementen dan niet bestaan.
                if (IsElementPresent() && job != 4)
                {
                    // Hier zet ik de elementen in variabelen door de XPath plus een extra XPath te gebruiken die naar het element verwijst.
                    var titel = driver.FindElement(By.XPath(xpath + "/span[2]/a/h2"));
                    var bedrijf = driver.FindElement(By.XPath(xpath + "/span[2]/span[1]"));
                    var locatie = driver.FindElement(By.XPath(xpath + "/span[2]/span[2]/span[2]/span/span"));
                    var keywords = driver.FindElement(By.XPath(xpath + "/span[2]/span[3]"));
                    var link = driver.FindElement(By.XPath(xpath + "/span[2]/a")).GetAttribute("href");

                    // Ik voeg de strings van de variabelen toe aan de 'text'-lijst.
                    text.Add(titel.Text);
                    text.Add(bedrijf.Text);
                    text.Add(locatie.Text);
                    text.Add(keywords.Text.ToString());
                    text.Add(link);
                    // "NextLine", zodat het systeem weet dat het volgende element in de lijst bij een andere job hoort of gewoon het einde van de lijst is.
                    text.Add("NextLine");
                }
            }
            // Schrijf de 'text'-lijst weg op basis van de keuze van de gebruiker door de 'Write'-methode van de logger aan te roepen.
            // Hier krijgt de gebruiker de keuze van wat hij wil doen met de resultaten.
            Logger.Write(text);
        }

        public void ScrapeManga()
        {
            // Ik maak een 'Printer'-object aan zodat ik printermethodes kan gebruiken.
            Printer print = new Printer();

            // Gewoon afdrukken.
            Console.Write("wait...");
            // Open de URL van AsuraToon, ga dus naar de manga-website die ik gebruik.
            driver.Url = "https://asuratoon.com/";

            // Ik wil "wait..." vervangen door de opties, dus ik verwijder die regel en print de opties.
            // https://stackoverflow.com/questions/8946808/can-console-clear-be-used-to-only-clear-a-line-instead-of-whole-console
            Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
            Console.Write("What do you want to do:");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("   - (1) add manga\n                          - (2) remove manga\n                          - (3) see print options\n                          - (4) back\n");

            // Deze methode schrijft gewoon "Choose an option: " en verandert de kleur.
            Printer.ChooseOption();
            // Hier krijgt de gebruiker de kans om iets in te typen en dit wordt opgeslagen in een ConsoleKeyInfo-variabele.
            ConsoleKeyInfo key = Console.ReadKey();
            // OptionPicked is een methode die controleert wat de speler indrukt.
            // Het verandert de variabele optionPicked, die ik gebruik in de switch om te controleren wat de speler heeft ingevoerd.
            // Deze methode heeft ook een lus en een foutcontrole, zodat het systeem niet doorgaat totdat de speler een geldige keuze invoert.
            print.Options(key.Key);

            // Afhankelijk van welke optie de gebruiker koos, gaat de switch naar een andere methode doorverwijzen. 
            // Met de 4de optie (back) gaat de speler terug naar het begin. Hier heb ik geen methode voor nodig.
            // Ik laat dit automatisch gebeuren door gewoon verder te gaan zonder iets te doen.
            switch (print.GetOptionPicked())
            {
                case 1:
                    // Voeg een manga toe.
                    Logger.AddManga();
                    break;
                case 2:
                    // Verwijder een manga.
                    Logger.RemoveManga();
                    break;
                case 3:
                    // Controleer of er nieuwe manga-hoofdstukken zijn. Als er nieuwe manga-hoofdstukken zijn, mag de speler kiezen wat hij
                    // met de output wil doen. Als er geen nieuwe manga-hoofdstukken zijn, wordt dat vermeld en gaat het programma terug naar het begin.
                    PrintManga();
                    break;
                default:
                    // Ik heb 'break' erbij gezet voor de zekerheid. In deze situatie dient het ook voor als optie 4 (back) wordt gekozen.
                    break;
            }

        }

        // Deze methode wordt alleen opgeroepen als het programma wordt uitgevoerd door de taakplanner.
        // De taakplanner opent dus gewoon de link naar AsuraToons zonder opties op te geven en controleert of er nieuwe manga-hoofdstukken zijn
        // door de methode PrintManga(true) uit te voeren. Ik geef 'true' mee met de methode zodat het weet dat de methode wordt opgeroepen door
        // de taakplanner en dus geen keuze moet geven over wat te doen met het resultaat.
        public void PrintMangaNotification()
        {

            driver.Url = "https://asuratoon.com/";
            PrintManga(true);
        }

        public void PrintManga(bool notification = false)
        {
            // 'text' is de lijst die ik doorgeef aan de logger en waar de resultaten in staan.
            // Ik maak de lijst gewoon opnieuw aan omdat "text.Clear()" mij problemen gaf.
            text = new List<string>();

            text.Add("Title");
            text.Add("Chapter");
            // Ik voeg ook een string genaamd 'NextLine' toe. Dit is zodat mijn loggers weten wanneer de headers en elementen per video/job/manga eindigen.
            // Zo weten ze dat ze bijvoorbeeld een lege regel moeten invoegen tijdens het printen naar de console.
            // Ik heb dit zo gemaakt zodat mijn loggers dynamisch zijn en niet hardgecodeerd.
            // Als ik meer elementen wil tonen, zoals wanneer de hoofdstukken zijn geüpload, moet ik alleen de headers ervoor toevoegen en niets
            // veranderen aan mijn code. Het werkt nog steeds perfect.
            text.Add("NextLine");

            // Aanmaken van variabelen.
            string result;
            string chapterString;

            // Ik controleer de laatste 15 manga's.
            for (int manga = 1; manga < 16; manga++)
            {
                // Dit is de XPath. Ik heb gezien dat div[2]/div[" + manga + "] verandert bij elke manga,
                // dus ik kan dit gebruiken om door de manga's te gaan.
                xpath = "/html/body/div[4]/div/div[1]/div[5]/div[2]/div[" + manga + "]";

                // Dit voegt de mangatitel toe aan de variabele 'title'.
                string title = driver.FindElement(By.XPath(xpath + "/div/div[2]/a/h4")).Text;
                // Nu ga ik de hoofdstukken toevoegen aan de variabele 'chapterString'.
                // Deze try-catch is er voor het geval er een nieuwe manga aan de site wordt toegevoegd zonder hoofdstukken.
                // Zonder deze try-catch zou ik een foutmelding krijgen omdat de nieuwe manga waarschijnlijk nog geen hoofdstukken zal hebben en dus
                // de XPath niet zal bestaan.
                try
                {
                    chapterString = driver.FindElement(By.XPath(xpath + "/div/div[2]/ul/li[1]/a")).Text;
                }
                catch
                {
                    chapterString = "0";
                }
                // Het probleem dat ik tegenkom, is dat we nu een string hebben met het formaat "hoofdstuk: nummer" in 'chapterString',
                // terwijl ik gewoon het nummer nodig heb als een int.

                // Dit is de werkelijke variabel die ik ga gebruiken voor de hoofdstukken.
                string chapterToChange = null;

                // Ga door de 'chapterString' en controleer of elk teken een nummer is of niet.
                // Als het een nummer is, voeg je het toe aan 'chapterToChange', zodat 'chapterToChange' een string is met het volledige nummer.
                foreach (char value in chapterString)
                {
                    if (char.IsNumber(value)) // https://www.geeksforgeeks.org/c-sharp-char-isnumber-method/
                    {
                        chapterToChange += value;
                    }
                }

                // Ik geef de 'title' en 'chapterToChange' door aan de ShowManga-methode.
                // Ik gebruik 'Int32.Parse(chapterToChange)' omdat ik de 'chapterToChange'-string wil veranderen in een int.
                // Deze methode retourneert de naam en het hoofdstuk in het formaat "Manga: mangaNaam Chapter: chapterNaam" als de manga op
                // de website overeenkomt met wat ik in mijn 'manga.json' heb staan. Anders retourneert het een null.
                result = Logger.ShowManga(title, Int32.Parse(chapterToChange));

                // Als een manga in mijn 'manga.json' op de website staat met een nieuw hoofdstuk, is 'result' niet null en ga je naar deze 'if'.
                // Het probleem dat we nu hebben, is dat 'result' een lange string is met de koppen "Manga:" en "Chapter:". We willen alleen de waarden hebben.
                // We gaan dus regex gebruiken.
                //https://www.tutorialsteacher.com/regex/regex-in-csharp
                if (result != null)
                {
                    // We zetten het regex-patroon waarop we willen matchen in een string.
                    string pattern = @"Manga:\s*(.*)\sChapter:";
                    // We gebruiken het regex-patroon op ons resultaat.
                    Match match = Regex.Match(result, pattern);
                    // Door de eerste groep op te roepen en het om te zetten naar een string, kunnen we de manganaam krijgen.
                    // We voegen tegelijkertijd de naam ook toe aan onze 'text'-lijst.
                    text.Add(match.Groups[1].ToString());
                    // We doen hetzelfde als hierboven, maar met ons hoofdstuk.
                    pattern = @"Chapter:\s*(.*)";
                    match = Regex.Match(result, pattern);
                    text.Add(match.Groups[1].ToString());
                    text.Add("NextLine");
                }
            }


            // De 'notification' wordt op 'true' gezet als het programma via de taakplanner wordt gestart.
            // Als het handmatig wordt gestart, kom je dus in de 'if'-tak en wordt gecontroleerd of 'text.Count()' gelijk is aan 3.
            // Als het gelijk is aan 3, betekent het dat er geen manga-hoofdstukken zijn toegevoegd en er geen nieuwe hoofdstukken zijn.
            // Als 'text.Count()' niet gelijk is aan 3, word je naar de logger gestuurd en vraagt het systeem wat je wilt doen met het resultaat.
            if (notification == false)
            {
                if (text.Count() == 3)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n\nNo new manga's\n");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine();
                    Logger.Write(text);
                }
            }
            else
            {
                // Als het programma door de taakplanner wordt gestart, kom je hierin terecht.
                // Als 'text.Count()' != 3, dan zijn er geen nieuwe manga-hoofdstukken.
                // Als het niet gelijk is aan 3, zijn er wel nieuwe manga-hoofdstukken, en wordt de 'SendNotification()'-methode opgeroepen.
                // Je geeft dan ook de lijst door aan de 'SendNotification()'-methode.
                if (text.Count() != 3)
                {
                    Logger.SendNotification(text);
                }
            }
        }

        // Dit wordt opgeroepen als ik ergens op moet klikken.
        private void Click(bool withScroll = false)
        {
            // We maken een 'IJavaScriptExecutor'-object aan. Dit object ga ik gebruiken om op het scherm te scrollen.
            // https://dev.to/lambdatest/how-to-scroll-down-a-page-in-selenium-webdriver-using-c-pjg
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            // Deze variabel zorgt ervoor dat we in de while-lus blijven zolang het 'false' is.
            bool clicked = false;

            // Deze while-lus controleert of je op het XPath-element kunt klikken. Als het de XPath niet vindt, krijgt het een foutmelding en komt het in de 'catch'-tak terecht. Als het faalt, begint het opnieuw.
            while (clicked == false)
            {
                // Wacht voor 1 seconde.
                Thread.Sleep(1000);
                try
                {
                    // Klik op het XPath-element. Als hier een fout optreedt omdat het de XPath niet kan vinden, gaat het direct naar de 'catch'-tak en
                    // wordt "clicked = true" niet uitgevoerd.
                    // Als het wel kan klikken op het XPath-element, wordt erop geklikt en wordt "clicked = true" ingesteld, waardoor je uit de lus gaat
                    // als het klaar is.
                    driver.FindElement(By.XPath(xpath)).Click();
                    clicked = true;
                }
                catch
                {
                    // Hier kom je terecht als je een foutmelding krijgt.
                    // Als 'withScroll' gelijk is aan 'true', scroll je naar beneden.
                    // De reden dat ik naar beneden scroll, is omdat ik anders de "date" knop niet kan vinden.
                    if (withScroll)
                    {
                        // Scroll naar beneden om de "date"-knop te zien.
                        // https://dev.to/lambdatest/how-to-scroll-down-a-page-in-selenium-webdriver-using-c-pjg
                        js.ExecuteScript("window.scrollBy(0,250)", "");
                    }
                }
            }
        }

        // Dit is exact hetzelfde als hierboven "Click()", maar dan om te zoeken.
        // Dit heeft ook geen optie om te scrollen omdat ik het niet nodig had.
        private void Search()
        {
            bool searched = false;
            while (searched == false)
            {
                Thread.Sleep(1000);
                try
                {
                    // In plaats van te klikken, moet ik nu de 'searchWord'-variabele zoeken in de zoekbalk.
                    var search = driver.FindElement(By.XPath(xpath));
                    search.SendKeys(searchWord);
                    search.Submit();
                    searched = true;
                }
                catch
                {
                }
            }
        }

        // Deze methode controleert of de XPath bestaat. Als het bestaat, geeft het 'true' terug.
        // Als de XPath niet bestaat, krijg je normaal gesproken een foutmelding en kom je in de 'catch'-tak terecht, waardoor je 'false' terugkrijgt.
        // https://stackoverflow.com/questions/27516545/how-to-check-if-an-element-exists
        private bool IsElementPresent()
        {
            try
            {
                // Zoek de XPath.
                driver.FindElement(By.XPath(xpath));
                // Return 'true' als je de XPath vindt.
                return true;
            }
            catch
            {
                // Return 'false' als je de XPath niet vindt.
                return false;
            }
        }
        public void Quit()
        {
            // Sluit de driver. Dit sluit Google Chrome.
            driver.Quit();
        }

        private void SetSearchWord()
        {
            // Ik wil de invoer van de gebruiker groen maken.
            Console.ForegroundColor = ConsoleColor.Blue;
            // Met 'ReadLine' luister ik naar de invoer van de speler.
            searchWord = Console.ReadLine();
            // Maak de rest van de tekst weer wit.
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

}
