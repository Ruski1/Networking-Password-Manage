﻿using Networking.Packets;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

namespace Networking.Packets
{
    /// <summary>
    /// From user to server operation types
    /// </summary>
    public enum NetworkOperationTypes : UInt16
    {
        SignIn = 0,
        SignUp = 1,
        LogOut = 2,
        Message = 3, // Sending message
		ProfileInformationChange = 4, // Changing Profile Picture
    }

    /// <summary>
    /// for multiple server reponses
    /// </summary>
    public static class NetworkReponse
    {
        /// <summary>
        /// server response codes
        /// </summary>
        public enum ResponseCodes : UInt16
        {
            successful = 0, // operation worked
            WrongPass = 1, // wrong password input (login only so far)
            NotFound = 2, // username doesn't exist (login only)
            UserExists = 3, // username already exists (sign up only)
            AlreadyLogged = 4, // Already logged in to an account (both)
            MessageSend = 5, // sending message to user
            NullOrEmpty = 6, // Username or Password field was empty or null
            InputTooLong = 7, // Input was too long, Username longer then 30 and Password longer then 150
            InvalidOperation = 8, // Operation can not be done at the current time
        }

        /// <summary>
        /// Field Specific Errors
        /// </summary>
        public enum Field : UInt16
        {
            username = 0,
            password = 1,
            both = 2,
        }
    }

    /// <summary>
    /// Packet base
    /// </summary>
    [Serializable]
    public class Packet
    {
        public Packet() { }
    }

    /// <summary>
    /// Request from user to server
    /// </summary>
    [Serializable]
    public class RequestPacket : Packet
    {
        /// <summary>
        /// Requested Operation type from user
        /// </summary>
        public NetworkOperationTypes RequestedOperationType { get; set; }

        /// <summary>
        /// user name sent from user
        /// </summary>
        public String Username { get; set; }

        /// <summary>
        /// Password sent from user (needs to get hashed/encrypted)
        /// </summary>
        public String Password { set; get; }

        /// <summary>
        /// any message sent (mostly anything that doesn't fit into username or password category)
        /// </summary>
        public String Message { set; get; }

		/// <summary>
		/// Profile picture of the user
		/// </summary>
		public Image ProfilePicture { get; set; }

        public RequestPacket() { }

        /// <summary>
        /// Just operation type (mostly for log out or operations that don't need any other input)
        /// </summary>
        /// <param name="requestedOperationType">Operation type</param>
        public RequestPacket(NetworkOperationTypes requestedOperationType)
        {
            RequestedOperationType = requestedOperationType;
        }

        /// <summary>
        /// Operation with a message (Currently only message operations)
        /// </summary>
        /// <param name="requestedOperationType">Operation type</param>
        /// <param name="message">message being sent</param>
        public RequestPacket(NetworkOperationTypes requestedOperationType, String message)
        {
            RequestedOperationType = requestedOperationType;
            Message = message;
        }

        /// <summary>
        /// Sign in or Sign up request
        /// </summary>
        /// <param name="requestedOperationType">Operation Type</param>
        /// <param name="username">username</param>
        /// <param name="password">password</param>
        public RequestPacket(NetworkOperationTypes requestedOperationType, String username, String password)
        {
            RequestedOperationType = requestedOperationType;
            Username = username;
            Password = password;
        }

		/// <summary>
		/// For Profile Picture change currently
		/// </summary>
		/// <param name="NewProfilePicture">Image to change to</param>
		public RequestPacket(Image NewProfilePicture)
		{
			RequestedOperationType = NetworkOperationTypes.ProfileInformationChange;
			ProfilePicture = NewProfilePicture;
		}
	}

    /// <summary>
    /// Server reponse to user
    /// </summary>
    [Serializable]
    public class ResponsePacket : Packet
    {
        /// <summary>
        /// Response type
        /// </summary>
        public NetworkReponse.ResponseCodes Response { get; set; }

        /// <summary>
        /// Field that is being described
        /// </summary>
        public NetworkReponse.Field EntryField { get; set; }

        /// <summary>
        /// Operation being responded to (incase it is needed in a function)
        /// </summary>
        public NetworkOperationTypes ReponseOperation { get; set; }

        /// <summary>
        /// any message that goes with the response
        /// </summary>
        public String ResponseString { get; set; }

        /// <summary>
        /// which User is sending
        /// </summary>
        public MessageObject MessageObject { get; set; }

        /// <summary>
        /// Information for the client e.g. username, usercount and etc
        /// </summary>
        public UserInformationPack currentUser { get; set; }

        public ResponsePacket() { }

