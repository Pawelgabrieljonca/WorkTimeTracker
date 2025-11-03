using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WorkTimeTracker.Utils
{
    /// <summary>
    /// Klasa narzędziowa do analizy refleksyjnej modeli.
    /// </summary>
    public static class ModelAnalyzer
    {
        /// <summary>
        /// Zwraca opisy wszystkich klas oznaczonych atrybutem OpisKlasy w podanym assembly.
        /// </summary>
        public static string PobierzOpisyKlas(Assembly assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();
            var builder = new StringBuilder();

            builder.AppendLine("DOKUMENTACJA KLAS MODELU");
            builder.AppendLine("=======================");
            builder.AppendLine();

            var oznaczoneKlasy = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<OpisKlasyAttribute>() != null)
                .OrderBy(t => t.Name);

            foreach (var klasa in oznaczoneKlasy)
            {
                var atrybut = klasa.GetCustomAttribute<OpisKlasyAttribute>();
                builder.AppendLine($"Klasa: {klasa.Name}");
                builder.AppendLine($"Opis: {atrybut.Opis}");
                builder.AppendLine($"Autor: {atrybut.Autor}");
                builder.AppendLine($"Data utworzenia: {atrybut.DataUtworzenia}");
                builder.AppendLine(new string('-', 50));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Zwraca słownik z opisami klas, gdzie kluczem jest nazwa klasy.
        /// </summary>
        public static Dictionary<string, (string Opis, string Autor, string Data)> PobierzMapęOpisów(Assembly assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();

            return assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<OpisKlasyAttribute>() != null)
                .ToDictionary(
                    t => t.Name,
                    t =>
                    {
                        var atrybut = t.GetCustomAttribute<OpisKlasyAttribute>();
                        return (atrybut.Opis, atrybut.Autor, atrybut.DataUtworzenia);
                    });
        }

        /// <summary>
        /// Sprawdza, czy dana klasa posiada atrybut OpisKlasy.
        /// </summary>
        public static bool CzyKlasaPosiadaOpis(Type typ)
        {
            return typ.GetCustomAttribute<OpisKlasyAttribute>() != null;
        }

        /// <summary>
        /// Pobiera opis konkretnej klasy, jeśli istnieje.
        /// </summary>
        public static (bool Istnieje, string Opis, string Autor, string Data) PobierzOpisKlasy(Type typ)
        {
            var atrybut = typ.GetCustomAttribute<OpisKlasyAttribute>();
            if (atrybut == null)
                return (false, null, null, null);

            return (true, atrybut.Opis, atrybut.Autor, atrybut.DataUtworzenia);
        }
    }
}