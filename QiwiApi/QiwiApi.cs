using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QiwiApiSharp.Entities;
using QiwiApiSharp.Enumerations;
using QiwiApiSharp.Exceptions;

namespace QiwiApiSharp
{
    public static class QiwiApi
    {
        private const string API_ENDPOINT = "edge.qiwi.com";

        public static bool Initialized { get; private set; }
        public static bool Authorized { get; private set; }

        private static string _token;
        private static HttpClient _httpClient;

        #region Public Api

        public static void Initialize(string token)
        {
            _token = token;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://" + API_ENDPOINT + "/");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
            Initialized = true;
        }

        public static async Task<AuthResponse> Authorize(bool authInfoEnabled = true, bool contractInfoEnabled = true, bool userInfoEnabled = true)
        {
            var response = await ApiCall("person-profile/v1/profile/current", new Dictionary<string, object>
            {
                {"authInfoEnabled", authInfoEnabled},
                {"contractInfoEnabled", contractInfoEnabled},
                {"userInfoEnabled", userInfoEnabled}
            });

            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<AuthResponse>(await response.Content.ReadAsStringAsync());
                Authorized = true;
                return data;
            }
            switch ((int)response.StatusCode)
            {
                case 401: throw new UnauthorizedException();
                case 423: throw new WalletNotFoundException();
            }

            response.EnsureSuccessStatusCode();
            return null;
        }

        public static async Task<int> MobileProvider(long phoneNumber)
        {
            ValidatePhoneNumber(phoneNumber);
            var response = await _httpClient.PostAsync("https://qiwi.com/mobile/detect.action", new FormUrlEncodedContent(new Dictionary<string, string> { { "phone", "+" + phoneNumber } }));
            response = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<JObject>(responseString);
            int providerId = 0;
            if (int.TryParse(content["message"].Value<string>(), out providerId)) return providerId;
            throw new RequestException(content["message"].Value<string>());
        }

        public static async Task<Provider> CardProvider(long cardNumber)
        {
            ValidateCardNumber(cardNumber);
            var response = await _httpClient.PostAsync("https://qiwi.com/card/detect.action", new FormUrlEncodedContent(new Dictionary<string, string> { { "cardNumber", cardNumber.ToString() } }));
            response = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<JObject>(responseString);
            int providerId = 0;
            if (int.TryParse(content["message"].Value<string>(), out providerId)) return (Provider)providerId;
            throw new RequestException(content["message"].Value<string>());
        }

        public static async Task<PaymentHistoryResponse> PaymentHistory(long walletId, int rows, Operation operation, Source[] sources, DateTime? startDate = null, DateTime? endDate = null, DateTime? nextTxnDate = null, long? nextTxnId = null)
        {
            if (!Initialized) throw new NotInitializedException();
            if(!Authorized) throw new UnauthorizedException();
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

            var response =  await ApiCall("payment-history/v1/persons/" + walletId + "/payments", query);
            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<PaymentHistoryResponse>(await response.Content.ReadAsStringAsync());
                return data;
            }
            switch ((int)response.StatusCode)
            {
                case 401: throw new UnauthorizedException();
                case 404: throw new TransactionNotFoundException();
            }

