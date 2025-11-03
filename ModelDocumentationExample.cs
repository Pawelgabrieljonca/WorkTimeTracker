using System;
using WorkTimeTracker.Utils;

namespace WorkTimeTracker
{
    public static class ModelDocumentationExample
    {
        public static void WyswietlDokumentacjeModeli()
        {
            Console.WriteLine("Analiza modeli aplikacji WorkTimeTracker");
            Console.WriteLine("=======================================");
            Console.WriteLine();

            var opisy = ModelAnalyzer.PobierzOpisyKlas();
            Console.WriteLine(opisy);

            // Przykład użycia konkretnej klasy
            var (istnieje, opis, autor, data) = ModelAnalyzer.PobierzOpisKlasy(typeof(Models.Pracownik));
            if (istnieje)
            {
                Console.WriteLine("Szczegółowa analiza klasy Pracownik:");
                Console.WriteLine($"Opis: {opis}");
                Console.WriteLine($"Autor: {autor}");
                Console.WriteLine($"Data: {data}");
            }

            // Przykład użycia mapy opisów
            var mapa = ModelAnalyzer.PobierzMapęOpisów();
            foreach (var (nazwaKlasy, dane) in mapa)
            {
                Console.WriteLine($"\nKlasa: {nazwaKlasy}");
                Console.WriteLine($"Opis: {dane.Opis}");
            }
        }
    }
}