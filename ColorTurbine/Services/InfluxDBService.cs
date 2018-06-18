using System;
using System.Threading.Tasks;
using ColorTurbine;
using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb;

public class InfluxDBService
{
    InfluxDbClient client;
    string database;
    public InfluxDBService()
    {
        var config = Services.Configuration.GetServiceConfiguration("influxDB");
        string url = config["url"];
        string user = config["user"];
        string password = config["password"];
        database = config["database"];

        client = new InfluxDbClient(url, user, password, InfluxDbVersion.Latest);
    }

    public Task WriteAsync(InfluxData.Net.InfluxDb.Models.Point pt)
    {
        return client.Client.WriteAsync(pt, database);
    }
}