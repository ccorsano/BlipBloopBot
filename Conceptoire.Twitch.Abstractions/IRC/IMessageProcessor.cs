using System;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    /// <summary>
    /// Twitch IRC Bot Message processor interface
    /// </summary>
    public interface IMessageProcessor
    {
        bool CanHandleMessage(in ParsedIRCMessage message);
        /// <summary>
        /// Called when the Twitch IRC context changes.
        /// 
        /// Allows for asynchronous operations: 
        /// processor is meant to react and store relevant information
        /// </summary>
        /// <param name="context">Updated context</param>
        Task OnUpdateContext(IProcessorContext context);
        
        /// <summary>
        /// Called when a message is received and forwarded to the processor.
        /// 
        /// This is called using stack-based value types during message processing.
        /// Any heavy operations or asynchronous calls needs to be triggered outside of this method.
        /// </summary>
        /// <param name="message">Parsed Twitch IRC message</param>
        /// <param name="sendResponse">Outgoing messages to push to the reply queue</param>
        void OnMessage(in ParsedIRCMessage message, Action<OutgoingMessage> sendResponse);
    }
}
