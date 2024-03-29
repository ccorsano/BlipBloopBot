﻿using Conceptoire.Twitch.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public interface IProcessorSettings
    {
        public string BroadcasterId { get; set; }
        void LoadFromOptions(CommandOptions options);
        void SaveToOptions(CommandOptions options);
    }
}