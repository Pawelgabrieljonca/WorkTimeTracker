using System;

namespace WorkTimeTracker.Utils
{
    /// <summary>
    /// Własny wyjątek zgłaszany gdy napotkane zostaną nieprawidłowe dane wejściowe.
    /// </summary>
    public class NieprawidloweDaneException : Exception
    {
        public NieprawidloweDaneException() { }

        public NieprawidloweDaneException(string message) : base(message) { }

        public NieprawidloweDaneException(string message, Exception inner) : base(message, inner) { }
    }
}
