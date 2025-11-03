using System;
using System.Text;
using System.IO;
using WorkTimeTracker.Interfaces;
using WorkTimeTracker.Models;
using WorkTimeTracker.Utils;

namespace WorkTimeTracker.Models
{
    /// <summary>
    /// Abstrakcyjna klasa bazowa dla wszystkich typów raportów.
    /// </summary>
    public abstract class Raport : IRaportowalny
    {
        /// <summary>
        /// Zdarzenie wywoływane po wygenerowaniu raportu. Statyczne, aby można było podpiąć globalnych słuchaczy.
        /// </summary>
        public static event EventHandler<RaportEventArgs>? RaportWygenerowano;

        protected readonly StringBuilder _builder;
        protected DateTime _dataGenerowania;
        protected string _tytul;

        protected Raport(string tytul)
        {
            _builder = new StringBuilder();
            _dataGenerowania = DateTime.Now;
            _tytul = tytul;
        }

        protected virtual void DodajNaglowek()
        {
            _builder.AppendLine(new string('=', 50));
            _builder.AppendLine(_tytul.ToUpper());
            _builder.AppendLine($"Wygenerowano: {_dataGenerowania:g}");
            _builder.AppendLine(new string('=', 50));
            _builder.AppendLine();
        }

        protected virtual void DodajStopke()
        {
            _builder.AppendLine();
            _builder.AppendLine(new string('-', 50));
            _builder.AppendLine($"Koniec raportu - {_dataGenerowania:d}");
        }

        public abstract string GenerujRaport();

        public virtual void ZapiszRaport(string sciezka)
        {
            if (string.IsNullOrEmpty(sciezka))
                throw new ArgumentException("Ścieżka nie może być pusta.", nameof(sciezka));

            var raport = GenerujRaport();
            File.WriteAllText(sciezka, raport, Encoding.UTF8);
        }

        protected virtual void WyczyscBuilder()
        {
            _builder.Clear();
        }

        /// <summary>
        /// Wywołuje zdarzenie informujące, że raport został wygenerowany.
        /// </summary>
        /// <param name="sciezkaPliku">Opcjonalna ścieżka zapisanego raportu.</param>
        protected void OnRaportWygenerowano(string? sciezkaPliku = null)
        {
            try
            {
                RaportWygenerowano?.Invoke(this, new RaportEventArgs(_tytul, _dataGenerowania, sciezkaPliku));
            }
            catch
            {
                // Nie przerywamy generowania raportu z powodu błędów w handlerach zdarzeń
            }
        }
    }
}