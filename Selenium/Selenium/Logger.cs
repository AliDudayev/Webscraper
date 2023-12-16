using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using PushbulletSharp;
using PushbulletSharp.Models.Requests;

namespace Selenium
{
    class Logger
    {
        // Definieer de bestandspaden. Deze bestanden bevinden zich momenteel op mijn bureaublad.
        private static string filenameManga = Path.Combine(@"C:\Users\Nexes\Desktop\manga.json");
        private static string filenameCsv = Path.Combine(@"C:\Users\Nexes\Desktop\log.csv");
        private static string filenameJson = Path.Combine(@"C:\Users\Nexes\Desktop\log.json");

        // Een dictionary voor een string en int.
        private static Dictionary<string, int> oldListJson;

        // Definieer de methode voor het toevoegen van manga aan.
        public static void AddManga()
        {
            // Printen met kleurwijzigingen voor visuele opmaak.
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n\nGive the manga name: ");
            Console.ForegroundColor = ConsoleColor.Blue;

            // Sla de gebruikersinvoer op in de variabele "name".
            string name = Console.ReadLine();

            // Printen met kleurwijzigingen voor visuele opmaak.
            Console.Write("\nGive the manga chapters: ");
            Console.ForegroundColor = ConsoleColor.Blue;

            // Sla de gebruikersinvoer op in de variabele "chapters".
            string chapters = Console.ReadLine();

            // Gebruik Console.ReadLine() om een string te krijgen. Controleer vervolgens of "chapters" een integer is en sla het op in
            // de variabele "chaptersButInt".
            int chaptersButInt;
            // Hier wordt gekeken of de chapters die de gebruiker meegaf werkelijk een int is.
            // "Int32.TryParse" probeert de "chapters" om te zetten naar een int.
            // Als dit lukt wordt "true" terug gegeven en wordt de omgezete int in "chaptersButInt" gezet.
            // Als de omgezetting niet lukt wordt "false" terug gegeven en ga je in de loop.
            // https://stackoverflow.com/questions/4804968/how-can-i-validate-console-input-as-integers
            while (!Int32.TryParse(chapters, out chaptersButInt))
            {
                // Toon een foutmelding, vraag opnieuw om manga chapters en wijzig de kleur.
                Printer.ErrorMessage();
                Console.Write("\nGive the manga chapters: ");
                Console.ForegroundColor = ConsoleColor.Blue;
                // Laat de gebruiker nog eens een input geven.
                chapters = Console.ReadLine();
            }

            // Geef de naam en omgezette chapters mee met de MangaFile-methode.
            MangaFile(name, chaptersButInt);
        }