        /// <summary>
        /// basic response
        /// </summary>
        /// <param name="response">response type</param>
        /// <param name="ResponseOp">Request that was being responded to</param>
        public ResponsePacket(NetworkReponse.ResponseCodes response, NetworkOperationTypes ResponseOp)
        {
            Response = response;
            ReponseOperation = ResponseOp;
        }

        /// <summary>
        /// Response with a specified field
        /// </summary>
        /// <param name="response">response type</param>
        /// <param name="ErrorField">Specified Field</param>
        /// <param name="ResponseOp">Request that was being responded to</param>
        public ResponsePacket(NetworkReponse.ResponseCodes response, NetworkReponse.Field ErrorField, NetworkOperationTypes ResponseOp)
        {
            Response = response;
            EntryField = ErrorField;
            ReponseOperation = ResponseOp;
        }

        /// <summary>
        /// Basic response plus the information (not done for the one above incase of failed login/signup attempts)
        /// </summary>
        /// <param name="response">response type</param>
        /// <param name="ResponseOp">Request that was being responded to</param>
        /// <param name="CurrentUser">Current User infomation pack</param>
        public ResponsePacket(NetworkReponse.ResponseCodes response, NetworkOperationTypes ResponseOp, UserInformationPack CurrentUser)
        {
            Response = response;
            ReponseOperation = ResponseOp;
            currentUser = CurrentUser;
        }

        /// <summary>
        /// Sending Message Object, automatically sends as "MessageSend" Response code
        /// </summary>
        /// <param name="messageobject">Message Object</param>
        public ResponsePacket(MessageObject messageobject)
        {
            Response = NetworkReponse.ResponseCodes.MessageSend;
            MessageObject = messageobject;
        }

        /// <summary>
        /// Simple response with message (usually just for chat and send all)
        /// </summary>
        /// <param name="response">response type</param>
        /// <param name="message">message being sent</param>
        public ResponsePacket(NetworkReponse.ResponseCodes response, String message)
        {
            Response = response;
            ResponseString = message;
        }

        /// <summary>
        /// basic response but with text incase of dynamic error message
        /// </summary>
        /// <param name="response">response type</param>
        /// <param name="ResponseOp">Request that was being responded to</param>
        /// <param name="SentText">extra info</param>
        public ResponsePacket(NetworkReponse.ResponseCodes response, NetworkOperationTypes ResponseOp, String SentText)
        {
            Response = response;
            ReponseOperation = ResponseOp;
            ResponseString = SentText;
        }
    }

    [Serializable]
    public class MessageObject
    {
        public String Username { get; set; }
        public String Message { get; set; }
        public Image ProfilePicture { get; set; }

        public MessageObject() { }

        /// <summary>
        /// Basic user and message sending
        /// </summary>
        /// <param name="user">user that is sending</param>
        /// <param name="message">message sent</param>
        public MessageObject(String user, String message, Image profilePicture = null)
        {
            Username = user;
            Message = message;
            ProfilePicture = profilePicture;
        }

        /// <summary>
        /// Sending just message incase error
        /// </summary>
        /// <param name="message">Whole message with error</param>
        public MessageObject(String message)
        {
            Message = message;
        }
    }


    /// <summary>
    /// Information for user
    /// </summary>
    [Serializable]
    public class UserInformationPack
    {
        /// <summary>
        /// User's username
        /// </summary>
        public String Username { get; set; }

        public UserInformationPack() { }

        /// <summary>
        /// Username Limited UserInformationPack
        /// </summary>
        /// <param name="username">Client's username</param>
        public UserInformationPack(String username)
        {
            Username = username;
        }
    }


}

namespace Networking.CustomNetObjects
{
    /// <summary>
    /// Custom Network stream with object sending ablities
    /// </summary>
    public class ObjectNetworkStream : NetworkStream
    {
        private readonly BinaryFormatter _bFormatter = new BinaryFormatter();

        public ObjectNetworkStream(Socket socket) : base(socket)
        {
        }
        public ObjectNetworkStream(Socket socket, bool ownsSocket) : base(socket, ownsSocket)
        {
        }
        public ObjectNetworkStream(Socket socket, FileAccess access) : base(socket, access)
        {
        }
        public ObjectNetworkStream(Socket socket, FileAccess access, bool ownsSocket) : base(socket, access, ownsSocket)
        {
        }

        /// <summary>
        /// Send Object over network
        /// </summary>
        /// <param name="ObjectToSend">Send Object Over Network</param>
        public void Write(Object ObjectToSend)
        {
            _bFormatter.Serialize(this, ObjectToSend);
        }
    }

