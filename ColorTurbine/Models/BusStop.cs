// Generated from https://app.quicktype.io/#l=cs&r=json2csharp

namespace QuickType
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;

    public partial class BusDeparture : IComparable
    {
        [JsonProperty("Actual")]
        public bool Actual { get; set; }

        [JsonProperty("BlockNumber")]
        public long BlockNumber { get; set; }

        [JsonProperty("DepartureText")]
        public string DepartureText { get; set; }

        [JsonProperty("DepartureTime")]
        public DateTime DepartureTime { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Gate")]
        public string Gate { get; set; }

        [JsonProperty("Route")]
        public int Route { get; set; }

        [JsonProperty("RouteDirection")]
        public string RouteDirection { get; set; }

        [JsonProperty("Terminal")]
        public string Terminal { get; set; }

        [JsonProperty("VehicleHeading")]
        public long VehicleHeading { get; set; }

        [JsonProperty("VehicleLatitude")]
        public double VehicleLatitude { get; set; }

        [JsonProperty("VehicleLongitude")]
        public double VehicleLongitude { get; set; }

        public BusStop BusStop { get; set; }

        public int CompareTo(object obj)
        {
            var other = obj as BusDeparture;
            if (other == null)
            {
                return 1;
            }

            if (this == other)
            {
                return 0;
            }

            return (DepartureTime + GetRelativeTimeCost()).CompareTo(other.DepartureTime + other.GetRelativeTimeCost());
        }

        public override string ToString()
        {
            var sch = Actual ? "" : "scheduled";
            return $"{Route}{Terminal} {DepartureText} {sch} {Description} {RouteDirection}";
        }

        public TimeSpan GetRelativeTimeCost()
        {
            if (BusStop == null)
            {
                return TimeSpan.Zero;
            }

            // This should really be boiled down to some sort of distance algorithm

            var matchingRoutes = from route in BusStop.Routes
                                 where route.RouteCode == Route
                                 where route.Terminal == Terminal
                                 select route;

            if (matchingRoutes == null || matchingRoutes.Count() == 0)
            {
                matchingRoutes = from route in BusStop.Routes
                                 where route.RouteCode == Route
                                 where route.Terminal == null
                                 select route;
            }

            if (matchingRoutes.Count() > 1)
            {
                throw new ArgumentException("Ambiguous routes");
            }

            if (matchingRoutes.Count() == 0)
            {
                return TimeSpan.Zero;
            }

            return matchingRoutes.First().RelativeTimeCost;
        }
    }

    public partial class BusDeparture
    {
        public static BusDeparture[] FromJson(string json) => JsonConvert.DeserializeObject<BusDeparture[]>(json, QuickType.Converter.Settings);
    }

    internal class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.DateTimeOffset,
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };
    }

    class MinuteTimespanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var val = (TimeSpan)value;
            var jt = JToken.FromObject(val.TotalMinutes, serializer);
            jt.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jt = JToken.Load(reader);
            return TimeSpan.FromMinutes(jt.ToObject<double>());
        }
    }

    public class BusRoute
    {
        public int RouteCode { get; set; }
        public string Terminal { get; set; }
        [JsonConverter(typeof(MinuteTimespanConverter))]
        public TimeSpan RelativeTimeCost { get; set; }
    }

    public class BusStop
    {
        public int StopId { get; set; }

        [JsonConverter(typeof(MinuteTimespanConverter))]
        public TimeSpan WalkTime { get; set; }
        public BusRoute[] Routes { get; set; }
    }
}