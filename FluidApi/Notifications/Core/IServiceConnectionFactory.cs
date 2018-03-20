namespace PushSharp.Core
{
    using System;

    public interface IServiceConnectionFactory<TNotification> where TNotification : INotification
    {
        IServiceConnection<TNotification> Create();
    }
}

