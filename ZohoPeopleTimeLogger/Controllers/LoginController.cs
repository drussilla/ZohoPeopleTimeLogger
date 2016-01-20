using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ZohoPeopleClient;
using ZohoPeopleClient.Exceptions;
using ZohoPeopleTimeLogger.Model;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.Controllers
{
    public class LoginController : ILoginController
    {
        private readonly IDialogService dialogService;
        private readonly IDateTimeService dateTimeService;
        private readonly IZohoClient zohoClient;

        private string previouslyTypedLogin;

        public LoginController(IZohoClient zohoClient, IDialogService dialogService, IDateTimeService dateTimeService)
        {
            this.zohoClient = zohoClient;
            this.dialogService = dialogService;
            this.dateTimeService = dateTimeService;
        }

        public async Task<AuthenticationData> LoginWithPassword()
        {
            var loginDetails = await dialogService.ShowLogin(previouslyTypedLogin);

            // cancel was pressed
            if (loginDetails == null)
            {
                previouslyTypedLogin = null;
                return null;
            }

            previouslyTypedLogin = loginDetails.Username;

            var progress = await dialogService.ShowProgress("Authentication", "Wait for response from server");
            progress.SetIndeterminate();

            var isError = false;
            var errorMessage = "";
            var token = "";
            try
            {
                token = await zohoClient.LoginAsync(loginDetails.Username, loginDetails.Password);
            }
            catch (ApiLoginErrorException exception)
            {
                isError = true;
                errorMessage = exception.Response;
            }

            var id = string.Empty;
            if (!isError)
            {
                try
                {
                    id = await GetEmployeeId(loginDetails.Username);
                }
                catch (Exception exception)
                {
                    isError = true;
                    errorMessage = exception.Message;
                }
            }

            await progress.CloseAsync();

            if (isError)
            {
                await dialogService.ShowMessageAsync("Authentication error", errorMessage);
                return null;
            }

            return new AuthenticationData {UserName = loginDetails.Username, Token = token, Id = id};
        }

        public async Task<bool> LoginWithToken(AuthenticationData authData)
        {
            zohoClient.Login(authData.Token);

            var progress = await dialogService.ShowProgress("Authentication", "Wait for response from server");
            progress.SetIndeterminate();

            var isSuccess = true;
            try
            {
                await
                    zohoClient.TimeTracker.TimeLog.GetAsync(authData.Id, dateTimeService.Now, dateTimeService.Now);
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.ProtocolError ||
                    ex.Response == null)
                {
                    throw;
                }

                var httpResponse = ex.Response as HttpWebResponse;
                if (httpResponse == null || httpResponse.StatusCode != HttpStatusCode.BadRequest)
                {
                    throw;
                }
                
                isSuccess = false;
            }

            await progress.CloseAsync();

            return isSuccess;
        }

        public async Task<string> GetEmployeeId(string username)
        {
            var employees = await zohoClient.FetchRecord.GetByViewAsync("P_EmployeeView");

            var employeeRecord =
                employees.FirstOrDefault(
                    x => ((string) x["Email ID"]).Equals(username, StringComparison.OrdinalIgnoreCase));
            if (employeeRecord == null)
            {
                throw new Exception("Cannot find user Id. User name: " + username);
            }

            return employeeRecord["EmployeeID"];
        }
    }
}