    /// <summary>
    /// Custom SSL Stream object that allows for using objects over network
    /// </summary>
    public class ObjectSSLStream : SslStream
    {
        private readonly BinaryFormatter _bFormatter = new BinaryFormatter();
        public ObjectSSLStream(Stream innerStream) : base(innerStream) { }

        public ObjectSSLStream(Stream innerStream, Boolean leaveInnerStreamOpen) : base(innerStream, leaveInnerStreamOpen) { }

        public ObjectSSLStream(Stream innerStream, Boolean leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback) : base(innerStream, leaveInnerStreamOpen, userCertificateValidationCallback) { }

        public ObjectSSLStream(Stream innerStream, Boolean leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback) : base(innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, userCertificateSelectionCallback) { }

        public ObjectSSLStream(Stream innerStream, Boolean leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback, EncryptionPolicy encryptionPolicy) : base(innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, userCertificateSelectionCallback, encryptionPolicy) { }

        /// <summary>
        /// Send Object over network
        /// </summary>
        /// <param name="ObjectToSend">Send Object Over Network</param>
        public void Write(Object ObjectToSend)
        {
            _bFormatter.Serialize(this, ObjectToSend);
        }
    }

    /// <summary>
    /// Custom TCP Client Object with network stream compatibility
    /// </summary>
    public class ObjectTcpClient : TcpClient
    {
        private ObjectNetworkStream ObjStream;

        public ObjectTcpClient() { }
        public ObjectTcpClient(IPEndPoint localEP) : base(localEP) { }
        public ObjectTcpClient(AddressFamily family) : base(family) { }
        public ObjectTcpClient(string hostname, int port) : base(hostname, port) { }

        internal ObjectTcpClient(Socket acceptedSocket)
        {
            Client = acceptedSocket;
        }

        /// <summary>
        /// Gives ObjectNetworkStream
        /// </summary>
        /// <returns>ObjectNetworkStream</returns>
        /// <exception cref="InvalidOperationException">not connected</exception>
        public new ObjectNetworkStream GetStream()
        {
            if (!Client.Connected)
            {
                throw new InvalidOperationException("net_notconnected");
            }

            if (ObjStream == null)
            {
                ObjStream = new ObjectNetworkStream(Client, ownsSocket: true);
            }

            return ObjStream;
        }
    }

    /// <summary>
    /// Custom TCP listener Object for outputing ObjectTCPClient
    /// </summary>
    public class ObjectTcpListener : TcpListener
    {
        public ObjectTcpListener(IPEndPoint localEP) : base(localEP) { }
        public ObjectTcpListener(int port) : base(port) { }
        public ObjectTcpListener(IPAddress localaddr, int port) : base(localaddr, port) { }

        /// <summary>
        /// Ouputs ObjectTcpClient instead of TcpClient
        /// </summary>
        /// <returns>ObjectTcpClient</returns>
        /// <exception cref="InvalidOperationException">idk</exception>
        public new ObjectTcpClient AcceptTcpClient()
        {
            if (!this.Active)
            {
                throw new InvalidOperationException("net_stopped");
            }

            Socket acceptedSocket = this.Server.Accept();
            ObjectTcpClient tcpClient = new ObjectTcpClient(acceptedSocket);

            return tcpClient;
        }
    }
}

namespace Networking.Utils
{
    public static class Utils
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// check if form or control is focused
        /// </summary>
        /// <param name="handle">Control handle</param>
        /// <returns>true if focused, false if not</returns>
        public static bool IsActive(IntPtr handle)
        {
            IntPtr activeHandle = GetForegroundWindow();
            return (activeHandle == handle);
        }

        public static string HexDump(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            int bytesLength = bytes.Length;

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - 2) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = asciiSymbol(b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
        }
        static char asciiSymbol(byte val)
        {
            if (val < 32) return '.';  // Non-printable ASCII
            if (val < 127) return (char)val;   // Normal ASCII
                                               // Handle the hole in Latin-1
            if (val == 127) return '.';
            if (val < 0x90) return "€.‚ƒ„…†‡ˆ‰Š‹Œ.Ž."[val & 0xF];
            if (val < 0xA0) return ".‘’“”•–—˜™š›œ.žŸ"[val & 0xF];
            if (val == 0xAD) return '.';   // Soft hyphen: this symbol is zero-width even in monospace fonts
            return (char)val;   // Normal Latin-1
        }
		public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
		{
			var ratioX = (double)maxWidth / image.Width;
			var ratioY = (double)maxHeight / image.Height;
			var ratio = Math.Min(ratioX, ratioY);

			var newWidth = (int)(image.Width * ratio);
			var newHeight = (int)(image.Height * ratio);

			var newImage = new Bitmap(newWidth, newHeight);

			using (var graphics = Graphics.FromImage(newImage))
				graphics.DrawImage(image, 0, 0, newWidth, newHeight);

			return newImage;
		}
	}
}

