using System;
using System.Text.Json.Serialization;

namespace webapi
{
    public class WeatherForecast
    {
        [JsonPropertyName("quote")]
        public string Quote { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("temperatureC")]
        public int TemperatureC { get; set; }

        [JsonPropertyName("temperatureF")]
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        [JsonPropertyName("summary")]
        public string Summary { get; set; }
    }
}
