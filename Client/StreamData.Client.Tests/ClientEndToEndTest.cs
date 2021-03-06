﻿using System.Configuration;
using System.Threading;
using NFluent;
using Streamdata.Client.Tests.Data;
using Xunit;

namespace Streamdata.Client.Tests
{
    public class ClientEndToEndTest
    {
        private string secretKey = null;
        public ClientEndToEndTest()
        {
            secretKey = ConfigurationManager.AppSettings["streamdata:secretkey"];
            if(string.IsNullOrEmpty(secretKey))
                throw new ConfigurationErrorsException(
                    "you must fill the key [streamdata:secretkey] in the App.Config with your personal StreamData secret key to test the service end to end");
        }

        [Fact]
        public void
        WHEN_client_starts_THEN_it_should_order_engine_to_start()
        {
            var client = StreamdataClient<StockMarketOrders>.WithDefaultConfiguration();
            StockMarketOrders orders = null;
            int counter = 0;
            var testApiUrl = "http://stockmarket.streamdata.io/prices";
            client.OnData(o=> orders = o);
            client.OnPatch(p=>counter++);
            client.Start(testApiUrl);
            var expectedUrl = $"{StreamdataOfficialUrls.PRODUCTION}/{testApiUrl}?X-Sd-Token={client.Configuration.SecretKey}";
            Check.That(client.ListenUrl).IsEqualTo(expectedUrl);
            //we wait 10s, after this time, we should already
            //have some first data and at least, one update
            Thread.Sleep(10*1000);
            Check.That(orders).IsNotNull();
            Check.That(counter).IsGreaterThan(0);
        }
    }
}
