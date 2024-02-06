using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using RestSharp;
using Method = RestSharp.Method;

namespace apiBillFold.Externo
{
	
        public class seguridad
        {
            static public string appPwdUnique = "MiClaveUnicaParaEncriptar";
            private byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
            {
                MemoryStream ms = new MemoryStream();
                Rijndael alg = Rijndael.Create();
                alg.Key = Key;
                alg.IV = IV;
                CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(clearData, 0, clearData.Length);
                cs.Close();
                byte[] encryptedData = ms.ToArray();
                return encryptedData;
            }
            public string Encrypt(string Data, string Password, int Bits)
            {
                byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(Data);
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, new byte[] { 0x0, 0x1, 0x2, 0x1C, 0x1D, 0x1E, 0x3, 0x4, 0x5, 0xF, 0x20, 0x21, 0xAD, 0xAF, 0xA4 });
                if (Bits == 128)
                {
                    byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(16), pdb.GetBytes(16));
                    return Convert.ToBase64String(encryptedData);
                }
                else if (Bits == 192)
                {
                    byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(24), pdb.GetBytes(16));
                    return Convert.ToBase64String(encryptedData);
                }
                else if (Bits == 256)
                {
                    byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
                    return Convert.ToBase64String(encryptedData);
                }
                else
                {
                    return String.Concat(Bits);
                }

            }
            private byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
            {
                MemoryStream ms = new MemoryStream();
                Rijndael alg = Rijndael.Create();
                alg.Key = Key;
                alg.IV = IV;
                CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(cipherData, 0, cipherData.Length);
                cs.Close();
                byte[] decryptedData = ms.ToArray();
                return decryptedData;
            }
            public static int enviarCorreo(String Correo, String body, String subject)
            {
                try
                {
                    MailMessage correo = new MailMessage();
                    correo.From = new MailAddress("soportePulli@gmail.com", "No-Responder", System.Text.Encoding.UTF8);//Correo de salida
                    foreach (var address in Correo.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        correo.To.Add(address);
                    }
                    correo.Subject = subject; //Asunto
                    correo.Body = body; //Mensaje del correo
                    correo.IsBodyHtml = true;
                    correo.Priority = MailPriority.Normal;
                    SmtpClient smtp = new SmtpClient();
                    smtp.UseDefaultCredentials = false;
                    smtp.Host = "smtp.gmail.com"; //Host del servidor de correo
                    smtp.Port = 587; //Puerto de salida
                    smtp.Credentials = new System.Net.NetworkCredential("soportepulli@gmail.com", "lsdmsjpvyclntasw");//Cuenta de correo
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };
                    smtp.EnableSsl = true;//True si el servidor de correo permite ssl
                    smtp.Send(correo);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
                return 0;
            }
            public static int enviarCorreoConAdjunto(String Correo, String body, String subject, long empresa)
            {
                try
                {
                    MailMessage correo = new MailMessage();
                    correo.From = new MailAddress("soportePulli@gmail.com", "No -Responder", System.Text.Encoding.UTF8);//Correo de salida
                    correo.To.Add(Correo); //Correo destino?
                    correo.Subject = subject; //Asunto
                    correo.Body = body; //Mensaje del correo
                    correo.IsBodyHtml = true;
                    correo.Priority = MailPriority.Normal;
                    String rutaArchivo = generarArchivo(empresa.ToString(), Environment.CurrentDirectory, empresa);
                    correo.Attachments.Add(new Attachment(rutaArchivo));
                    SmtpClient smtp = new SmtpClient();
                    smtp.UseDefaultCredentials = false;
                    smtp.Host = "smtp.gmail.com"; //Host del servidor de correo
                    smtp.Port = 587; //Puerto de salida
                    smtp.Credentials = new System.Net.NetworkCredential("soportePulli@gmail.com", "lsdmsjpvyclntasw");//Cuenta de correo
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) { return true; };
                    smtp.EnableSsl = true;//True si el servidor de correo permite ssl
                    smtp.Send(correo);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
                return 0;
            }
        
       
            private static String corregirFecha(String date)
            {
                String[] sep = date.Split("-");
                return sep[0] + "/" + remplazarMes(sep[1]) + "/" + sep[2];
            }
            private static String remplazarMes(String Mes)
            {
                String nuevo = "";
                switch (Mes)
                {
                    case "1":
                        return "Enero";

                    case "2":
                        return "Febrero";
                    case "3":
                        return "Marzo";
                    case "4":
                        return "Abril";
                    case "5":
                        return "Mayo";
                    case "6":
                        return "Junio";
                    case "7":
                        return "Julio";
                    case "8":
                        return "Agosto";
                    case "9":
                        return "Septiembre";
                    case "10":
                        return "Octubre";
                    case "11":
                        return "Noviembre";
                    case "12":
                        return "Diciembre";


                }
                return nuevo;
            }
            private static RestClient client = new RestClient("https://farmapagogt.azurewebsites.net/api");
        
            private static string generarArchivo(String Empresa, String rutaPrincipal, long empresa)
            {


                string nameGenerico = Empresa + "_" + "movimientos_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString();
                string extension = ".xlsx";
                string name = nameGenerico + extension;
                string targetFileName = $"{rutaPrincipal}/{name}";
             //   generarExcel(targetFileName, empresa);
                return targetFileName;
            }
            public string Decrypt(string Data, string Password, int Bits)
            {
                try
                {
                    byte[] cipherBytes = Convert.FromBase64String(Data);
                    PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, new byte[] { 0x0, 0x1, 0x2, 0x1C, 0x1D, 0x1E, 0x3, 0x4, 0x5, 0xF, 0x20, 0x21, 0xAD, 0xAF, 0xA4 });
                    if (Bits == 128)
                    {
                        byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(16), pdb.GetBytes(16));
                        return System.Text.Encoding.Unicode.GetString(decryptedData);
                    }
                    else if (Bits == 192)
                    {
                        byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(24), pdb.GetBytes(16));
                        return System.Text.Encoding.Unicode.GetString(decryptedData);
                    }
                    else if (Bits == 256)
                    {
                        byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
                        return System.Text.Encoding.Unicode.GetString(decryptedData);
                    }
                    else
                    {
                        return String.Concat(Bits);
                    }
                }
                catch (Exception ex)
                {
                    return String.Concat(Bits);
                }
            }

            
        }
    }


