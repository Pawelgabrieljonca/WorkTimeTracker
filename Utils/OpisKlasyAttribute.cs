using System;

namespace WorkTimeTracker.Utils
{
    /// <summary>
    /// Atrybut służący do opisania funkcji klasy w modelu danych.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class OpisKlasyAttribute : Attribute
    {
        public string Opis { get; }
        public string Autor { get; }
        public string DataUtworzenia { get; }

        public OpisKlasyAttribute(string opis)
        {
            Opis = opis ?? throw new ArgumentNullException(nameof(opis));
            Autor = "Nieznany";
            DataUtworzenia = DateTime.Now.ToString("yyyy-MM-dd");
        }

        public OpisKlasyAttribute(string opis, string autor, string dataUtworzenia = null)
        {
            Opis = opis ?? throw new ArgumentNullException(nameof(opis));
            Autor = autor ?? throw new ArgumentNullException(nameof(autor));
            DataUtworzenia = dataUtworzenia ?? DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}