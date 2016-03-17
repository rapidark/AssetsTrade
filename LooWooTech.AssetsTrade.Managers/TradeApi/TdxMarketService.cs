﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LooWooTech.AssetsTrade.Models;

namespace LooWooTech.AssetsTrade.Managers.TradeApi
{
    public class TdxMarketService : IMarketService
    {
        private int _resultCapacity = 0x1000 * 100;
        private int _errorCapacity = 0x100;

        public ApiResult Connect(ApiHost host)
        {
            var data = new StringBuilder(_resultCapacity);
            var error = new StringBuilder(_errorCapacity);
            TdxMarketApi.TdxL2Hq_Connect(host.IPAddress, host.Port, data, error);

            return new ApiResult
            {
                Result = error.Length == 1,
                Data = data.ToString(),
                Error = error.ToString()
            };
        }

        public void Disconnet()
        {
            TdxMarketApi.TdxL2Hq_Disconnect();
        }

        public ApiResult GetMarketInfo(string[] stockCodes)
        {
            var markets = stockCodes.Select(str => (byte)(str.StartsWith("6") ? 1 : 0)).ToArray();
            var data = new StringBuilder(_resultCapacity);
            var error = new StringBuilder(_errorCapacity);
            TdxMarketApi.TdxL2Hq_GetSecurityQuotes10(markets, stockCodes, stockCodes.Length, data, error);

            return new ApiResult
            {
                Result = error.Length == 0,
                Data = data.ToString(),
                Error = error.ToString()
            };
        }
    }
}
