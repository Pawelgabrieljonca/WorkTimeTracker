using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WorkTimeTracker.Models;

namespace WorkTimeTracker.Utils
{
    public class WpisCzasuJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(WpisCzasu).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var typWpisu = jo["TypWpisu"]?.Value<string>();

            WpisCzasu wpis = typWpisu switch
            {
                "ZwyklyDzien" => new ZwyklyDzien(
                    jo["Data"].Value<DateTime>(),
                    jo["LiczbaGodzin"].Value<decimal>()),

                "Nadgodziny" => new Nadgodziny(
                    jo["Data"].Value<DateTime>(),
                    jo["LiczbaGodzin"].Value<decimal>()),

                "Urlop" => new Urlop(jo["Data"].Value<DateTime>()),

                _ => throw new JsonSerializationException($"Nieznany typ wpisu czasu: {typWpisu}")
            };

            serializer.Populate(jo.CreateReader(), wpis);
            return wpis;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jo = new JObject();
            var type = value.GetType();

            // Dodaj informację o typie
            jo.Add("TypWpisu", type.Name);

            // Skopiuj wszystkie właściwości
            foreach (var prop in type.GetProperties())
            {
                if (prop.CanRead)
                {
                    var val = prop.GetValue(value);
                    jo.Add(prop.Name, JToken.FromObject(val, serializer));
                }
            }

            jo.WriteTo(writer);
        }
    }
}