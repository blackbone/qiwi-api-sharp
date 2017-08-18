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

        private static string _token;
        private static HttpClient _httpClient;

        #region Public Api

        /// <summary>
        ///     Initializes api with <see cref="token"/> passed.
        /// </summary>
        /// <param name="token"> Api token obtained from <see href="https://qiwi.com/api"/> for instructions. </param>
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

        /// <summary>
        ///     Identifies mobile provider by phone number.
        /// </summary>
        /// <param name="phoneNumber"> Mobile phone number - 11 digits. </param>
        /// <returns> Mobile operator identifier. </returns>
        /// <exception cref="ArgumentException"> If invalid number passed. </exception>
        /// <exception cref="RequestException"> If request were not processed. </exception>
        public static async Task<int> MobileProviderAsync(string phoneNumber)
        {
            if (!ValidatePhoneNumber(phoneNumber)) throw new ArgumentException("Invalid phone number.");
            var response = await _httpClient.PostAsync("https://qiwi.com/mobile/detect.action", new FormUrlEncodedContent(new Dictionary<string, string> { { "phone", "+" + phoneNumber } }));
            response = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<JObject>(responseString);
            int providerId = 0;
            if (int.TryParse(content["message"].Value<string>(), out providerId)) return providerId;
            throw new RequestException(content["message"].Value<string>());
        }

        /// <summary>
        ///     Identifies card emitter by card number.
        /// </summary>
        /// <param name="cardNumber"> Mobile phone number - 16 digits. </param>
        /// <returns> Card emitter identifier. </returns>
        /// <exception cref="ArgumentException"> If invalid number passed. </exception>
        /// <exception cref="RequestException"> If request were not processed. </exception>
        public static async Task<Provider> CardProviderAsync(string cardNumber)
        {
            if(!ValidateCardNumber(cardNumber)) throw new ArgumentException("Invalid card number.");
            var response = await _httpClient.PostAsync("https://qiwi.com/card/detect.action", new FormUrlEncodedContent(new Dictionary<string, string> { { "cardNumber", cardNumber.ToString() } }));
            response = response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<JObject>(responseString);
            int providerId = 0;
            if (int.TryParse(content["message"].Value<string>(), out providerId)) return (Provider)providerId;
            throw new RequestException(content["message"].Value<string>());
        }

        /// <summary>
        ///     Requests user profile.
        /// </summary>
        /// <param name="authInfoEnabled"> Is authorization info will be included in response. </param>
        /// <param name="contractInfoEnabled"> Is contract info will be included in response. </param>
        /// <param name="userInfoEnabled"> Is user info will be included in response. </param>
        /// <returns> UserProfileResponse object. See <see cref="UserProfileResponse"/> for details. </returns>
        /// <exception cref="NotInitializedException"> If Qiwi Api were not initalized calling QiwiApi.Initialize(*token*).</exception>
        /// <exception cref="UnauthorizedException"> If token is invalid or were expired.</exception>
        /// <exception cref="WalletNotFoundException"> If wallet were not found.</exception>
        public static async Task<UserProfileResponse> UserProfileAsync(bool authInfoEnabled = true, bool contractInfoEnabled = true, bool userInfoEnabled = true)
        {
            if (!Initialized) throw new NotInitializedException();
            var response = await ApiCallAsync("person-profile/v1/profile/current", new Dictionary<string, object>
            {
                {"authInfoEnabled", authInfoEnabled},
                {"contractInfoEnabled", contractInfoEnabled},
                {"userInfoEnabled", userInfoEnabled}
            });

            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<UserProfileResponse>(await response.Content.ReadAsStringAsync());
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

        /// <summary>
        ///     Requests payment history. 
        /// </summary>
        /// <param name="walletId"> Id of wallet for history request. </param>
        /// <param name="rows"> Rows count to request. </param>
        /// <param name="operation"> Wallet operation to include in response. See <see cref="Operation"/> for possible variaties. </param>
        /// <param name="sources"> Wallet sources to include in response. See <see cref="Source"/> for possible variaties. </param>
        /// <param name="startDate"> Start date to request history. Must be specified with <see cref="endDate"/>. </param>
        /// <param name="endDate"> End date to request history. Must be specified with <see cref="startDate"/>. </param>
        /// <param name="nextTxnDate"> Txn date to request from previous list. See <see cref="PaymentHistoryResponse.nextTxnDate"/> in <see cref="PaymentHistoryResponse"/>. Must be specified with <see cref="nextTxnId"/>. </param>
        /// <param name="nextTxnId">Txn previous id to request from previous list. See <see cref="PaymentHistoryResponse.nextTxnId"/> in <see cref="PaymentHistoryResponse"/>. Must be specified with <see cref="nextTxnDate"/>.</param>
        /// <returns> PaymentHistoryResponse object. See <see cref="PaymentHistoryResponse"/> for details. </returns>
        /// <exception cref="NotInitializedException"> If Qiwi Api were not initalized calling QiwiApi.Initialize(*token*).</exception>
        /// <exception cref="ArgumentException"> If wallet id is not a phone number format. </exception>
        /// <exception cref="UnauthorizedException"> If token is invalid or were expired.</exception>
        /// <exception cref="TransactionNotFoundException"> If transaction is missing or no transactions can be found with spocified signs. </exception>
        public static async Task<PaymentHistoryResponse> PaymentHistoryAsync(string walletId, int rows, Operation operation, Source[] sources, DateTime? startDate = null, DateTime? endDate = null, DateTime? nextTxnDate = null, long? nextTxnId = null)
        {
            if (!Initialized) throw new NotInitializedException();
            if (!ValidatePhoneNumber(walletId)) throw new ArgumentException("Invalid wallet id.");
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

            var response =  await ApiCallAsync("payment-history/v1/persons/" + walletId + "/payments", query);
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

        /// <summary>
        ///     Requests payments statistics for period.
        /// </summary>
        /// <param name="walletId"> Id of wallet for history request. </param>
        /// <param name="startDate"> Start date to request history. Must be specified with <see cref="endDate"/>. </param>
        /// <param name="endDate"> End date to request history. Must be specified with <see cref="startDate"/>. </param>
        /// <param name="operation"> Wallet operation to include in response. See <see cref="Operation"/> for possible variaties. </param>
        /// <param name="sources"> Wallet sources to include in response. See <see cref="Source"/> for possible variaties. </param>
        /// <returns> PaymentStatisticsResponse object. See <see cref="PaymentStatisticsResponse"/> for details.</returns>
        /// <exception cref="NotInitializedException"> If Qiwi Api were not initalized calling QiwiApi.Initialize(*token*).</exception>
        /// <exception cref="ArgumentException"> If wallet id is not a phone number format. </exception>
        /// <exception cref="UnauthorizedException"> If token is invalid or were expired.</exception>        
        /// <exception cref="TransactionNotFoundException"> If transaction is missing or no transactions can be found with spocified signs. </exception>
        public static async Task<PaymentStatisticsResponse> PaymentStatisticsAsync(string walletId, DateTime startDate, DateTime endDate, Operation operation, Source[] sources)
        {
            if (!Initialized) throw new NotInitializedException();
            if (!ValidatePhoneNumber(walletId)) throw new ArgumentException("Invalid wallet id.");
            var query = new Dictionary<string, object>
            {
                {"startDate", startDate.ToString("s") + "Z"  },
                {"endDate", endDate.ToString("s") + "Z" },
                {"operation", operation.ToString()}
            };
            for (int i = 0; i < sources.Length; i++)
                query.Add("source[" + i + "]", sources[i].ToString());

            var response = await ApiCallAsync("payment-history/v1/persons/" + walletId + "/payments/total", query);
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

        /// <summary>
        ///     Requests balance for current user.
        /// </summary>
        /// <returns> BalanceResponse object. See <see cref="BalanceResponse"/> for details.</returns>
        /// <exception cref="NotInitializedException"> If Qiwi Api were not initalized calling QiwiApi.Initialize(*token*).</exception>
        /// <exception cref="UnauthorizedException"> If token is invalid or were expired.</exception>
        /// <exception cref="WalletNotFoundException"> If wallet were not found.</exception>
        public static async Task<BalanceResponse> BalanceAsync()
        {
            if (!Initialized) throw new NotInitializedException();
            var response = await ApiCallAsync("funding-sources/v1/accounts/current", null);
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

        /// <summary>
        ///     Requests comission for payment provider.
        /// </summary>
        /// <param name="provider"> Provider id. See <see cref="Provider"/> for ids, or <see cref="MobileProvider"/> or <see cref="CardProvider"/> to get provider id from phone or card number.</param>
        /// <returns> CommissionResponse object. See <see cref="CommissionResponse"/> for details.</returns>
        /// <exception cref="NotInitializedException"> If Qiwi Api were not initalized calling QiwiApi.Initialize(*token*).</exception>
        public static async Task<CommissionResponse> ComissionAsync(int provider)
        {
            if (!Initialized) throw new NotInitializedException();
            var response = await ApiCallAsync("sinap/providers/" + provider + "/form", null);
            if (response.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<CommissionResponse>(await response.Content.ReadAsStringAsync());
                return data;
            }
            response.EnsureSuccessStatusCode();
            return null;
        }

        /// <summary>
        ///     Makes payment to other qiwi wallet.
        /// </summary>
        /// <param name="currency"> <see cref="Currency"/> of payment. </param>
        /// <param name="amount"> Funds amount to pay. </param>
        /// <param name="phoneNumber"> Phone number of recipient. </param>
        /// <returns> PaymentResponse object. See <see cref="PaymentResponse"/> for details.</returns>
        /// <exception cref="NotInitializedException"> If Qiwi Api were not initalized calling QiwiApi.Initialize(*token*).</exception>
        /// <exception cref="ArgumentException"> If phone number  of recipient is invalid. </exception>
        /// <exception cref="UnauthorizedException"> If token is invalid or were expired.</exception>
        /// <exception cref="WalletNotFoundException"> If wallet were not found.</exception>
        public static async Task<PaymentResponse> QiwiPaymentAsync(Currency currency, double amount, string phoneNumber)
        {
            if (!Initialized) throw new NotInitializedException();
            if(!ValidatePhoneNumber(phoneNumber)) throw new ArgumentException("Invalid phone number.");
            return await PaymentAsync(99, currency, amount, "+" + phoneNumber);
        }

        /// <summary>
        ///     Makes moblie payment.
        /// </summary>
        /// <param name="currency"> <see cref="Currency"/> of payment. </param>
        /// <param name="amount"> Funds amount to pay. </param>
        /// <param name="phoneNumber"> Phone number of to send funds. </param>
        /// <returns> PaymentResponse object. See <see cref="PaymentResponse"/> for details.</returns>
        /// <exception cref="NotInitializedException"> If Qiwi Api were not initalized calling QiwiApi.Initialize(*token*).</exception>
        /// <exception cref="ArgumentException"> If phone number is invalid. </exception>
        /// <exception cref="UnauthorizedException"> If token is invalid or were expired.</exception>
        /// <exception cref="WalletNotFoundException"> If wallet were not found.</exception>
        public static async Task<PaymentResponse> MobilePaymentAsync(Currency currency, double amount, string phoneNumber)
        {
            if (!Initialized) throw new NotInitializedException();
            if (!ValidatePhoneNumber(phoneNumber)) throw new ArgumentException("Invalid phone number.");
            var providerId = await MobileProviderAsync(phoneNumber);
            return await PaymentAsync(providerId, currency, amount, phoneNumber.ToString().Substring(1));
        }

        /// <summary>
        ///     Makes card payment.
        /// </summary>
        /// <param name="currency"> <see cref="Currency"/> of payment. </param>
        /// <param name="amount"> Funds amount to pay. </param>
        /// <param name="cardNumber"> Card number of recipient. </param>
        /// <returns> PaymentResponse object. See <see cref="PaymentResponse"/> for details.</returns>
        /// <exception cref="NotInitializedException"> If Qiwi Api were not initalized calling QiwiApi.Initialize(*token*).</exception>
        /// <exception cref="ArgumentException"> If phone number is invalid. </exception>
        /// <exception cref="UnauthorizedException"> If token is invalid or were expired.</exception>
        /// <exception cref="WalletNotFoundException"> If wallet were not found.</exception>
        public static async Task<PaymentResponse> CardPayment(Currency currency, double amount, string cardNumber)
        {
            if (!Initialized) throw new NotInitializedException();
            if(!ValidateCardNumber(cardNumber)) throw new ArgumentException("Invalid card number.");
            var providerId = await CardProviderAsync(cardNumber);
            return await PaymentAsync((int)providerId, currency, amount, cardNumber);
        }

        /// <summary>
        ///     Makes bank payment.
        /// </summary>
        /// <param name="currency"> <see cref="Currency"/> of payment. </param>
        /// <param name="amount"> Funds amount to pay. </param>
        /// <param name="cardOrAccountNumber"> Card number of recipient. </param>
        /// <param name="expDate"> Card expiration date in 4 digits format. </param>
        /// <param name="accountType"> Bank account type.
        /// (1 - card, 3 - contract) for <see cref="Provider.Tinkoff"/>.
        /// (1 - card, 2 - account) for <see cref="Provider.AlfaBank"/>.
        /// (7 - card, 9 - account) for <see cref="Provider.PromsvyazBank"/>.
        /// (1 - card, 2 - account, 3 - contract) for <see cref="Provider.RussianStandard"/>.
        /// </param>
        /// <returns> PaymentResponse object. See <see cref="PaymentResponse"/> for details.</returns>
        /// <exception cref="NotInitializedException"> If Qiwi Api were not initalized calling QiwiApi.Initialize(*token*).</exception>
        /// <exception cref="ArgumentException"> If card or bank account number is invalid. </exception>
        /// <exception cref="UnauthorizedException"> If token is invalid or were expired.</exception>
        /// <exception cref="WalletNotFoundException"> If wallet were not found.</exception>
        public static async Task<PaymentResponse> BankPaymentAsync(Currency currency, double amount, string cardOrAccountNumber, short expDate, short accountType)
        {
            if (!Initialized) throw new NotInitializedException();
            if(!ValidateCardNumber(cardOrAccountNumber) && !ValidateBankAccountNumber(cardOrAccountNumber)) throw new ArgumentException("Card or bank account number is invalid.");
            if(!ValidateExpDate(expDate)) throw new ArgumentException("Invalid expiration date.");
            var providerId = await CardProviderAsync(cardOrAccountNumber);
            if(!ValidateAccountType(providerId, accountType)) throw new ArgumentException("Invalid account type date.");
            return await PaymentAsync((int)providerId, currency, amount, cardOrAccountNumber, expDate, accountType);
        }

        #endregion

        #region Internal Api

        private static async Task<HttpResponseMessage> ApiCallAsync(string request, Dictionary<string, object> query)
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

        private static async Task<PaymentResponse> PaymentAsync(int providerId, Currency currency, double amount, string accountId, short expDate = 0, short accountType = 0)
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

        private static bool ValidateAccountType(Provider providerId, short accountType)
        {
            switch (providerId)
            {
                case Provider.Tinkoff:
                    if (accountType == 1 || accountType == 3) return true;
                    break;
                case Provider.AlfaBank:
                    if (accountType == 1 || accountType == 2) return true;
                    break;
                case Provider.PromsvyazBank:
                    if (accountType == 7 || accountType == 9) return true;
                    break;
                case Provider.RussianStandard:
                    if (accountType == 1 || accountType == 2 || accountType == 3) return true;
                    break;
            }
            return false;
        }

        private static bool ValidateExpDate(short expDate)
        {
            if(DigitsCount(expDate) != 4) throw new ArgumentException("Invalid expiration date.");
            var m = expDate / 100;
            var y = expDate % 100;
            return new DateTime(2000 + y, m, 0) > DateTime.Now.Date;
        }

        private static bool ValidateBankAccountNumber(string bankAccountNumber)
        {
            return bankAccountNumber.Length == 16;
        }

        private static bool ValidateCardNumber(string cardNumber)
        {
            return cardNumber.Length == 16;
        }

        private static bool ValidatePhoneNumber(string phoneNumber)
        {
            return phoneNumber.Length == 11;
        }

        private static long DigitsCount(long n)
        {
            return (long) (n == 0 ? 1 : Math.Floor(Math.Log10(Math.Abs(n)) + 1));
        }
        #endregion
    }
}