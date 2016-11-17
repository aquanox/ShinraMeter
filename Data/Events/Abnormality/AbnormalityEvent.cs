﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tera.Game;

namespace Data.Events.Abnormality
{
    public class AbnormalityEvent : Event
    {
        public List<int> Ids { get; set; }

      
        public int RemainingSecondBeforeTrigger { get; set; }
        public int RewarnTimeoutSeconds { get; set; }
        public List<HotDot.Types> Types { get; set; }
        public AbnormalityTargetType Target { get; set; }
        public AbnormalityTriggerType Trigger { get; set; }
        public AbnormalityEvent(bool inGame, bool active, List<int> ids, List<HotDot.Types> types, AbnormalityTargetType target, AbnormalityTriggerType trigger, int remainingSecondsBeforeTrigger, int rewarnTimeoutSecounds): base(inGame, active)
        {
            Types = types;
            Ids = ids;
            Target = target;
            Trigger = trigger;
            RemainingSecondBeforeTrigger = remainingSecondsBeforeTrigger;
            RewarnTimeoutSeconds = rewarnTimeoutSecounds;
        }
    }
}
