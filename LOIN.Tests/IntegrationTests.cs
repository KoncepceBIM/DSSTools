﻿using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LOIN.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        private readonly WebApplicationFactory<Server.Program> application;

        public IntegrationTests()
        {
            application = new WebApplicationFactory<Server.Program>()
                .WithWebHostBuilder(builder =>
                {
                    // ... Configure test services
                });
        }

        [TestMethod]
        public async Task Should_read_data()
        {
            using var client = CreateClient();

            var result = await client.GetAsync("/api/SFDI_Koordinace/breakdown/requirement-sets?groupingType=CS&actors=&reasons=&breakdown=20737,20778,20819,20860,20906,20947,21000,21041,21082,21123,21169,21210,21251,21292,21333,21374,21415,21456,21497,21538,21579,21620,21666,21707,21748,21789,21830,21871,21912,21953,21999,22040,22081,22122,22163,22204,22245,22286,22327,22368,22409,22450,22491,22532,22573,22614,22655,22696,22737,22778,22819,22860,22901,22942,22983,23024,23070,23111,23157,23198,23244,23285,23326,23367,23408,23449,23490,23536,23589,23630,23671,23712,23753,23799,23840,23881,23922,23963,24009,24050,24091,24132,24173,24214,24255,24296,24337,24378,24419,24460,24501,24542,24583,24629,24670,24711,24752,24793,24834,24875,24916,24962,25003,25044,25085,25126,25167,25208,25249,25295,25341,25382,25423,25464,25505,25546,25587,25628,25674,25715,25756,25802,25843,25884,25925,25966,26007,26048,26089,26130,26171,26212,26253,26294,26335,26376,26417,26458,26499,26540,26581,26622,26663,26704,26745,26786,26827,26873,26914,26955,26996,27042,27083,27124,27170,27211,27252,27293,27334,27380,27421,27467,27520,27561,27602,27643,27684,27730,27771,27812,27858,27904,27945,27986,28027,28068,28109,28155,28196,28237,28278,28319,28360,28406,28459,28500,28541,28582,28623,28669,28710,28751,28792,28833,28879,28920,28966,29019,29060,29101,29142,29183,29229,29270,29311,29352,29393,29434,29480,29521,29567,29620,29661,29702,29748,29794,29835,29876,29922,29963,30004,30045,30086,30127,30173,30214,30255,30296,30337,30383,30424,30465,30506,30547,30588,30629,30670,30711,30752,30793,30834,30880,30921,30962,31003,31044,31085,31131,31172,31213,31259,31305,31346,31387,31428,31469,31510,31556,31597,31643,31684,31725,31766,31807,31853,31899&milestones=");

            result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Should_expand_context_for_requirement_sets()
        { 
            using var client = CreateClient();

            var result = await client.GetAsync("/api/SFDI_Koordinace/requirement-sets?expandContext=true&breakdown=20737");

            result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var data = await result.Content.ReadAsStringAsync();
        }

        [TestMethod]
        public async Task Should_expand_context_for_requirements()
        {
            using var client = CreateClient();

            var result = await client.GetAsync("/api/SFDI_Koordinace/requirements?expandContext=true&breakdown=20737");

            result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var data = await result.Content.ReadAsStringAsync();
        }

        private HttpClient CreateClient()
        {
            var client = application.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            return client;
        }

    }
}