        public static void SendNotification(List<string> message)
        {
            // Maak een apiKey-string aan voor het verzenden van pushmeldingen naar mijn account.
            // Creëer een clientobject met de apiKey.
            // https://en.code-bude.net/2018/02/14/send-push-notifications-c/
            string apiKey = "o.tS0Tyxn1uWUcYQyLfcKAdtBARKc4oEfW";
            PushbulletClient client = new PushbulletClient(apiKey);

            // Haal informatie op van de account die gelinkt is aan de API key.
            var currentUserInformation = client.CurrentUsersInformation();

            // Dit is de string die ik ga versturen als notificatie.
            string notification = "";

            // Bepaal het aantal headers in de lijst en gebruik "whereToStart" hieronder om de positie van de werkelijke values te bepalen.
            int whereToStart = WhereHeader(message);

            // Deze for-loop begint niet aan het begin van de lijst vanwege de headers.
            // Het begint waar de waarden in de lijst beginnen, zoals bepaald door het aantal headers.
            // De variabele "count" wordt gebruikt om headers toe te voegen en "int x" om waarden toe te voegen.
            int count = 0;
            for (int x = whereToStart + 1; x < message.Count(); x++)
            {

                // Als de waarde gelijk is aan "NextLine", reset de count voor het afdrukken van headers voor de volgende manga.
                // Voeg ook een "\n" toe om naar de volgende regel te gaan.
                // Als "message[x]" niet gelijk is aan "NextLine" en een waarde van de manga is, ga naar het "else".
                if (message[x] == "NextLine")
                {
                    count = 0;
                    notification += "\n";
                }
                else
                {
                    // Voeg eerst een header toe aan de "notification"-variabele.
                    notification += message[count] + ": ";
                    // Voeg vervolgens de waarde toe aan de "notification"-variabele.
                    notification += message[x] + "\n";
                    // Verhoog daarna de count om de volgende header toe te voegen.
                    count++;
                }
            }

            // Controleer of gebruikersgegevens kunnen worden opgehaald.
            if (currentUserInformation != null)
            {
                // Maak een request aan.
                PushNoteRequest request = new PushNoteRequest
                {
                    // Titel van de notificatie.
                    Title = "New Manga chapters",
                    // In de body zet ik de variabele "notification" met alle headers, manga-namen en manga-chapters.
                    Body = notification

                    // Een voorbeeld van de mogelijke opmaak:
                        //New Manga chapters
                        //Title: the greatest estate developer
                        //Chapter: 119

                        //Title: superhuman battlefield
                        //Chapter: 84
                };

                // Verzendt de pushnotificatie.
                client.PushNote(request);
            }
        }

        // Deze public methode is bedoeld om een manga te verwijderen van buiten de klasse.
        // Hier wordt tekst weergegeven, wordt de gebruiker in staat gesteld invoer te geven, en wordt de methode opgeroepen
        // die daadwerkelijk de manga zal verwijderen.
        public static void RemoveManga()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n\nGive the manga name: ");
            Console.ForegroundColor = ConsoleColor.Blue;
            string name = Console.ReadLine();

            // De "add:false"-parameter geeft aan dat de add-parameter op false staat. Hierdoor zal de "MangaFile()" methode
            // worden gebruikt voor het verwijderen in plaats van het toevoegen van manga.
            MangaFile(name, add: false);
        }

        // Deze methode wordt zowel gebruikt bij het toevoegen als verwijderen van manga's.
        // Om deze methode te gebruiken voor het verwijderen van manga, moet ik eenvoudigweg de bool "add" op false zetten.
        // De -99 wordt gebruikt omdat het opgeven van een chapter niet van belang is. Hoewel de gebruiker wordt gedwongen
        // om een chapter mee te geven door error-checking, maakt het in werkelijkheid niet uit.
        // Als er een update is, wordt het hoofdstuk in de manga.json hoe dan ook overschreven.
        private static void MangaFile(string name, int chapter = -99, bool add = true)
        {
            // Maak een bool die check of het "manga.json" bestand bestaat
            bool fileExists = true;
 
            // Zet de meegegeven naam naar lowercase om problemen te voorkomen met hoofdletters.
            name = name.ToLower();

            // Maak een nieuwe dictionary aan.
            // https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-8.0&redirectedfrom=MSDN
            oldListJson = new Dictionary<string, int>();

            // Als het "manga.json"-bestand bestaat, plaats de manga-gegevens in de variabelen. Anders, zet "fileExists" op false.
            if (ReadMangaList() != null)
            {
                oldListJson = ReadMangaList();
            }
            else
            {
                fileExists = false;
            }

            // Print de volgende berichten in het groen.
            Console.ForegroundColor = ConsoleColor.Green;

            // Als deze methode wordt opgeroepen om een manga toe te voegen, is "add" true.
            // Als deze methode wordt opgeroepen om een manga te verwijderen, is "add" false.
            if (add)
            {
                try
                {
                    // Probeer een manga toe te voegen; als de naam al bestaat, wordt een fout gegenereerd en wordt de "catch" uitgevoerd.
                    // Bij succes wordt de manga toegevoegd met zijn hoofdstuk.
                    oldListJson.Add(name, chapter);
                    Console.WriteLine("\n\nAdded manga\n");
                }
                catch (ArgumentException)
                {
                    // Als de catch wordt bereikt, betekent dit dat de manga naam al bestaat in het bestand.
                    // Daarom wordt de manga eerst verwijderd en vervolgens opnieuw toegevoegd, maar deze keer met het nieuwe hoofdstuk.
                    oldListJson.Remove(name);
                    oldListJson.Add(name, chapter);
                    Console.WriteLine("\n\nChanged manga chapters\n");
                }
            }
            else
            {
                // Voer dit stuk code alleen uit als het "manga.json"-bestand bestaat, anders wordt "Manga is not in list" afgedrukt.
                if (fileExists)
                {
                    // Als de manga naam in de lijst overeenkomt met de opgegeven naam door de gebruiker, wordt de manga verwijderd.
                    // Ik stel ook "mangaFound" in op true om te voorkomen dat "Manga is not in list" wordt afgedrukt.
                    bool mangaFound = false;
                    foreach (var item in oldListJson)
                    {
                        if (item.Key == name)
                        {
                            oldListJson.Remove(name);
                            Console.WriteLine("\n\nRemoved manga\n");
                            mangaFound = true;
                            break;
                        }
                    }

                    // Dit wordt alleen bereikt als de foreach geen overeenkomende manga naam vindt.
                    if (mangaFound == false)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n\nManga is not in list\n");
                    }
                }
            }

