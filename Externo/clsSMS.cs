using System;
using Amazon;
using Amazon.SimpleNotificationService.Model;
namespace apiBillFold.Externo
{
	public class clsSMS
	{
        private static readonly string awsKeyId = ""; //Access Key for the user.
        private static readonly string awsKeySecret = ""; //Secret Access Key for the user.
        private static readonly string messageType = "Transactional";


        // The sender ID to use when sending the message. Support for sender ID
        // varies by country or region. For more information, see
        // https://docs.aws.amazon.com/pinpoint/latest/userguide/channels-sms-countries.html
        private static readonly string senderId = "TiendasMass";

#pragma warning disable CS1998 // El método asincrónico carece de operadores "await" y se ejecutará de forma sincrónica. Puede usar el operador 'await' para esperar llamadas API que no sean de bloqueo o 'await Task.Run(...)' para hacer tareas enlazadas a la CPU en un subproceso en segundo plano.
        public static async Task Enviar(String pais, String Number, String strMensaje)
#pragma warning restore CS1998 // El método asincrónico carece de operadores "await" y se ejecutará de forma sincrónica. Puede usar el operador 'await' para esperar llamadas API que no sean de bloqueo o 'await Task.Run(...)' para hacer tareas enlazadas a la CPU en un subproceso en segundo plano.
        {
            try
            {
                String numeroFinal = $"+" + pais + Number;
                var snsService = new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceClient(awsKeyId, awsKeySecret, RegionEndpoint.USEast2);
                var attributes = new Dictionary<string, MessageAttributeValue>();
                attributes.Add("AWS.SNS.SMS.SenderID", new MessageAttributeValue() { StringValue = senderId, DataType = "String" });
                attributes.Add("AWS.SNS.SMS.SMSType", new MessageAttributeValue() { StringValue = messageType, DataType = "String" });
                var request = new Amazon.SimpleNotificationService.Model.PublishRequest();
                request.PhoneNumber = numeroFinal;
                request.Message = strMensaje;
                request.MessageAttributes = attributes;
                var result = snsService.PublishAsync(request).Result;
                if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    Console.Write("");
                else
                {


                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message + "" + e.StackTrace);

            }
        }
    }
}