namespace Networking.Controls
{
    [ComVisible(true)]
    [DefaultProperty("Text")]
    public class LabelButton : Label
    {
        private System.Windows.Forms.Timer FadeOutTimer = new System.Windows.Forms.Timer(), FadeInTimer = new System.Windows.Forms.Timer();

        private UInt16 opacity = 0;

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(0)]
        public UInt16 Opacity
        {
            get
            {
                return opacity;
            }
            set
            {
                opacity = value;
            }
        }

        public LabelButton()
        {
            this.FadeOutTimer.Interval = 1;
            this.FadeInTimer.Interval = 1;

            this.MouseLeave += new EventHandler(this.LabelButton_MouseLeave);
            this.MouseHover += new EventHandler(this.LabelButton_MouseHover);

            this.FadeInTimer.Tick += FadeIn;
            this.FadeOutTimer.Tick += FadeOut;

            this.Cursor = Cursors.Hand;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.ForeColor = Color.FromArgb(150, 150, 150);
            this.Font = new Font("Helvetica Rounded", 13F, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.BackColor = Color.FromArgb(opacity, 160, 120, 230);
            this.AutoSize = false;
            this.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void RedrawBackground()
        {
            this.BackColor = Color.FromArgb(opacity, 160, 120, 230);
        }

        private void LabelButton_MouseHover(Object sender, EventArgs e)
        {
            this.FadeOutTimer.Stop();
            this.FadeInTimer.Start();
        }

        private void LabelButton_MouseLeave(Object sender, EventArgs e)
        {
            this.FadeInTimer.Stop();
            this.FadeOutTimer.Start();
        }

        private void FadeIn(Object Sender, EventArgs e)
        {
            if (opacity >= 150)
            {
                FadeOutTimer.Stop();
            }
            else
            {
                opacity += 30;
            }
            RedrawBackground();
        }

        private void FadeOut(Object Sender, EventArgs e)
        {
            if (opacity <= 1)
            {
                FadeOutTimer.Stop();
            }
            else
            {
                opacity -= 15;
            }
            RedrawBackground();
        }
    }

    public class FlowLayoutConsolePanel : FlowLayoutPanel
    {
        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;

        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == WM_HSCROLL || m.Msg == WM_VSCROLL)
            && (((int)m.WParam & 0xFFFF) == 5))
            {
                // Change SB_THUMBTRACK to SB_THUMBPOSITION
                m.WParam = (IntPtr)(((int)m.WParam & ~0xFFFF) | 4);
            }
            base.WndProc(ref m);
        }

        public FlowLayoutConsolePanel()
        {
            this.ControlAdded += new ControlEventHandler(this.FlowLayoutConsolePanel_ControlAdded);
        }

        public void Write(MessageObject Message)
        {
            this.Controls.Add(this.GenerateMessagePanel(Message));
        }

        public Panel GenerateMessagePanel(MessageObject Message)
        {
            Panel MessageContainerPanel = new Panel();
            PictureBox ProfilePictureBox = new PictureBox();
            Label UsernameLabel = new Label();
            Label MessageLabel = new Label();

            // Profile Picture Box
            ProfilePictureBox.Size = new Size(50, 50);
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, ProfilePictureBox.Width - 3, ProfilePictureBox.Height - 3);
            Region rg = new Region(gp);
            ProfilePictureBox.Region = rg;
            ProfilePictureBox.Image = Message.ProfilePicture;
            ProfilePictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            //Username Label
            UsernameLabel.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            UsernameLabel.ForeColor = Color.FromArgb(0, 192, 0);
            UsernameLabel.Text = Message.Username;
            UsernameLabel.Location = new Point(50, 0);

            //Message Label
            MessageLabel.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MessageLabel.ForeColor = Color.FromArgb(0, 192, 0);
            MessageLabel.Text = Message.Message;
            MessageLabel.Location = new Point(50, 30);

            //Message Container Panel
            MessageContainerPanel.Size = new Size(780, 80);
            MessageContainerPanel.Controls.Add(ProfilePictureBox);
            MessageContainerPanel.Controls.Add(UsernameLabel);
            MessageContainerPanel.Controls.Add(MessageLabel);

            return MessageContainerPanel;
        }

        private void FlowLayoutConsolePanel_ControlAdded(object sender, ControlEventArgs e)
        {
            this.ScrollControlIntoView(e.Control);
        }
    }
}