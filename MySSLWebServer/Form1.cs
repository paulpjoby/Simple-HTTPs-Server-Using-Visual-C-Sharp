using System.Collections;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MySSLWebServer
{
    public partial class Form1 : Form
    {
        private static String rootPathUrl = "D:/SSL_Server";
        private static String certPath = "";
        private static String wwwPath = "";
        private static X509Certificate2 sslCertificate;
        private static bool isReadingDetailsComplete = false;
        private Thread thread;
        private IDictionary<String, String> fileTypes = new Dictionary<String, String>() {

            {".txt", "text/plain; charset=utf-8"},
            {".png", "image/png"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".gif", "image/gif"},
            {".html", "text/html; charset=utf-8"},
            {".htm", "text/html; charset=utf-8"},
            {".json", "application/json; charset=utf-8"},
            {".xml", "application/xml; charset=utf-8"},

        };


        public Form1()
        {
            InitializeComponent();
        }

        private void DisplayLogs(TcpClient client, SslStream stream)
        {
            if (client == null || stream == null) { return; }
            try
            {
                textBox1.Text += "\r\n" + "\r\n";
                textBox1.Text += client.Connected.ToString() + "\r\n";
                textBox1.Text += client.ReceiveTimeout.ToString() + "\r\n";
                textBox1.Text += client.SendTimeout.ToString() + "\r\n";
                textBox1.Text += client.SendBufferSize.ToString() + "\r\n";
                textBox1.Text += client.ReceiveBufferSize.ToString() + "\r\n";
                textBox1.Text += "\r\n";

                textBox1.Text += "\r\n" + "Cipher: " + stream.CipherAlgorithm + "Strength: "
                + stream.CipherStrength.ToString();
                textBox1.Text += "\r\n" + "Hash: " + stream.HashAlgorithm + "strength :"
                    + stream.HashStrength.ToString();
                textBox1.Text += "\r\n" + "Key exchange: " + stream.KeyExchangeAlgorithm.ToString()
                    + " strength " + stream.KeyExchangeStrength;
                textBox1.Text += "\r\n" + "Protocol: "
                    + stream.SslProtocol.ToString();

                textBox1.Text += "\r\n" + "Is authenticated: " + stream.IsAuthenticated.ToString()
                + " as server? " + stream.IsServer.ToString();
                textBox1.Text += "\r\n" + "IsSigned: " + stream.IsSigned.ToString();
                textBox1.Text += "\r\n" + "Is Encrypted: " + stream.IsEncrypted.ToString();
                textBox1.Text += "\r\n" + "Is mutually authenticated: "
                    + stream.IsMutuallyAuthenticated.ToString();

                textBox1.Text += "\r\n" + "Can read: " + stream.CanRead
                    + " write: " + stream.CanWrite.ToString();
                textBox1.Text += "\r\n" + "Can timeout: " + stream.CanTimeout.ToString();
            }
            catch (Exception ex)
            {

            }
        }

        private void prepareHttpResponse(String resourceUrl, SslStream sslStream)
        {
            // https://localhost.com/some_page.html ---> /some_page.html
            // https://localhost.com/some/ ---> /some/index.html ---> /www/some/index.html
            String actualPath = wwwPath;
            int responseCode = 200;
            byte[] outputPayload;
            String? fileType = null;

            if (resourceUrl == null
                || resourceUrl.Trim().Equals("")
                || resourceUrl.Trim().Equals("/")
                )
            {
                actualPath += "/index.html";
            }
            else if (resourceUrl.Trim().EndsWith("/"))
            {
                actualPath += resourceUrl.Trim() + "/index.html";
            }
            else
            {
                actualPath += resourceUrl.Trim();
            }

            if (File.Exists(actualPath))
            {
                var indexOfDot = actualPath.LastIndexOf('.');
                if (indexOfDot > 0)
                {
                    fileType = fileTypes[actualPath.Substring(indexOfDot)];
                }

                outputPayload = fetchFileContent(actualPath);
            }
            else
            {
                responseCode = 404;
                fileType ??= "text/html; charset=utf-8";
                outputPayload = Encoding.ASCII.GetBytes("" +
                    "<html> <head> <title> 404 </title> </head>" +
                    "<body style=\"text-align:center;\" > <h2> Oops ! Page Not Found </h2> </body> ");
            }

            fileType ??= "text/plain; charset=utf-8";

            byte[] headerBytes = Encoding.ASCII.GetBytes(
                    httpHeader(responseCode, fileType, outputPayload.Length + 1));

            byte[] endConnect = Encoding.ASCII.GetBytes("\n");

            // Write the entire HTTP response to the stream;
            if (sslStream != null)
            {
                sslStream.Write(headerBytes, 0, headerBytes.Length);
                sslStream.Write(outputPayload, 0, outputPayload.Length);
                sslStream.Write(endConnect, 0, endConnect.Length);
                sslStream.Flush();
            }

            return;

        }

        //Prepare HTTP Response Header
        private String httpHeader(int responseCode, String contentType, int contentLen)
        {
            String responseHeader = "HTTP/1.1 " + responseCode.ToString()
                + "\nServer: SimpleSSLServer"
                + "\nContent-Type: " + contentType
                + "\nContent-Length: " + contentLen.ToString()
                + "\n\n";

            return responseHeader;
        }

        private byte[] fetchFileContent(String filePath)
        {
            return File.ReadAllBytes(filePath);
        }
        private void processRequest(SslStream sslStream)
        {
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                bytes = sslStream.Read(buffer, 0, buffer.Length);

                Decoder decoder = Encoding.ASCII.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);

                if (messageData.ToString().IndexOf("\r\n\r\n") > -1
                    || messageData.ToString().IndexOf("\n\n") > -1)
                {
                    break;
                }
            }
            while (bytes > 0);

            this.BeginInvoke(new MethodInvoker(delegate
            {
                textBox1.Text += "\r\n" + "Payload Read: "
                      + "\r\n" + messageData.ToString() + "\r\n";
            }));

            // GET /index.html HTTP/1.1
            var idxStart = messageData.ToString().IndexOf("GET ");
            var endIndex = messageData.ToString().IndexOf(" HTTP/1.1");


            String resourceUrl = messageData.ToString().Substring(idxStart + 4, endIndex - 4).Trim();

            prepareHttpResponse(resourceUrl, sslStream);

        }

        private void prepareSslStream(TcpClient client)
        {
            isReadingDetailsComplete = false;
            SslStream sslStream = new SslStream(client.GetStream(), false);

            try
            {
                // TLS 1.0 and 1.1 are deprecated in Chrome, Edge, Firefox and InternetExplorer 11
                SslProtocols protocols = System.Security.Authentication.SslProtocols.Tls
                    | System.Security.Authentication.SslProtocols.Tls11
                    | System.Security.Authentication.SslProtocols.Tls12
                    | System.Security.Authentication.SslProtocols.Tls13;

                sslStream.AuthenticateAsServer(sslCertificate, clientCertificateRequired: false,
                    enabledSslProtocols: protocols,
                    checkCertificateRevocation: false
                    );

                this.BeginInvoke(
                    new MethodInvoker(delegate
                    {

                        DisplayLogs(client, sslStream);
                        isReadingDetailsComplete = true;

                    }));

                sslStream.ReadTimeout = 60000;
                sslStream.WriteTimeout = 60000;

                // Read and Write to SSL Stream here 
                processRequest(sslStream);

                while (!isReadingDetailsComplete) ;

                // Prevent stream from being disposed or closed till the UI thread writes the logs info
                sslStream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                this.BeginInvoke(
                    new MethodInvoker(delegate
                    {

                        textBox1.Text += "\r\n" + ex.Message.ToString();
                        textBox1.Text += "\r\n" + ex.InnerException?.Message.ToString();
                        textBox1.Text += "\r\n";

                    }));
            }
            finally
            {
                sslStream.Close();
                client.Close();
            }
        }
        private void getCertificate()
        {
            try
            {
                var cert = X509Certificate2.CreateFromPemFile(
                     certPath + "/server.crt", certPath + "/server.key"
                    );

                sslCertificate = new X509Certificate2(cert.Export(X509ContentType.Pfx, "password"), "password");
            }
            catch (Exception ex)
            {

            }
        }

        private TcpListener tcpListener;
        private volatile bool keepThreadAlive = true;
        private void Start_Server_Click(object sender, EventArgs e)
        {
            certPath = rootPathUrl + "/certs";
            wwwPath = rootPathUrl + "/www";
            Start_Server.Enabled = false;
            Stop_Server.Enabled = true;

            keepThreadAlive = true;
            textBox1.Text = "";
            getCertificate();
            textBox1.Text += "\r\n" + sslCertificate.GetSerialNumberString();
            textBox1.Text += "\r\n" + sslCertificate.NotAfter.ToString();
            textBox1.Text += "\r\n" + sslCertificate.NotBefore.ToString();
            textBox1.Text += "\r\n";

            thread = new Thread(() =>
            {

                tcpListener = new TcpListener(IPAddress.Any, 443);
                tcpListener.Start();

                while (true && keepThreadAlive)
                {
                    try
                    {
                        TcpClient client = tcpListener.AcceptTcpClient();
                        prepareSslStream(client);
                    }
                    catch (Exception ex) { }

                }
            });

            thread.Start();
        }

        private void Stop_Server_Click(object sender, EventArgs e)
        {
            Start_Server.Enabled = true;
            Stop_Server.Enabled = false;

            try
            {
                keepThreadAlive = false;
                tcpListener.Stop();
            }
            catch (Exception ex) { }
        }
    }
}