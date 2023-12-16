using System;

namespace Selenium
{
    // Deze website heeft me geholpen bij het opzetten van het programma.
    // https://medium.com/@ertekinozturgut/basic-web-scraping-via-selenium-in-c-for-google-news-c012c2b4939d
    // De video heeft me alles uitgelegd over Selenium met betrekking tot XPaths.
    // https://www.youtube.com/watch?v=CpugqTr2j60
    // De video heeft me alles uitgelegd over het gebruik van de taakplanner.
    // https://www.youtube.com/watch?v=KQpw_OYkKq8&t=183s

    class Program
    { 
        static void Main(string[] args)
        {
            // Maak het scraper object aan.
            Scraper scraper = new Scraper();

            // Met de taakplanner geef ik een lange string mee, waardoor args.Length niet gelijk is aan 0.
            // Als de taakplanner het programma start, komt de uitvoering in de 'else' terecht, niet in deze 'if'.
            // Wanneer ik het programma zelf start, is "args.Length == 0", waardoor de uitvoering in deze 'if' plaatsvindt.
            // 'args' is de parameter die je kunt meegeven bij het starten van het programma. Als ik het zelf start, geef ik dus geen
            // parameter mee. Als de taakplanner het start, wordt wel een parameter meegegeven.
            // https://www.codeproject.com/Questions/507701/ConsoleplusappplusargumentsplusfromplusTaskplusSch
            if (args.Length == 0)
            {
                // Maak het printer object aan.
                Printer print = new Printer();

                // Deze lus voert het hele programma uit en bevat de begin opties die een gebruiker ziet.

                bool loop = true;
                while (loop)
                {
                    // Print het intro.
                    // Het intro toont de beschikbare opties en vraagt de gebruiker om een keuze te maken.
                    Console.Clear();
                    print.Intro();

                    // GetOptionPicked() is een getter voor optionPicked.
                    // Op deze manier houdt het systeem bij wat de speler heeft gekozen.
                    switch (print.GetOptionPicked())
                    {

                        // De OptionChosen() methode toont je keuze.
                        // De Scrape-methodes voeren de daadwerkelijke scraping van de websites uit.
                        case 1:
                            print.OptionChosen("Youtube video");
                            scraper.ScrapeYoutube();
                            break;
                        case 2:
                            print.OptionChosen("Jobsite");
                            scraper.ScrapeJobs();
                            break;
                        case 3:
                            print.OptionChosen("Manga");
                            scraper.ScrapeManga();
                            break;
                        case 4:
                            // Optie 4 is 'exit', dus je stelt de lusconditie in op false en verlaat het programma.
                            loop = false;
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                // Deze methode wordt alleen opgeroepen als het programma wordt uitgevoerd door de taakplanner.
                scraper.PrintMangaNotification();
            }

            // Roep de methode op om de browser te sluiten.
            scraper.Quit();

        }
    }
}

