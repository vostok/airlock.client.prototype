﻿using System;

namespace Vostok.Airlock
{
    public class AirlockException : Exception
    {
        public AirlockException(string message, RequestSendResult sendResult) : base(message)
        {
            SendResult = sendResult;
        }

        public RequestSendResult SendResult { get; }
    }
}