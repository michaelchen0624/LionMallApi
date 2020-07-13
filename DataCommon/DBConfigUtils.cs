using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataCommon
{
    public class DBConfigUtils
    {
        public static IConfiguration Configuration { get; set; }
        static DBConfigUtils()
        {
            Configuration = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = "Config/dbsettings.json", ReloadOnChange = true })
                .Build();
        }
    }
}
