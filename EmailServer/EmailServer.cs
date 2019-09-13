using MailKit.Net.Smtp;
using MimeKit;
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.Mail;
using SmtpServer.Net;
using SmtpServer.Storage;

namespace EmailServer
{
    class EmailServer
    {
        private static EmailServer singleton;

        private EmailServer()
        {
            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Port(25, 587)
                .Build();

            var smtpServer = new SmtpServer.SmtpServer(options);
             
        }

        public EmailServer GetInstance()
        {
            singleton = singleton ?? new EmailServer();
            return singleton;
        }
    }

    class EmailAuthenticator : UserAuthenticator
    {
        /// <summary>
        /// Authenticate a user account.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="user">The user to authenticate.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true if the user is authenticated, false if not.</returns>
        public override Task<bool> AuthenticateAsync(
            ISessionContext context,
            string user,
            string password,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("[" + user + "|" + password + "]");
            return Task.FromResult(user == "user" && password == "password");
        }
    }

    public class EmailMessageStore : MessageStore
    {
        /// <summary>
        /// Save the given message to the underlying storage system.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="transaction">The SMTP message transaction to store.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A unique identifier that represents this message in the underlying message store.</returns>
        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
        {
            var textMessage = (ITextMessage)transaction.Message;

            using (var reader = new StreamReader(textMessage.Content, Encoding.UTF8))
            {
                Console.WriteLine(reader.ReadToEnd());
            }

            return Task.FromResult(SmtpResponse.Ok);
        }
    }

    public class SampleMailboxFilter : MailboxFilter
    {
        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="from">The mailbox to test.</param>
        /// <param name="size">The estimated message size to accept.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public override Task<MailboxFilterResult> CanAcceptFromAsync(
            ISessionContext context,
            IMailbox @from,
            int size,
            CancellationToken cancellationToken)
        {
            if (@from == Mailbox.Empty)
            {
                return Task.FromResult(MailboxFilterResult.NoPermanently);
            }

            var endpoint = (IPEndPoint)context.Properties[EndpointListener.RemoteEndPointKey];

            if (endpoint.Address.Equals(IPAddress.Parse("127.0.0.1")))
            {
                return Task.FromResult(MailboxFilterResult.Yes);
            }

            return Task.FromResult(MailboxFilterResult.NoPermanently);
        }

        /// <summary>
        /// Returns a value indicating whether the given mailbox can be accepted as a recipient to the given sender.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="to">The mailbox to test.</param>
        /// <param name="from">The sender's mailbox.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The acceptance state of the mailbox.</returns>
        public override Task<MailboxFilterResult> CanDeliverToAsync(
            ISessionContext context,
            IMailbox to,
            IMailbox @from,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(MailboxFilterResult.Yes);
        }
    }

}