            // Schrijf "oldListJson" naar het "manga.json"-bestand.
            WriteMangaList(oldListJson);
            // Houd de console open.
            Console.ReadLine();
        }

        // Deze methode wordt gebruikt om manga te vergelijken en te controleren of er een nieuwe manga-hoofdstuk beschikbaar is.
        // Het ontvangt de naam en het hoofdstuk van een manga op de website en vergelijkt deze met die in het "manga.json"-bestand.
        public static string ShowManga(string manga, int chapter)
        {
            // Maak een nieuwe dictionarylijst aan.
            oldListJson = new Dictionary<string, int>(); // https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-8.0&redirectedfrom=MSDN

            // Zet alles naar kleine letters om eenvoudig te kunnen vergelijken.
            manga = manga.ToLower();

            // Definieer de variabelen die we zullen retourneren.
            string result = null;

            // "ReadMangaList()" controleert of het "manga.json"-bestand bestaat. Als het niet bestaat, wordt null geretourneerd.
            if (ReadMangaList() != null)
            {
                // Plaats de manga dictionary in "oldListJson"  zodat we het kunnen gebruiken.
                oldListJson = ReadMangaList();
                string mangaNameInFile = "";

                foreach (var item in oldListJson)
                {

                    // Dit wordt alleen uitgevoerd als de naam van de manga op de website overeenkomt met de naam van de manga in "oldListJson"
                    // en als het hoofdstuk in "oldListJson" lager is dan het hoofdstuk op de website. Let op dat we "item.Key.ToLower()" gebruiken
                    // om niet met hoofdletters te werken, omdat anders dezelfde naam hoofdletters kan hebben en een andere geen hoofdletters kan hebben.
                    if (item.Key.ToLower() == manga && item.Value < chapter)
                    {
                        // Voeg de manga en het hoofdstuk toe aan het resultaat dat we zullen retourneren.
                        result = "Manga: " + manga + " Chapter: " + chapter;

                        // We slaan de originele naam met hoofdletters en al op in een variabele.
                        // Dit is nodig om de manga uit de lijst te halen en omdat ik de originele naam met hoofdletters en al terug in de lijst wil plaatsen.
                        mangaNameInFile = item.Key;
                    }
                }

                // Als er een resultaat is, gaan we in deze "if".
                if (result != null)
                {
                    // Verwijder de manga uit de lijst en voeg deze opnieuw toe met het nieuwe hoofdstuk.
                    oldListJson.Remove(mangaNameInFile);
                    oldListJson.Add(mangaNameInFile, chapter);
                }

            }

            // Schrijf "oldListJson" naar het "manga.json"-bestand.
            WriteMangaList(oldListJson);
            //return het resultaat.
            return result;
        }


        private static Dictionary<string, int> ReadMangaList()
        {
            // Probeer het bestand te openen; als dit niet lukt (omdat het niet bestaat), wordt een fout gegenereerd en wordt naar de catch gegaan.
            try
            {
                // Open het bestand met de bestandsnaam van de variabele "filenameManga".
                // "filenameManga" is het pad naar het "manga.json"-bestand.
                using (StreamReader file = new StreamReader(filenameManga))
                {
                    // Lees het bestand tot het einde en sla het op in de string "fileRead".
                    string fileRead = file.ReadToEnd();
                    // Deserialiseer de JSON-inhoud naar een dictionary met een string en int.
                    return JsonConvert.DeserializeObject<Dictionary<string, int>>(fileRead);
                }
            }
            catch (FileNotFoundException)
            {
                Console.Write("\n\nNo file found -> making a new manga.json file");
            }
            return null;
        }

        private static void WriteMangaList(Dictionary<string, int> oldListJsonParameter)
        {
            // Converteer de dictionary naar JSON-formaat.
            // https://www.delftstack.com/howto/csharp/csharp-write-json-to-file/
            string newListJson = JsonConvert.SerializeObject(oldListJsonParameter);

            // Open een StreamWriter om het bestand weg te schrijven naar het opgegeven pad in "filenameManga".
            // De "filenameManga" is het pad naar het "manga.json"-bestand.
            // De "false"-parameter geeft aan dat als het bestand al bestaat, het moet worden overschreven.
            using (StreamWriter file = new StreamWriter(filenameManga, false))
            {
                // Schrijf de JSON-string naar het bestand.
                file.WriteLine(newListJson.ToString());
            }
        }

        // Deze methode wordt gebruikt om naar het CSV-bestand te loggen.
        // https://www.youtube.com/watch?v=vDpww7HsdnM
        private static void LogToCsv(List<string> message)
        {
            // Printen en kleur veranderen.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nSent to Csv-file on desktop\n");

            // Open een StreamWriter om het bestand weg te schrijven naar het opgegeven pad in "filenameCsv".
            // De "filenameCsv" is het pad naar het log.csv-bestand.
            // De "false"-parameter geeft aan dat als het bestand al bestaat, het moet worden overschreven.
            using (StreamWriter file = new StreamWriter(filenameCsv, false))
            {
                // Controleer elke waarde in de meegegeven "message" lijst.
                foreach (string value in message)
                {
                    // Als de waarde niet gelijk is aan "NextLine", voeg dan de waarde toe aan het bestand.
                    // Anders moet ik naar de volgende regel in het bestand gaan.
                    if (value != "NextLine")
                    {
                        // Schrijf de waarde tussen "" en voeg vervolgens een komma toe.
                        file.Write($"\"{value}\"" + ", ");
                    }
                    else
                    {
                        // Ga naar de volgende regel.
                        file.WriteLine();
                    }
                }
            }
        }

        private static void LogToJson(List<string> message)
        {
            // Printen en kleur veranderen.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nSent to Json-file on desktop\n");

            // Maak een dictionary aan met twee strings en een lijst van dictionaries met twee strings.
            // https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-8.0&redirectedfrom=MSDN
            // https://stackoverflow.com/questions/40375799/add-dictionary-to-dictionary-list
            Dictionary<string, string> listJson = new Dictionary<string, string>();
            List<Dictionary<string, string>> listDictionary = new List<Dictionary<string, string>>();

            // Bepaal hier hoeveel headers er in de lijst zitten. De variabele "whereToStart" wordt hieronder gebruikt
            // om te bepalen waar de werkelijke waarden in de lijst staan.
            int whereToStart = WhereHeader(message);

            // JSON-bestanden vereisen een sleutel en een waarde voor elk element, bijvoorbeeld "key": "value" (bijvoorbeeld "Locatie": "Leuven").
            // Daarom moeten we in de foreach eerst bepalen waar de headers zijn en waar de waarden beginnen.
            // Ik zal regel voor regel uitleggen wat ik doe want het kan snel ingewikkeld worden.
            // Ik maak een "count"-variabele aan. Deze "count"-variabele wordt gebruikt om de sleutel (header) te bepalen.
            // Dit wordt gedaan met "message[count]".
            // Neem bijvoorbeeld het resultaat van "jobs". Als "count" gelijk is aan 0, zou je de sleutel (headers) "title" krijgen , als
            // "count" gelijk is aan 1, zou je de sleutel "Bedrijf" (header) krijgen, enz.

            int count = 0;

            // Een for-loop die niet bij het begin begint, maar waar de waarden starten. Dit komt omdat we ook headers in de lijst hebben.
            // Eerder heb ik berekend hoeveel headers er in de lijst zijn, dus nu kan ik starten met "int x = whereToStart + 1;"
            // en gewoon de headers over te slaan.
            for (int x = whereToStart + 1; x < message.Count(); x++)
            {
                if (message[x] == "NextLine")
                {
                    // Als ik "NextLine" tegenkom, zet ik "count" op 0 zodat we opnieuw door de headers kunnen gaan.
                    count = 0;

                    // Ik voeg de "listJson" toe aan de eindresultaat "listDictionary". Mijn JSON is een lijst met dictionaries, dus ik voeg
                    // de tijdelijke dictionary "listJson" toe aan de eindresultaat "listDictionary". "listJson" bevat een dictionary met de
                    // headers als keys en de resultaten als values.
                    listDictionary.Add(listJson);

                    // Ik gebruik niet "listJson.Clear()" omdat dit niet werkt. Door "listDictionary.Add(listJson);" wordt er een verwijzing gemaakt tussen
                    // de twee, dus als ik "listJson.Clear()" zou gebruiken, wordt "listDictionary" ook leeggemaakt. Daarom maak ik gewoon een nieuwe lijst aan.
                    // Deze "listJson" is een tijdelijke dictionary waarin ik de headers als sleutels en de resultaten als waarden toevoeg.
                    listJson = new Dictionary<string, string>();
                }
                else
                {
                    // Als het element niet "NextLine" is, weet ik dat het een waarde is, dus voeg ik het toe aan de dictionary met de bijbehorende header.
                    // "message[count]" geeft de header en "message[x]" geeft de waarde.
                    listJson.Add(message[count], message[x]);

                    // Ik verhoog de count zodat de volgende message[x] de volgende header krijgt met message[count] tijdens het toevoegen aan de dictionary
                    // hierboven met listJson.Add.
                    count++;
                }
            }

            // Converteer de lijst met dictionaries naar JSON-formaat.
            // https://www.delftstack.com/howto/csharp/csharp-write-json-to-file/
            string json = JsonConvert.SerializeObject(listDictionary);

            // Open een StreamWriter om het bestand weg te schrijven naar het opgegeven pad in "filenameJson".
            // "filenameJson" is het pad naar het log.json-bestand.
            // De "false"-parameter geeft aan dat als het bestand al bestaat, het moet worden overschreven.
            using (StreamWriter file = new StreamWriter(filenameJson, false))
            {
                file.WriteLine(json.ToString());
            }

        }

        // Methode om het resultaat naar de console te schrijven.
        private static void LogToConsole(List<string> message)
        {
            // Eerst maak ik de console schoon.
            Console.Clear();

            // Bepaal hier hoeveel headers er in de lijst zitten. De variabele "whereToStart" wordt hieronder gebruikt
            // om te bepalen waar de werkelijke waarden in de lijst staan.
            int whereToStart = WhereHeader(message);

            // Als de "message" zonder de headers geen items (waarde) bevat, ga dan naar de "else" en print dat er geen resultaten zijn.
            if ((message.Count() - whereToStart) != 1)
            {
                // Een for-loop die niet bij het begin begint, maar waar de waarden starten. Dit komt omdat we ook headers in de lijst hebben.
                // Eerder heb ik berekend hoeveel headers er in de lijst zijn, dus nu kan ik beginnen met "int x = whereToStart + 1;"
                // en gewoon de headers overslaan.
                int count = 0;
                for (int x = whereToStart + 1; x < message.Count(); x++)
                {
                    if (message[x] == "NextLine")
                    {
                        // Als ik "NextLine" tegenkom, zet ik "count" op 0 zodat we opnieuw door de headers kunnen gaan.
                        count = 0;
                        // Ik maak een lege regel voor duidelijkheid.
                        Console.WriteLine();
                    }
                    else
                    {
                        // Verander de kleur.
                        Console.ForegroundColor = ConsoleColor.White;
                        // Schrijf de header plus ":".
                        Console.Write(message[count] + ": ");
                        // Verander de kleur.
                        Console.ForegroundColor = ConsoleColor.Blue;
                        // Schrijf de waarde.
                        Console.WriteLine(message[x]);
                        // Verhoog "count" om de volgende header te pakken.
                        count++;
                    }
                }
            }
            else
            {
                // Print dat er geen resultaten zijn.
                Console.WriteLine("No result!");
            }
        }

        private static int WhereHeader(List<string> message)
        {
            // Bepaal hier hoeveel headers er in de lijst zitten. De variabele "whereToStart" wordt later gebruikt
            // om te bepalen waar de werkelijke waarden in de lijst staan.
            int whereToStart = 0;
            foreach (string value in message)
            {
                // "whereToStart" wordt vergroot totdat de waarde gelijk is aan "NextLine", waarna we de foreach verlaten.
                // Op dat moment is "whereToStart" de lengte van waar de headers eindigen en de waarden beginnen in de lijst.
                if (value == "NextLine")
                {
                    break;
                }
                whereToStart++;
            }
            return whereToStart;
        }

        public static void Write(List<string> message)
        {
            // Print de opties van de speler.
            Console.Write("\nWhat do you want to do with the result (multiple options possible):");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("  - (1) print to CONSOLE\n                                                                     - (2) print to CSV\n                                                                     - (3) print to JSON\n");

            // Maak een variabele voor de loop.
            bool choseSomething = false;

            // Blijf in deze lus zolang er nog geen juiste keuze gemaakt is.
            while (choseSomething == false)
            {
                // Geeft de gebruiker de mogelijkheid om input te geven. 
                // Hier kan de gebruiker meerdere opties kiezen, daarom gebruik ik "ReadLine()" in plaats van "ReadKey()".
                string option = Console.ReadLine();

                // Loop door de tekens van de invoerstring.
                // "option.Distinct()" zorgt ervoor dat de string distinct is, waardoor dezelfde optie niet meerdere keren kan worden gekozen.
                foreach (char value in option.Distinct())
                {
                    switch (value)
                    {
                        // Roep de "case" aan die overeenkomt met het opgegeven karakter.
                        // Meerdere "cases" kunnen worden gekozen omdat de gebruiker een hele string meegeeft.
                        case '1':
                            LogToConsole(message);
                            choseSomething = true;
                            break;
                        case '2':
                            LogToCsv(message);
                            choseSomething = true;
                            break;
                        case '3':
                            LogToJson(message);
                            choseSomething = true;
                            break;
                        default:
                            break;
                    }
                }

                // Dit is de errorcheck. Als de gebruiker niets kiest, wordt er een foutmelding weergegeven en wordt gevraagd opnieuw te kiezen.
                if (choseSomething == false)
                {
                    Printer.ErrorMessage();
                    Console.WriteLine();
                    Printer.ChooseOption();
                }
            }
            // Wacht op de input van de gebruiker.
            Console.ReadLine();
        }
    }
}
