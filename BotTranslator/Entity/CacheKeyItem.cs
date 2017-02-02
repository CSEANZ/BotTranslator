﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTranslator.Entity
{
    public class KeyCacheItem
    {
        public string SubscriptionKey { get; set; }
        public string Bearer { get; set; }
        public DateTime Expires { get; set; }
    }
}
