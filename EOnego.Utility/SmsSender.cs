using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace EOnego.Utility
{
   
    public class SmsSender
    {
        private TwilioSettings _twilioSettings { get; }
        public SmsSender(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
        }
        public void SendSms(string toPhoneNumber, string message)
        {
            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
            var messageOptions = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
            {
                From = new PhoneNumber(_twilioSettings.PhoneNumber),
                Body = message
            };
            MessageResource.Create(messageOptions);
        }

        // for phone call
        public void VoiceCalling(string toPhoneNumber)
        {
            var call = CallResource.Create(
                to: new PhoneNumber(toPhoneNumber),
                from: new PhoneNumber(_twilioSettings.PhoneNumber),
                url: new Uri(_twilioSettings.ApplicationUrl)
                );
        }
    }
}
