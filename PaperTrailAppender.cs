using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarrodA.Log4Net
{
    public class PaperTrailAppender : UdpAppender
    {
        public PaperTrailAppender()
        {
        }

        public PatternLayout Program { get; set; }

        public PatternLayout Component { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                string program = Program == null ? loggingEvent.Domain : Program.Format(loggingEvent),
                    component = Component == null ? loggingEvent.LoggerName : Component.Format(loggingEvent);

                // Message. The message goes after the tag/identity
                string message = RenderLoggingEvent(loggingEvent);

                string timestamp = loggingEvent.TimeStamp.ToString("MMM d HH:mm:ss");

                Byte[] buffer;
                int i = 0;
                char c;

                StringBuilder builder = new StringBuilder();

                while (i < message.Length)
                {
                    // Clear StringBuilder
                    builder.Length = 0;

                    // Write priority
                    builder.AppendFormat("<22>{0} {1} {2}:", timestamp, program, component);

                    for (; i < message.Length; i++)
                    {
                        c = message[i];

                        // Accept only visible ASCII characters and space. See RFC 3164 section 4.1.3
                        if (((int)c >= 32) && ((int)c <= 126))
                        {
                            builder.Append(c);
                        }
                        // If character is newline, break and send the current line
                        else if ((c == '\r') || (c == '\n'))
                        {
                            // Check the next character to handle \r\n or \n\r
                            if ((message.Length > i + 1) && ((message[i + 1] == '\r') || (message[i + 1] == '\n')))
                            {
                                i++;
                            }
                            i++;
                            break;
                        }
                    }

                    // Grab as a byte array
                    buffer = this.Encoding.GetBytes(builder.ToString());

                    this.Client.Send(buffer, buffer.Length, this.RemoteEndPoint);
                }
            }
            catch (Exception e)
            {
                ErrorHandler.Error(
                    "Unable to send logging event to PaperTrail " +
                    this.RemoteAddress.ToString() +
                    " on port " +
                    this.RemotePort + ".",
                    e,
                    ErrorCode.WriteFailure);
            }
        }
    }
}
