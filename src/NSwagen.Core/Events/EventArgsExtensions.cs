using System;

namespace NSwagen.Core.Events
{
    public static class EventArgsExtensions
    {
#pragma warning disable CA1030 // Use events where appropriate
        public static void FireStatusEvent<TType>(this object caller,
            EventHandler<StatusEventArgs<TType>>? handler,
            TType statusType,
            object? metadata = null,
            string? message = null)
            where TType : Enum
        {
            var handlerCopy = handler;
            if (handlerCopy != null)
            {
                var args = new StatusEventArgs<TType>(statusType, metadata, message);
                handlerCopy(caller, args);
            }
        }

        public static void Fire<TType>(this EventHandler<StatusEventArgs<TType>>? handler,
            TType statusType,
            object? metadata = null,
            string? message = null)
            where TType : Enum
        {
            var handlerCopy = handler;
            if (handlerCopy != null)
            {
                var args = new StatusEventArgs<TType>(statusType, metadata, message);
                handlerCopy(null, args);
            }
        }

        //public static void Fire<TType>(this EventHandler<ProgressEventArgs<TType>>? handler,
        //    TType progressType,
        //    int totalRecord,
        //    int progress,
        //    string? message = null)
        //    where TType : Enum
        //{
        //    var handlerCopy = handler;
        //    if (handlerCopy != null)
        //    {
        //        var args = new ProgressEventArgs<TType>(progressType, totalRecord, progress, message);
        //        handlerCopy(null, args);
        //    }
        //}
    }
}
#pragma warning restore CA1030 // Use events where appropriate