            response.EnsureSuccessStatusCode();
            return null;
        }

        public static async Task<PaymentStatisticsResponse> PaymentStatistics(long walletId, DateTime startDate, DateTime endDate, Operation operation, Source[] sources)
        {
            var query = new Dictionary<string, object>
            {
                {"startDate", startDate.ToString("s") + "Z"  },
                {"endDate", endDate.ToString("s") + "Z" },
                {"operation", operation.ToString()}
            };
            for (int i = 0; i < sources.Length; i++)
                query.Add("source[" + i + "]", sources[i].ToString());

            var response = await ApiCall("payment-history/v1/persons/" + walletId + "/payments/total", query);
            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<PaymentStatisticsResponse>(await response.Content.ReadAsStringAsync());
                return data;
            }
            switch ((int)response.StatusCode)
            {
                case 401: throw new UnauthorizedException();
                case 404: throw new TransactionNotFoundException();
            }

            response.EnsureSuccessStatusCode();
            return null;
        }

        public static async Task<BalanceResponse> Balance()
        {
            var response = await ApiCall("funding-sources/v1/accounts/current", null);
            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<BalanceResponse>(await response.Content.ReadAsStringAsync());
                return data;
            }
            switch ((int)response.StatusCode)
            {
                case 401: throw new UnauthorizedException();
                case 404: throw new WalletNotFoundException();
            }

            response.EnsureSuccessStatusCode();
            return null;
        }

        public static async Task<CommissionResponse> Comission(long walletId, Provider provider)
        {
            int providerId = 0;
            if (provider == Provider.MobilePhone)
            {
                providerId = await MobileProvider(walletId);
            }
            else providerId = (int)provider;

            var response = await ApiCall("sinap/providers/" + providerId + "/form", null);
            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<CommissionResponse>(await response.Content.ReadAsStringAsync());
                return data;
            }
            response.EnsureSuccessStatusCode();
            return null;
        }

        public static async Task<PaymentResponse> QiwiPayment(Currency currency, double amount, long phoneNumber)
        {
            ValidatePhoneNumber(phoneNumber);
            return await Payment(99, currency, amount, "+" + phoneNumber);
        }

        public static async Task<PaymentResponse> MobilePayment(Currency currency, double amount, long phoneNumber)
        {
            ValidatePhoneNumber(phoneNumber);
            var providerId = await MobileProvider(phoneNumber);
            return await Payment(providerId, currency, amount, phoneNumber.ToString().Substring(1));
        }

        public static async Task<PaymentResponse> CardPayment(Currency currency, double amount, long cardNumber)
        {
            ValidateCardNumber(cardNumber);
            var providerId = await CardProvider(cardNumber);
            return await Payment((int)providerId, currency, amount, cardNumber.ToString());
        }

        public static async Task<PaymentResponse> BankPayment(Currency currency, double amount, long cardNumber, short expDate, short accountType)
        {
            ValidateCardNumber(cardNumber);
            ValidateExpDate(expDate);
            var providerId = await CardProvider(cardNumber);
            ValidateAccountType(providerId, accountType);
            return await Payment((int)providerId, currency, amount, cardNumber.ToString(), expDate, accountType);
        }

        #endregion

        #region Internal Api

        private static async Task<HttpResponseMessage> ApiCall(string request, Dictionary<string, object> query)
        {
            string queryString = null;
            if (query != null && query.Count > 0)
            {
                var queryBuilder = new StringBuilder();
                foreach (var param in query)
                {
                    queryBuilder.Append(param.Key);
                    queryBuilder.Append("=");
                    queryBuilder.Append(param.Value);
                    queryBuilder.Append("&");
                }
                queryString = queryBuilder.ToString();
            }
            queryString = string.IsNullOrEmpty(queryString) ? "" : "?" + queryString;
            return await _httpClient.GetAsync(request + queryString);
        }

        private static async Task<PaymentResponse> Payment(int providerId, Currency currency, double amount, string accountId, short expDate = 0, short accountType = 0)
        {
            var fields = new Dictionary<string, string>();
            fields.Add("account", accountId);
            if (expDate > 0) fields.Add("expDate", expDate.ToString());
            if (accountType > 0) fields.Add("accountType", accountType.ToString());

            var body = new Dictionary<string, object>
            {
                { "id", (int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds },
                { "sum", new CurrencyAmount
                    {
                        amount = amount,
                        currency = currency
                    }
                },
                {"source", "account_643"},
                { "paymentMethod",
                    new {
                        type = "account",
                        accountId = 643
                    }
                },
                { "fields", fields }
            };
            var response = await _httpClient.PostAsync("sinap/terms/" + providerId + "/payments", new StringContent(JsonConvert.SerializeObject(body)));
            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<PaymentResponse>(await response.Content.ReadAsStringAsync());
                return data;
            }
            switch ((int)response.StatusCode)
            {
                case 401: throw new UnauthorizedException();
                case 404: throw new WalletNotFoundException();
            }

            response.EnsureSuccessStatusCode();
            return null;
        }

        #endregion

        #region Validation

        private static void ValidateAccountType(Provider providerId, short accountType)
        {
            switch (providerId)
            {
                case Provider.Tinkoff:
                    if(accountType != 1 && accountType != 3) throw new ArgumentException("Invalid account type date.");
                    break;
                case Provider.AlfaBank:
                    if (accountType != 1 && accountType != 2) throw new ArgumentException("Invalid account type date.");
                    break;
                case Provider.PromsvyazBank:
                    if (accountType != 7 && accountType != 9) throw new ArgumentException("Invalid account type date.");
                    break;
                case Provider.RussianStandard:
                    if (accountType != 1 && accountType != 2 && accountType != 3) throw new ArgumentException("Invalid account type date.");
                    break;
            }
        }

        private static void ValidateExpDate(short expDate)
        {
            if(DigitsCount(expDate) != 4) throw new ArgumentException("Invalid expiration date.");
            var m = expDate / 100;
            var y = expDate % 100;
            if(new DateTime(2000 + y, m, 0) < DateTime.Now.Date) throw new ArgumentException("Invalid expiration date.");
        }

        private static void ValidateCardNumber(long phoneNumber)
        {
            if (DigitsCount(phoneNumber) != 16) throw new ArgumentException("Invalid card number.");
        }

        private static void ValidatePhoneNumber(long phoneNumber)
        {
            if (DigitsCount(phoneNumber) != 11) throw new ArgumentException("Invalid phone number.");
        }

        private static int DigitsCount(long number)
        {
            return (int)(number == 0 ? 1 : Math.Floor(Math.Log10(Math.Abs(number)) + 1));
        }

        #endregion
    }
}