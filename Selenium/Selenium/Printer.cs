using System;

namespace Selenium
{
    class Printer
    {
        // Een ConsoleKeyInfo-variabele om de informatie bij te houden over welke toets de gebruiker heeft ingedrukt.
        // https://learn.microsoft.com/en-us/dotnet/api/system.console.readkey?view=net-8.0
        // https://www.geeksforgeeks.org/console-readkey-method-in-c-sharp/
        private ConsoleKeyInfo key; 

        // Dit is de variabele die verandert op basis van de gekozen optie.
        // Deze wordt ook gebruikt door het 'Program'-klasse om te bepalen wat je hebt gekozen.
        private int optionPicked = 0;

        // Deze methode toont het intro en biedt de gebruiker een keuze aan.
        public void Intro()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Welcome to the Selenium webscraper made by ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Magomed-Ali Dudayev\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("You have three scraping options:");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("  - (1) Youtube video\n                                  - (2) Jobsite\n                                  - (3) Manga\n                                  - (4) Exit\n");
            ChooseOption();

            // Ik gebruik Console.ReadKey(), hoewel ik ook Console.ReadLine() had kunnen gebruiken.
            // Mijn bedoeling was te tonen dat ik ook Console.ReadKey() kon gebruiken, wat naar mijn mening iets complexer is dan Console.ReadLine().
            // Deze methode vangt één toets op die de gebruiker indrukt.
            key = Console.ReadKey();
            // Ik wil de ConsoleKey van de ingedrukte toets krijgen, daarom gebruik ik key.Key.
            Options(key.Key);
        }
        public void Options(ConsoleKey keyPressed)
        {
            // Ik stel optionPicked in op -99. Ik heb gewoon een willekeurig absurd getal gekozen.
            // Elk ander getal zou ook werken, zolang het maar niet 1, 2, 3 of 4 was.
            optionPicked = -99;

            while (optionPicked == -99)
            {
                // Ik controleer welke toets er is ingedrukt.
                switch (keyPressed)
                {
                    // Ik wil controleren voor zowel de toetsen op het numerieke toetsenbord als de reguliere toetsen, dus ik gebruik
                    // ConsoleKey.NumPad1 en ConsoleKey.D1.
                    case ConsoleKey.NumPad1:
                    case ConsoleKey.D1:
                        // Als de gebruiker '1' indrukte op het numerieke toetsenbord of het reguliere toetsenbord, komt de uitvoering hier terecht.
                        // Ik verander optionPicked naar de keuze van de gebruiker.
                        optionPicked = 1;
                        break;
                    case ConsoleKey.NumPad2:
                    case ConsoleKey.D2:
                        optionPicked = 2;
                        break;
                    case ConsoleKey.NumPad3:
                    case ConsoleKey.D3:
                        optionPicked = 3;
                        break;
                    case ConsoleKey.NumPad4:
                    case ConsoleKey.D4:
                        optionPicked = 4;
                        break;
                    default:
                        // ErrorMessage en ChooseOption zijn gewoon methodes die de kleur aanpassen en tekst weergeven.
                        ErrorMessage();
                        ChooseOption();
                        // De gebruiker krijgt opnieuw de mogelijkheid om een toets in te drukken.
                        keyPressed = Console.ReadKey().Key;
                        break;
                }
            }
        }

        // Methode om het foutbericht te tonen.
        public static void ErrorMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\nERROR! This is not an option! \n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        // Dit geeft gewoon de scraper-optie weer die de speler aan het begin heeft gekozen.
        public void OptionChosen(string optionText)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("You have chosen ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(optionText + "\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        // Toont eenvoudigweg aan dat de speler een optie moet kiezen en verandert de tekstkleur.
        public static void ChooseOption()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Choose an option: ");
            Console.ForegroundColor = ConsoleColor.Blue;
        }

        // Getter voor optionPicked.
        public int GetOptionPicked()
        {
            return optionPicked;
        }
    }
}
