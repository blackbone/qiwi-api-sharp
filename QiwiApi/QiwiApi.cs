using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QiwiApiSharp.Enumerations;

namespace QiwiApiSharp
{
    public static class QiwiApi
    {
        public const string API_ENDPOINT = "edge.qiwi.com";

        public static bool Initialized { get; private set; }
        
        private static string _token;
        private static HttpClient _httpClient;

        public static void Initialize(string token)
        {
            _token = token;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
            Initialized = true;
        }

        private static async Task<T> ApiCall<T>(string request, Dictionary<string, object> args)
        {
            var query = new StringBuilder();
            foreach (var param in args)
            {
                query.Append(param.Key);
                query.Append("=");
                query.Append(param.Value);
                query.Append("&");
            }
            Console.WriteLine("https://" + API_ENDPOINT + "/" + request + "?" + query);
            var responce = await _httpClient.GetStringAsync("https://" + API_ENDPOINT + "/" + request + "?" + query);
            return JsonConvert.DeserializeObject<T>(responce);
        }

        #region Public Api

        public static async Task<AuthResponse> Authorize(bool authInfoEnabled = true, bool contractInfoEnabled = true, bool userInfoEnabled = true)
        {
            return await ApiCall<AuthResponse>("person-profile/v1/profile/current", new Dictionary<string, object>
            {
                {"authInfoEnabled", authInfoEnabled},
                {"contractInfoEnabled", contractInfoEnabled},
                {"userInfoEnabled", userInfoEnabled}
            });
        }

        public static async Task<PaymentHistoryResponse> PaymentHistory(long personId, int rows, Operation operation, Source[] sources, DateTime? startDate = null, DateTime? endDate = null, DateTime? nextTxnDate = null, long? nextTxnId = null)
        {
            var query = new Dictionary<string, object>
            {
                {"rows", rows },
                {"operation", operation.ToString() }
            };

            for (int i = 0; i < sources.Length; i++)
                query.Add("source[" + i + "]", sources[i].ToString());

            if (startDate != null && endDate != null)
            {
                query.Add("startDate", startDate.Value.ToString("s") + "Z");
                query.Add("endDate", endDate.Value.ToString("s") + "Z");
            }

            if(nextTxnDate != null && nextTxnId != null)
            {
                query.Add("nextTxnDate", nextTxnDate.Value.ToString("s") + "Z");
                query.Add("nextTxnId", nextTxnId);
            }
            return await ApiCall<PaymentHistoryResponse>("payment-history/v1/persons/" + personId + "/payments", query);
        }

        public static async Task<PaymentStatisticsResponse> PaymentStatistics(long personId, DateTime startDate,
            DateTime endDate, Operation operation, Source[] sources)
        {
            var query = new Dictionary<string, object>
            {
                {"startDate", startDate.ToString("s") + "Z"  },
                {"endDate", endDate.ToString("s") + "Z" },
                {"operation", operation.ToString()}
            };
            for (int i = 0; i < sources.Length; i++)
                query.Add("source[" + i + "]", sources[i].ToString());

            return await ApiCall<PaymentStatisticsResponse>("payment-history/v1/persons/" + personId + "/payments/total", query);
        }

        #endregion
    }
}