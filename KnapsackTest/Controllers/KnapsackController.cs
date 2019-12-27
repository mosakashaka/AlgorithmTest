using KnapsackTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KnapsackTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KnapsackController
    {
        private readonly ILogger<KnapsackController> _logger;

        public KnapsackController(ILogger<KnapsackController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<KnapsackResponse> Calculate([FromBody]KnapsackRequest req)
        {
            var response = new KnapsackResponse();

            var goods = Flatten(req.Items);
            Stopwatch w = new Stopwatch();
            w.Start();
            _logger.LogInformation("Flattened:{goods}", JsonSerializer.Serialize(goods.Select(p => new
            {
                k = p.k,
                v = p.v
            })));
            (decimal total, string chosen) = KnapsackDo(0, req.QuantityLimit, req.PriceLimit, goods, null);

            _logger.LogInformation("Result:[{}][{}] {}ms", total, chosen, w.ElapsedMilliseconds);
            response.TotalPrice = total;

            var ids = chosen.Split(',');
            response.TotalQuantity = ids.Length;
            response.Items = ids.GroupBy(p => int.Parse(p),
                        p => int.Parse(p),
                        (key, group) => new Item()
                        {
                            Id = key,
                            UnitPrice = goods.First(p => p.k.Equals(key)).v,
                            Quantity = group.Count()
                        }
            ).ToList();

            return await Task.FromResult(response);
        }

        private IList<(int k, decimal v)> Flatten(IList<Item> items)
        {
            var orderedItems = items.OrderByDescending(p => p.UnitPrice);
            IList<(int k, decimal v)> flattened = new List<(int k, decimal v)>();
            foreach (var item in orderedItems)
            {
                for (var i = 0; i < item.Quantity; i++)
                {
                    flattened.Add((item.Id, item.UnitPrice));
                }
            }
            return flattened;
        }

        private (decimal price, string chosen) KnapsackDo(int step, int quantityLimit,
            decimal priceRemain,
            IList<(int k, decimal v)> goods,
            IDictionary<string, (decimal price, string chosen)> cached)
        {
            if (null == cached)
            {
                cached = new Dictionary<string, (decimal price, string chosen)>();
            }
            var prefix = new string(' ', step * 2);
            var key = $"{step}-{quantityLimit}-{priceRemain}";
            _logger.LogInformation("{pref}Calculate:[{key}]", prefix, key);
            if (cached.ContainsKey(key))
            {
                return cached[key];
            }

            if ((step >= goods.Count) || (quantityLimit == 0) || (priceRemain <= 0))
            {
                return (0, "");
            }

            (decimal priceWo, string chosenWo) = KnapsackDo(step + 1, quantityLimit, priceRemain, goods, cached);
            if (goods[step].v <= priceRemain)
            {
                (decimal priceW, string chosenW) = KnapsackDo(step + 1, quantityLimit - 1,
                    priceRemain - goods[step].v,
                    goods, cached);
                priceW += goods[step].v;

                if (priceW >= priceWo)
                {
                    chosenW = goods[step].k + (string.IsNullOrEmpty(chosenW) ? "" : ("," + chosenW));
                    _logger.LogInformation("{pref}Try take: [{}] at step [{}]", prefix, goods[step].k, step);
                    cached.Add(key, (priceW, chosenW));
                    return (priceW, chosenW);
                }
                else
                {
                    cached.Add(key, (priceWo, chosenWo));
                    return (priceWo, chosenWo);
                }
            }
            else
            {
                cached.Add(key, (priceWo, chosenWo));
                return (priceWo, chosenWo);
            }
        }
    }
}