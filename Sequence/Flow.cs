using Common;
using Common.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace Sequence
{
    /// <summary>
    /// flow program sequence
    /// </summary>
    public class Flow : IRecipient<MainWindowRenderedMessage>, IRecipient<MainViewCloseMessage>
    {
        private readonly Log _log;

        public Flow(Log log)
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
            _log = log;
        }

        public async void Receive(MainWindowRenderedMessage message)
        {
            try
            {
                //do init
                
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new DialogMessage("init error", ex.Message));
                _log.Write(ex.Message);
            }
            finally
            {
                WeakReferenceMessenger.Default.Send(new BusyMessage(false)); //close wait
            }
        }

        public async void Receive(MainViewCloseMessage message)
        {
            WeakReferenceMessenger.Default.Send(new BusyMessage(true, "exit..."));
            try
            {
                //do dispose

                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new DialogMessage("dispose error", ex.Message));
                _log.Write(ex.Message);
            }
        }
    }
}
